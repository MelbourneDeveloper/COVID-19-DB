using Covid19DB.Entities;
using Covid19DB.Models;
using Covid19DB.Repositories;
using Covid19DB.Services;
using Microsoft.Extensions.Logging;
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
        private const string Unassigned = "Unassigned";
        private readonly Dictionary<Guid, int?> _confirmedCasesByLocation = new Dictionary<Guid, int?>();
        private readonly Dictionary<Guid, int?> _recoveriesByLocation = new Dictionary<Guid, int?>();
        private readonly Dictionary<Guid, int?> _deathsByLocation = new Dictionary<Guid, int?>();
        private readonly ICache<Region> _regionsByName = new Cache<Region>();
        private readonly ICache<Province> _provincesByRegionAndName = new Cache<Province>();
        private readonly ICache<Location> _locationsByRegionProvinceName = new Cache<Location>();
        private readonly IProvinceRepository _provinceRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ILocationDayRepository _locationDayRepository;
        private readonly ILogger<Processor> _logger;
        private readonly IProvinceLookupService _provinceLookupService;
        #endregion

        #region Constructor
        public Processor(
        IProvinceRepository provinceRepository,
        IRegionRepository regionRepository,
        ILocationRepository locationRepository,
        ILocationDayRepository locationDayRepository,
        ILogger<Processor> logger,
        IProvinceLookupService provinceLookupService
            )
        {
            _provinceRepository = provinceRepository;
            _regionRepository = regionRepository;
            _locationRepository = locationRepository;
            _locationDayRepository = locationDayRepository;
            _logger = logger;
            _provinceLookupService = provinceLookupService;
        }
        #endregion

        #region Public Methods
        public void Process(IEnumerable<RowModel> rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            foreach (var rawModel in rows)
            {
                var region = GetRegion(rawModel.Country_Region);

                var provinceName = rawModel.Province_State;
                var locationName = rawModel.Admin2;

                if (region.Name == "Netherlands")
                {
                    //ISSUE

                    //There is a comma in some of the Netherlands' provinces
                }
                else if (provinceName == "Virgin Islands, U.S.")
                {
                    //ISSUE: https://github.com/CSSEGISandData/COVID-19/issues/2119
                    provinceName = "Virgin Islands";
                }
                else if (provinceName != null && provinceName.Contains(',', StringComparison.OrdinalIgnoreCase))
                {
                    //Deal with cases where there is a comma in the Province field. This means that we're dealing with a US county/state

                    var tokens = provinceName.Split(",").ToList();
                    locationName = tokens[0].Trim();
                    var provinceToken = tokens[1].Replace("(From Diamond Princess)", string.Empty, StringComparison.OrdinalIgnoreCase);
                    provinceName = _provinceLookupService.GetProvinceName(region.Name, provinceToken.Trim());
                }

                var province = GetProvince(provinceName, region);
                var location = GetLocation(locationName, rawModel.Lat, rawModel.Long_, province);

                var currentNewCases = GetDailyValue(_confirmedCasesByLocation, location.Id, rawModel.Confirmed, "New Cases", rawModel.Date);
                var currentDeaths = GetDailyValue(_deathsByLocation, location.Id, rawModel.Deaths, "Deaths", rawModel.Date);
                var currentRecoveries = GetDailyValue(_recoveriesByLocation, location.Id, rawModel.Recovered, "Recoveries", rawModel.Date);


                _ = _locationDayRepository.GetOrInsert(rawModel.Date, location, currentNewCases, currentDeaths, currentRecoveries);

                if (!_confirmedCasesByLocation.ContainsKey(location.Id)) _confirmedCasesByLocation.Add(location.Id, rawModel.Confirmed);
            }
        }
        #endregion

        #region Private Methods
        private int? GetDailyValue(Dictionary<Guid, int?> calculatedValuesByLocationId, Guid locationId, int? rowValue, string columnName, DateTimeOffset date)
        {
            _ = calculatedValuesByLocationId.TryGetValue(locationId, out var total);
            int? returnValue = null;

            if (rowValue.HasValue)
            {
                if (total.HasValue)
                {
                    returnValue = rowValue - total;
                    calculatedValuesByLocationId[locationId] = total + returnValue;
                }
                else
                {
                    returnValue = rowValue;
                    calculatedValuesByLocationId.Add(locationId, rowValue);
                }
            }

            if (returnValue.HasValue && returnValue < 0)
            {
                _logger.Log(LogLevel.Warning, default, new CountAnomaly { ColumnName = columnName, Date = date, LocationId = locationId }, null, null);
            }

            return returnValue;
        }

        private static string ReplaceEmpty(string name)
        {
            return name == null ||
                string.Compare(name, None, StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(name, string.Empty, StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(name, Unassigned, StringComparison.OrdinalIgnoreCase) == 0
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
