﻿using Covid19DB.Model;
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
            var provinceGroupings = aggregatedData.Where(a => !string.IsNullOrEmpty(a.Province_State)).GroupBy(a => a.Province_State).ToList();
            var locationGroupings = aggregatedData.Where(a => !string.IsNullOrEmpty(a.Admin2)).GroupBy(a => a.Admin2).ToList();

            var regionsByName = new Dictionary<string, Region>(StringComparer.OrdinalIgnoreCase);
            var provincesByRegionName = new Dictionary<string, Province>(StringComparer.OrdinalIgnoreCase);
            var locationsByRegionProvinceName = new Dictionary<string, Location>(StringComparer.OrdinalIgnoreCase);

            using (var covid19DbContext = new Covid19DbContext())
            {
                //Add any missing regions
                foreach (var regionGrouping in regionGroupings)
                {
                    var regionName = regionGrouping.Key;
                    var region = GetRegion(covid19DbContext, regionName);
                    regionsByName.Add(regionGrouping.Key, region);
                }

                //Add any missing provinces
                foreach (var provinceGrouping in provinceGroupings)
                {
                    var rawModel = provinceGrouping.First();
                    var region = regionsByName[rawModel.Country_Region];
                    var province = GetProvince(covid19DbContext, provinceGrouping.Key, region);
                    provincesByRegionName.Add($"{region.Name}.{province.Name}", province);
                }

                //Add any missing locations
                foreach (var locationGrouping in locationGroupings)
                {
                    var rawModel = locationGrouping.First();
                    var region = regionsByName[rawModel.Country_Region];
                    var province = GetProvince(covid19DbContext, rawModel.Province_State, region);

                    var location = GetLocation(covid19DbContext, rawModel, province);

                    locationsByRegionProvinceName.Add($"{region.Name}.{province.Name}.{location.Name}", location);
                }

                foreach (var key in modelsByDate.Keys.OrderBy(k => k))
                {
                    var rawModel = modelsByDate[key];



                }

                covid19DbContext.SaveChanges();
            }


        }

        private static Location GetLocation(Covid19DbContext covid19DbContext, RawModel rawModel, Province province)
        {
            var location = covid19DbContext.Locations.FirstOrDefault(l =>
            l.Name == rawModel.Admin2 &&
            l.ProvinceId == province.Id
            );

            if (location == null)
            {
                location = new Location
                {
                    Name = rawModel.Admin2,
                    ProvinceId = province.Id,
                    Latitude = rawModel.Lat,
                    Longitude = rawModel.Long_
                };
                covid19DbContext.Provinces.Add(province);
            }

            return location;
        }

        private static Province GetProvince(Covid19DbContext covid19DbContext, string provinceName, Region region)
        {
            var province = covid19DbContext.Provinces.FirstOrDefault(r =>
            r.Name == provinceName &&
            r.RegionId == region.Id
            );

            if (province == null)
            {
                province = new Province
                {
                    Name = provinceName,
                    RegionId = region.Id
                };
                covid19DbContext.Provinces.Add(province);
            }

            return province;
        }

        private static Region GetRegion(Covid19DbContext covid19DbContext, string regionName)
        {
            var region = covid19DbContext.Regions.FirstOrDefault(r => r.Name == regionName);
            if (region == null)
            {
                region = new Region { Name = regionName };
                covid19DbContext.Regions.Add(region);
            }

            return region;
        }

        private static List<RawModel> ProcessFile(string fileName, DateTimeOffset date)
        {
            //Read the text
            var text = File.ReadAllText(fileName);

            var lines = text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();

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

            var admin2Index = headerNames.IndexOf(nameof(RawModel.Admin2));

            var rawModels = new List<RawModel>();

            //Iterate through the lines in the file
            for (var i = 1; i < lines.Count; i++)
            {
                var lineText = lines[i];
                var tokens = lineText.Split(',', StringSplitOptions.None).ToList();

                if (tokens.Count != headerNames.Count)
                {
                    throw new Exception($"Filename: {fileName} Headers: {headerNames.Count} Tokens: {tokens.Count} Line: {i + 1}");
                }

                var rawModel = ProcessRow(date, confirmedIndex, deathsIndex, countryRegionIndex, provinceStateIndex, latitudeIndex, longitudeIndex, admin2Index, tokens, headerNames);

                if (rawModel != null) rawModels.Add(rawModel);
            }

            return rawModels;
        }

        private static RawModel ProcessRow(DateTimeOffset date, int confirmedIndex, int deathsIndex, int countryRegionIndex, int provinceStateIndex, int latitudeIndex, int longitudeIndex, int admin2Index, List<string> tokens, List<string> headerNames)
        {
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

            string admin2Text = null;
            if (admin2Index > -1)
            {
                admin2Text = tokens[admin2Index];
            }

            return new RawModel
            {
                Confirmed = !string.IsNullOrEmpty(confirmedText) ? int.Parse(confirmedText) : (int?)null,
                Deaths = !string.IsNullOrEmpty(deathsText) ? int.Parse(deathsText) : (int?)null,
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
