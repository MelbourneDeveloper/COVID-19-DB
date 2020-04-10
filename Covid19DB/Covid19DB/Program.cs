using Covid19DB.Entities;
using Covid19DB.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Covid19DB
{

    class Program
    {
        private const string EmptyValue = "N/A";
        private const string None = "NONE";
        private static Dictionary<Guid, int?> ConfirmedCasesByLocation = new Dictionary<Guid, int?>();
        private static IServiceCollection ServiceCollection = new ServiceCollection();
        private static ICache<Region> regionsByName = new Cache<Region>();
        private static ICache<Province> provincesByRegionAndName = new Cache<Province>();
        private static ICache<Location> locationsByRegionProvinceName = new Cache<Location>();

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

                var rawModels = ProcessFile(fileName, date);

                modelsByDate.Add(date, rawModels);
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

            using (var covid19DbContext = new Covid19DbContext())
            {
                ProcessAll(modelsByDate, regionGroupings, provinceGroupings, locationGroupings, regionsByName, locationsByRegionProvinceName, covid19DbContext);
            }
        }

        private static void ProcessAll(Dictionary<DateTimeOffset, List<RawModel>> modelsByDate, List<IGrouping<string, RawModel>> regionGroupings, List<IGrouping<string, RawModel>> provinceGroupings, List<IGrouping<string, RawModel>> locationGroupings, ICache<Region> regionsByName, ICache<Location> locationsByRegionProvinceName, Covid19DbContext covid19DbContext)
        {
            IProvinceRepository provinceRepository = new ProvinceRepository(covid19DbContext);
            IRegionRepository regionRepository = new RegionRepository(covid19DbContext);
            ILocationRepository locationRepository = new LocationRepository(covid19DbContext);

            //Add any missing regions
            foreach (var regionGrouping in regionGroupings)
            {
                var regionName = regionGrouping.Key;
                var region = GetRegion(regionRepository, regionName);
                regionsByName.Add(regionGrouping.Key, region);
            }

            //Add any missing provinces
            foreach (var provinceGrouping in provinceGroupings)
            {
                var rawModel = provinceGrouping.First();
                var region = regionsByName.Get(rawModel.Country_Region);

                var provinceName = provinceGrouping.Key;

                var province = GetProvince(provinceRepository, provinceName, region.Id);
                provincesByRegionAndName.Add(GetProvinceKey(region.Name, provinceName), province);
            }

            //Add any missing locations
            foreach (var locationGrouping in locationGroupings)
            {
                var rawModel = locationGrouping.First();
                var region = regionsByName.Get(rawModel.Country_Region);
                var province = GetProvince(provinceRepository, rawModel.Province_State, region.Id);

                var location = GetLocation(locationRepository, rawModel.Admin2, rawModel.Lat, rawModel.Long_, province);

                locationsByRegionProvinceName.Add(GetLocationKey(region.Name, province.Name, location.Name), location);
            }

            foreach (var key in modelsByDate.Keys.OrderBy(k => k))
            {
                var rawModels = modelsByDate[key];

                foreach (var rawModel in rawModels)
                {
                    var locationKey = GetLocationKey(rawModel.Country_Region, rawModel.Province_State, rawModel.Admin2);

                    var location = locationsByRegionProvinceName.Get(locationKey);

                    if (location == null)
                    {
                        //Location is empty

                        location = new Location
                        {
                            Name = EmptyValue
                        };

                        Province province = null;

                        var provinceKey = GetProvinceKey(rawModel.Country_Region, rawModel.Province_State);

                        if (!string.IsNullOrEmpty(rawModel.Province_State))
                        {
                            if (rawModel.Province_State == "From Diamond Princess")
                            {
                                //ISSUE : Naming special case
                                //Deal with Diamond Princess in general

                                province = GetProvince(provinceRepository, "From Diamond Princess", regionsByName.Get(rawModel.Country_Region).Id);
                            }
                            else
                            {
                                if (string.Compare(rawModel.Province_State, None, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    province = provincesByRegionAndName.Get(provinceKey);
                                    if (province == null)
                                    {
                                        province = GetProvince(regionsByName, provincesByRegionAndName, covid19DbContext, rawModel.Country_Region);
                                    }
                                }
                                else
                                {
                                    province = provincesByRegionAndName.Get(provinceKey);
                                    if (province == null)
                                    {
                                        //ISSUE: Something weird here with Hong Kong SAR

                                        //The province was not created so create it
                                        var region = regionsByName.Get(rawModel.Country_Region);
                                        province = GetProvince(provinceRepository, rawModel.Province_State, region.Id);
                                        provincesByRegionAndName.Add(GetProvinceKey(region.Name, rawModel.Province_State), province);
                                    }
                                }
                            }
                        }
                        else
                        {
                            province = provincesByRegionAndName.Get(provinceKey);
                            if (province == null)
                                province = GetEmptyProvince(regionsByName, provincesByRegionAndName, covid19DbContext, rawModel.Country_Region);
                        }

                        location.ProvinceId = province.Id;

                        //Craete a new location with N/A
                        covid19DbContext.Locations.Add(location);
                    }

                    ConfirmedCasesByLocation.TryGetValue(location.Id, out var totalConfirmed);

                    int? currentConfirmed = null;
                    if (totalConfirmed.HasValue)
                    {
                        if (rawModel.Confirmed.HasValue)
                        {
                            currentConfirmed = rawModel.Confirmed - totalConfirmed;
                        }
                        else
                        {
                            //do nothing
                        }
                    }

                    var day = covid19DbContext.Days.FirstOrDefault(d =>
                    d.Date == rawModel.Date &&
                    d.LocationId == location.Id
                    );

                    if (day == null)
                    {
                        day = new Day
                        {
                            Date = rawModel.Date,
                            Cases = currentConfirmed,
                            Deaths = rawModel.Deaths,
                            LocationId = location.Id
                        };

                        covid19DbContext.Days.Add(day);
                    }

                    if (!ConfirmedCasesByLocation.ContainsKey(location.Id)) ConfirmedCasesByLocation.Add(location.Id, rawModel.Confirmed);
                }


            }

            covid19DbContext.SaveChanges();
        }

        private static string GetProvinceKey(string regionName, string provinceName)
        {
            return $"{ReplaceEmpty(regionName)}.{ReplaceEmpty(provinceName)}";
        }

        private static string GetLocationKey(string regionName, string provinceName, string locationName)
        {
            return $"{GetProvinceKey(regionName, provinceName)}.{ReplaceEmpty(locationName)}";
        }

        private static Location GetLocation(ILocationRepository locationRepository, string name, decimal? latitude, decimal? longitude, Province province)
        {
            return locationRepository.GetOrInsert(ReplaceEmpty(name), province.Id, latitude, longitude);
        }

        private static Province GetProvince(IProvinceRepository provinceRepository, string provinceName, Guid regionId)
        {
            return provinceRepository.GetOrInsert(ReplaceEmpty(provinceName), regionId);
        }

        private static string ReplaceEmpty(string name)
        {
            if (
                name == null ||
                string.Compare(name, None, StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(name, string.Empty, StringComparison.OrdinalIgnoreCase) == 0
                ) return EmptyValue;

            return name;
        }

        private static Region GetRegion(IRegionRepository regionRepository, string regionName)
        {
            return regionRepository.GetOrInsert(regionName);
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
