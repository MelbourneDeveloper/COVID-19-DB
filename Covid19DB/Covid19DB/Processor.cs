using Covid19DB.Entities;
using Covid19DB.Repositories;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Covid19DB
{
    public class Processor
    {
        private const string EmptyValue = "N/A";
        private const string None = "NONE";
        private static Dictionary<Guid, int?> ConfirmedCasesByLocation = new Dictionary<Guid, int?>();
        private static ICache<Region> regionsByName = new Cache<Region>();
        private static ICache<Province> provincesByRegionAndName = new Cache<Province>();
        private static ICache<Location> locationsByRegionProvinceName = new Cache<Location>();
        IProvinceRepository _provinceRepository;
        IRegionRepository _regionRepository;
        ILocationRepository _locationRepository;

        public Processor(
        IProvinceRepository provinceRepository,
        IRegionRepository regionRepository,
        ILocationRepository locationRepository
            )
        {
            _provinceRepository = provinceRepository;
            _regionRepository = regionRepository;
            _locationRepository = locationRepository;
        }

        public void ProcessAll(Dictionary<DateTimeOffset, List<RawModel>> modelsByDate, List<IGrouping<string, RawModel>> regionGroupings, List<IGrouping<string, RawModel>> provinceGroupings, List<IGrouping<string, RawModel>> locationGroupings)
        {

            //Add any missing regions
            foreach (var regionGrouping in regionGroupings)
            {
                var regionName = regionGrouping.Key;
                var region = GetRegion(_regionRepository, regionName);
                regionsByName.Add(regionGrouping.Key, region);
            }

            //Add any missing provinces
            foreach (var provinceGrouping in provinceGroupings)
            {
                var rawModel = provinceGrouping.First();
                var region = regionsByName.Get(rawModel.Country_Region);

                var provinceName = provinceGrouping.Key;

                var province = GetProvince(_provinceRepository, provinceName, region);
                provincesByRegionAndName.Add(GetProvinceKey(region.Name, provinceName), province);
            }

            //Add any missing locations
            foreach (var locationGrouping in locationGroupings)
            {
                var rawModel = locationGrouping.First();
                var region = regionsByName.Get(rawModel.Country_Region);
                var province = GetProvince(_provinceRepository, rawModel.Province_State, region);

                var location = GetLocation(_locationRepository, rawModel.Admin2, rawModel.Lat, rawModel.Long_, province);

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

                        var region = GetRegion(_regionRepository, rawModel.Country_Region);

                        var province = GetProvince(_provinceRepository, rawModel.Province_State, region);

                        location = new Location
                        {
                            Name = EmptyValue,
                            ProvinceId = province.Id,
                        };

                        //Craete a new location with N/A
                        _locationRepository.Insert(location);
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
            var locationKey = GetLocationKey(province.Region.Name, province.Name, name);

            var location = locationsByRegionProvinceName.Get(locationKey);

            if (location != null) return location;

            return locationRepository.GetOrInsert(ReplaceEmpty(name), province.Id, latitude, longitude);
        }

        private static Province GetProvince(IProvinceRepository provinceRepository, string provinceName, Region region)
        {
            var provinceKey = GetProvinceKey(region.Name, provinceName);

            var province = provincesByRegionAndName.Get(provinceKey);

            if (province != null) return province;

            return provinceRepository.GetOrInsert(ReplaceEmpty(provinceName), region.Id);
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
    }
}
