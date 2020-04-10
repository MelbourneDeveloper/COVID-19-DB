using Covid19DB.Entities;
using Covid19DB.Repositories;
using Microsoft.VisualBasic.FileIO;
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

            var modelsByDate = new List<List<RawModel>>();

            //Iterate through the files
            foreach (var fileName in Directory.GetFiles(dailyReportsFolder, "*.csv"))
            {
                //Get the date
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var numbers = fileNameWithoutExtension.Split('-').Select(t => int.Parse(t)).ToList();
                var date = new DateTimeOffset(numbers[2], numbers[0], numbers[1], 0, 0, 0, default);

                var rawModels = ProcessFile(fileName, date);

                modelsByDate.Add(rawModels);
            }

            using (var covid19DbContext = new Covid19DbContext())
            {
                var provinceRepository = new ProvinceRepository(covid19DbContext);
                var regionRepository = new RegionRepository(covid19DbContext);
                var locationRepository = new LocationRepository(covid19DbContext);
                var locationDayRepository = new LocationDayRepository(covid19DbContext);

                var processor = new Processor(provinceRepository, regionRepository, locationRepository, locationDayRepository, modelsByDate.Aggregate((a, b) =>
                {
                    var aggregatedList = new List<RawModel>(a);
                    aggregatedList.AddRange(b);
                    return aggregatedList;
                }));

                processor.Process();

                covid19DbContext.SaveChanges();
            }
        }

        private static List<RawModel> ProcessFile(string fileName, DateTimeOffset date)
        {
            using (var parser = new TextFieldParser(fileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                var headerNames = parser.ReadFields().ToList();

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

                var admin2Index = headerNames.IndexOf(nameof(RawModel.Admin2));

                var recoveredIndex = headerNames.IndexOf(nameof(RawModel.Recovered));


                var rawModels = new List<RawModel>();

                //Number is 1 based and matches tyhe Github line
                var i = 2;

                //Iterate through the lines in the file
                while (!parser.EndOfData)
                {
                    var tokens = parser.ReadFields().ToList();

                    if (tokens.Count != headerNames.Count)
                    {
                        throw new Exception($"Filename: {fileName} Headers: {headerNames.Count} Tokens: {tokens.Count} Line: {i + 1}");
                    }

                    var rawModel = ProcessRow(date, confirmedIndex, deathsIndex, countryRegionIndex, provinceStateIndex, latitudeIndex, longitudeIndex, admin2Index, recoveredIndex, tokens, headerNames);

                    if (rawModel != null) rawModels.Add(rawModel);

                    i++;
                }

                return rawModels;
            }
        }

        private static RawModel ProcessRow(DateTimeOffset date, int confirmedIndex, int deathsIndex, int countryRegionIndex, int provinceStateIndex, int latitudeIndex, int longitudeIndex, int admin2Index, int recoveredIndex, List<string> tokens, List<string> headerNames)
        {
            var confirmedText = tokens[confirmedIndex];
            var deathsText = tokens[deathsIndex];
            var recoveredText = tokens[recoveredIndex];

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

            if (string.IsNullOrEmpty(confirmedText) && string.IsNullOrEmpty(deathsText) && string.IsNullOrEmpty(recoveredText)) return null;

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

            string admin2Text = null;
            if (admin2Index > -1)
            {
                admin2Text = tokens[admin2Index];
            }

            return new RawModel
            {
                Confirmed = !string.IsNullOrEmpty(confirmedText) ? int.Parse(confirmedText) : (int?)null,
                Deaths = !string.IsNullOrEmpty(deathsText) ? int.Parse(deathsText) : (int?)null,
                Recovered = !string.IsNullOrEmpty(recoveredText) ? int.Parse(recoveredText) : (int?)null,
                Country_Region = tokens[countryRegionIndex],
                Province_State = tokens[provinceStateIndex],
                Lat = latitude,
                Long_ = longitude,
                Date = date,
                Admin2 = admin2Text
            };
        }
    }
}
