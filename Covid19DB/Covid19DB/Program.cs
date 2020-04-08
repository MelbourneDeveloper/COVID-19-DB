using Covid19DB.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Covid19DB
{
    class Program
    {
        static void Main(string[] args)
        {
            var dailyReportsFolder = @"C:\Code\COVID-19\csse_covid_19_data\csse_covid_19_daily_reports";

            var modelsByDate = new Dictionary<DateTimeOffset, List<RawModel>>();

            //Iterate through the files
            foreach (var fileName in Directory.GetFiles(dailyReportsFolder, "*.csv"))
            {
                //Get the date
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var numbers = fileNameWithoutExtension.Split('-').Select(t => int.Parse(t)).ToList();
                var date = new DateTimeOffset(numbers[2], numbers[0], numbers[1], 0, 0, 0, default);

                var rowModels = ProcessFile(fileName, date);

                modelsByDate.Add(date, rowModels);
            }

            var aggregatedData = modelsByDate.Values.Aggregate((a, b) =>
            {
                var aggregatedList = new List<RawModel>(a);
                aggregatedList.AddRange(b);
                return aggregatedList;
            }).ToList();

            var regionGroupings = aggregatedData.Where(a => !string.IsNullOrEmpty(a.Country_Region)).GroupBy(a => a.Country_Region).ToList();
            var provinces = aggregatedData.Where(a => !string.IsNullOrEmpty(a.Province_State)).GroupBy(a => a.Province_State).ToList();

            using (var covid19DbContext = new Covid19DbContext())
            {
                foreach (var regionGrouping in regionGroupings)
                {
                    var regionEntity = covid19DbContext.Regions.FirstOrDefault(r => r.Name == regionGrouping.Key);
                    if (regionEntity == null)
                    {
                        regionEntity = new Region { Name = regionGrouping.Key };
                        covid19DbContext.Regions.Add(regionEntity);
                    }
                }

                foreach (var key in modelsByDate.Keys.OrderBy(k => k))
                {
                    var rawModel = modelsByDate[key];



                }

                covid19DbContext.SaveChanges();
            }


        }

        private static List<RawModel> ProcessFile(string fileName, DateTimeOffset date)
        {
            //Read the text
            var text = File.ReadAllText(fileName);

            var lines = text.Split("\r\n").ToList();

            //Get the indexes of the columns by header name
            var headerNames = lines[0].Split(',').ToList();

            var confirmedIndex = headerNames.IndexOf(nameof(RawModel.Confirmed));
            var deathsIndex = headerNames.IndexOf(nameof(RawModel.Deaths));

            var countryRegionIndex = headerNames.IndexOf(nameof(RawModel.Country_Region));
            //ISSUE: Deal with inconsistent header names
            if (countryRegionIndex == -1) countryRegionIndex = headerNames.IndexOf("Country/Region");

            var provinceStateIndex = headerNames.IndexOf(nameof(RawModel.Province_State));
            //ISSUE: Deal with inconsistent header names
            if (provinceStateIndex == -1) provinceStateIndex = headerNames.IndexOf("Province/State");

            var latitudeIndex = headerNames.IndexOf(nameof(RawModel.Lat));
            var longitudeIndex = headerNames.IndexOf(nameof(RawModel.Long_));

            var rawModels = new List<RawModel>();

            //Iterate through the lines in the file
            for (var i = 1; i < lines.Count; i++)
            {
                var tokens = lines[i].Split(',').ToList();
                var rawModel = ProcessRow(date, confirmedIndex, deathsIndex, countryRegionIndex, provinceStateIndex, latitudeIndex, longitudeIndex, tokens, headerNames);

                if (rawModel != null) rawModels.Add(rawModel);
            }

            return rawModels;
        }

        private static RawModel ProcessRow(DateTimeOffset date, int confirmedIndex, int deathsIndex, int countryRegionIndex, int provinceStateIndex, int latitudeIndex, int longitudeIndex, List<string> tokens, List<string> headerNames)
        {
            if (tokens.Count != headerNames.Count) return null;

            var confirmedText = tokens[confirmedIndex];
            var deathsText = tokens[deathsIndex];

            string latitudeText = null;
            if (latitudeIndex > -1)
            {
                latitudeText = tokens[latitudeIndex];
            }

            string longitutdeText = null;
            if (longitudeIndex > -1)
            {
                longitutdeText = tokens[longitudeIndex];
            }

            if (string.IsNullOrEmpty(confirmedText) && string.IsNullOrEmpty(deathsText)) return null;

            decimal? latitude = null;
            if (!string.IsNullOrEmpty(latitudeText))
            {
                latitude = decimal.Parse(latitudeText);
            }

            decimal? longitude = null;
            if (!string.IsNullOrEmpty(longitutdeText))
            {
                longitude = decimal.Parse(longitutdeText);
            }

            return new RawModel
            {
                Confirmed = !string.IsNullOrEmpty(confirmedText) ? int.Parse(confirmedText) : (int?)null,
                Deaths = !string.IsNullOrEmpty(deathsText) ? int.Parse(deathsText) : (int?)null,
                Country_Region = tokens[countryRegionIndex],
                Province_State = tokens[provinceStateIndex],
                Lat = latitude,
                Long_ = longitude,
                Date = date
            };
        }
    }
}
