using Covid19DB.Entities;
using Covid19DB.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Covid19DB
{
    public class Processor
    {
        #region Fields
        private const string EmptyValue = "N/A";
        private const string None = "NONE";
        private Dictionary<Guid, int?> _confirmedCasesByLocation = new Dictionary<Guid, int?>();
        private ICache<Region> _regionsByName = new Cache<Region>();
        private ICache<Province> _provincesByRegionAndName = new Cache<Province>();
        private ICache<Location> _locationsByRegionProvinceName = new Cache<Location>();
        IProvinceRepository _provinceRepository;
        IRegionRepository _regionRepository;
        ILocationRepository _locationRepository;
        ILocationDayRepository _locationDayRepository;
        IEnumerable<RawModel> _rows;
        #endregion

        #region Constructor
        public Processor(
        IProvinceRepository provinceRepository,
        IRegionRepository regionRepository,
        ILocationRepository locationRepository,
        ILocationDayRepository locationDayRepository,
        IEnumerable<RawModel> rows
            )
        {
            _provinceRepository = provinceRepository;
            _regionRepository = regionRepository;
            _locationRepository = locationRepository;
            _locationDayRepository = locationDayRepository;
            _rows = rows;
        }
        #endregion

        #region Public Methods
        public void Process()
        {
            var regionGroupings = _rows.Where(a => !string.IsNullOrEmpty(a.Country_Region)).GroupBy(a => a.Country_Region).ToList();
            var provinceGroupings = _rows.Where(a => !string.IsNullOrEmpty(a.Province_State)).GroupBy(a => a.Province_State).ToList();
            var locationGroupings = _rows.Where(a => !string.IsNullOrEmpty(a.Admin2)).GroupBy(a => a.Admin2).ToList();

            //Add any missing regions
            foreach (var regionGrouping in regionGroupings)
            {
                var regionName = regionGrouping.Key;
                var region = GetRegion(_regionRepository, regionName);
                _regionsByName.Add(regionGrouping.Key, region);
            }

            //Add any missing provinces
            foreach (var provinceGrouping in provinceGroupings)
            {
                var rawModel = provinceGrouping.First();
                var region = _regionsByName.Get(rawModel.Country_Region);

                var provinceName = provinceGrouping.Key;

                var province = GetProvince(_provinceRepository, provinceName, region);
                _provincesByRegionAndName.Add(GetProvinceKey(region.Name, provinceName), province);
            }

            //Add any missing locations
            foreach (var locationGrouping in locationGroupings)
            {
                var rawModel = locationGrouping.First();
                var region = _regionsByName.Get(rawModel.Country_Region);
                var province = GetProvince(_provinceRepository, rawModel.Province_State, region);

                var location = GetLocation(_locationRepository, rawModel.Admin2, rawModel.Lat, rawModel.Long_, province);

                _locationsByRegionProvinceName.Add(GetLocationKey(region.Name, province.Name, location.Name), location);
            }

            foreach (var rawModel in _rows.OrderBy(r => r.Date))
            {

                var locationKey = GetLocationKey(rawModel.Country_Region, rawModel.Province_State, rawModel.Admin2);

                var location = _locationsByRegionProvinceName.Get(locationKey);

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

                _confirmedCasesByLocation.TryGetValue(location.Id, out var totalConfirmed);

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

                _ = _locationDayRepository.GetOrInsertLocationDay(rawModel.Date, location.Id, currentConfirmed, rawModel.Deaths, rawModel.Recovered);

                if (!_confirmedCasesByLocation.ContainsKey(location.Id)) _confirmedCasesByLocation.Add(location.Id, rawModel.Confirmed);
            }
        }
        #endregion

        #region Private Methods
        private string GetProvinceKey(string regionName, string provinceName)
        {
            return $"{ReplaceEmpty(regionName)}.{ReplaceEmpty(provinceName)}";
        }

        private string GetLocationKey(string regionName, string provinceName, string locationName)
        {
            return $"{GetProvinceKey(regionName, provinceName)}.{ReplaceEmpty(locationName)}";
        }

        private Location GetLocation(ILocationRepository locationRepository, string name, decimal? latitude, decimal? longitude, Province province)
        {
            var locationKey = GetLocationKey(province.Region.Name, province.Name, name);

            var location = _locationsByRegionProvinceName.Get(locationKey);

            if (location != null) return location;

            return locationRepository.GetOrInsert(ReplaceEmpty(name), province.Id, latitude, longitude);
        }

        private Province GetProvince(IProvinceRepository provinceRepository, string provinceName, Region region)
        {
            var provinceKey = GetProvinceKey(region.Name, provinceName);

            var province = _provincesByRegionAndName.Get(provinceKey);

            if (province != null) return province;

            return provinceRepository.GetOrInsert(ReplaceEmpty(provinceName), region.Id);
        }

        private string ReplaceEmpty(string name)
        {
            if (
                name == null ||
                string.Compare(name, None, StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(name, string.Empty, StringComparison.OrdinalIgnoreCase) == 0
                ) return EmptyValue;

            return name;
        }

        private Region GetRegion(IRegionRepository regionRepository, string regionName)
        {
            return regionRepository.GetOrInsert(regionName);
        }
        #endregion
    }
}
