using Covid19DB.Entities;
using Covid19DB.Repositories;
using Covid19DB.Services;
using System;
using System.Collections.Generic;

namespace Covid19DB
{
    public class Processor
    {
        #region Fields
        private const string EmptyValue = "N/A";
        private const string None = "NONE";
        private readonly Dictionary<Guid, int?> _confirmedCasesByLocation = new Dictionary<Guid, int?>();
        private readonly ICache<Region> _regionsByName = new Cache<Region>();
        private readonly ICache<Province> _provincesByRegionAndName = new Cache<Province>();
        private readonly ICache<Location> _locationsByRegionProvinceName = new Cache<Location>();
        private readonly IProvinceRepository _provinceRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ILocationDayRepository _locationDayRepository;
        #endregion

        #region Constructor
        public Processor(
        IProvinceRepository provinceRepository,
        IRegionRepository regionRepository,
        ILocationRepository locationRepository,
        ILocationDayRepository locationDayRepository
            )
        {
            _provinceRepository = provinceRepository;
            _regionRepository = regionRepository;
            _locationRepository = locationRepository;
            _locationDayRepository = locationDayRepository;
        }
        #endregion

        #region Public Methods
        public void Process(IEnumerable<RowModel> rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            foreach (var rawModel in rows)
            {
                var region = GetRegion(rawModel.Country_Region);
                var province = GetProvince(rawModel.Province_State, region);
                var location = GetLocation(rawModel.Admin2, rawModel.Lat, rawModel.Long_, province);

                _ = _confirmedCasesByLocation.TryGetValue(location.Id, out var totalConfirmed);

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

                _ = _locationDayRepository.GetOrInsert(rawModel.Date, location, currentConfirmed, rawModel.Deaths, rawModel.Recovered);

                if (!_confirmedCasesByLocation.ContainsKey(location.Id)) _confirmedCasesByLocation.Add(location.Id, rawModel.Confirmed);
            }
        }
        #endregion

        #region Private Methods
        private static string ReplaceEmpty(string name)
        {
            return name == null ||
                string.Compare(name, None, StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(name, string.Empty, StringComparison.OrdinalIgnoreCase) == 0
                ? EmptyValue
                : name;
        }

        private static string GetProvinceKey(string regionName, string provinceName)
        {
            return $"{ReplaceEmpty(regionName)}.{ReplaceEmpty(provinceName)}";
        }

        private static string GetLocationKey(string regionName, string provinceName, string locationName)
        {
            return $"{GetProvinceKey(regionName, provinceName)}.{ReplaceEmpty(locationName)}";
        }

        private Location GetLocation(string name, decimal? latitude, decimal? longitude, Province province)
        {
            var locationKey = GetLocationKey(province.Region.Name, province.Name, name);

            var location = _locationsByRegionProvinceName.Get(locationKey);

            if (location != null) return location;

            location = _locationRepository.GetOrInsert(ReplaceEmpty(name), province, latitude, longitude);

            _locationsByRegionProvinceName.Add(locationKey, location);

            return location;
        }

        private Province GetProvince(string provinceName, Region region)
        {
            var provinceKey = GetProvinceKey(region.Name, provinceName);

            var province = _provincesByRegionAndName.Get(provinceKey);

            if (province != null) return province;

            province = _provinceRepository.GetOrInsert(ReplaceEmpty(provinceName), region);

            _provincesByRegionAndName.Add(provinceKey, province);

            return province;
        }

        private Region GetRegion(string regionName)
        {
            var region = _regionsByName.Get(regionName);

            if (region != null) return region;

            region = _regionRepository.GetOrInsert(regionName);

            _regionsByName.Add(regionName, region);

            return region;
        }
        #endregion
    }
}
