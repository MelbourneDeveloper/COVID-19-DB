using Covid19DB.Entities;
using Covid19DB.Models;
using Covid19DB.Models.Logging;
using Covid19DB.Repositories;
using Covid19DB.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Covid19DB
{
    public class RollingTotalsInfo
    {
        public int? RollingTotal { get; set; }
        public DateTimeOffset PreviousDate { get; set; }
        public int PreviousDateRowNumber { get; set; }
    }

    public class Processor
    {
        #region Fields
        private const string EmptyValue = "N/A";
        private const string None = "NONE";
        //private const string Unassigned = "Unassigned";
        private readonly Dictionary<Guid, RollingTotalsInfo> _confirmedCasesByLocation = new Dictionary<Guid, RollingTotalsInfo>();
        private readonly Dictionary<Guid, RollingTotalsInfo> _recoveriesByLocation = new Dictionary<Guid, RollingTotalsInfo>();
        private readonly Dictionary<Guid, RollingTotalsInfo> _deathsByLocation = new Dictionary<Guid, RollingTotalsInfo>();

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

            foreach (var rowModel in rows)
            {
                if (rowModel.Confirmed.HasValue)
                {
                    if ((rowModel.Deaths + rowModel.Recovered) > rowModel.Confirmed)
                    {
                        LogRowInbalance(rowModel, "Deaths and Recovered are higher than Confirmed");
                    }

                    if (rowModel.Active.HasValue && rowModel.Active > 0)
                    {
                        if ((rowModel.Active + rowModel.Deaths + rowModel.Recovered) != rowModel.Confirmed)
                        {
                            LogRowInbalance(rowModel, "Deaths and Recovered and Active don't equal Confirmed");
                        }
                    }
                }

                var region = GetRegion(rowModel.Country_Region);

                var provinceName = rowModel.Province_State;
                var locationName = rowModel.Admin2;

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
                    locationName = tokens[0].
                        Replace(" County", string.Empty, StringComparison.OrdinalIgnoreCase). // Remove county so figures are merged. E.g. Hudson County -> Hudson
                        Trim();
                    var provinceToken = tokens[1].
                        Replace("(From Diamond Princess)", string.Empty, StringComparison.OrdinalIgnoreCase);  //Get rid of From Diamond Princess //TODO: Mark data with source somehow 
                    provinceName = _provinceLookupService.GetProvinceName(region.Name, provinceToken.Trim());
                }

                var province = GetProvince(provinceName, region);
                var location = GetLocation(locationName, rowModel.Lat, rowModel.Long_, province);

                var currentNewCases = GetDailyValue(_confirmedCasesByLocation, location, rowModel.Confirmed, "New Cases", rowModel.CsvRowNumber, rowModel.Date);
                var currentDeaths = GetDailyValue(_deathsByLocation, location, rowModel.Deaths, "Deaths", rowModel.CsvRowNumber, rowModel.Date);
                var currentRecoveries = GetDailyValue(_recoveriesByLocation, location, rowModel.Recovered, "Recoveries", rowModel.CsvRowNumber, rowModel.Date);

                _ = _locationDayRepository.GetOrInsert(rowModel.Date, location, currentNewCases, currentDeaths, currentRecoveries);
            }
        }
        #endregion

        #region Private Methods
        public void UpdateLocationCoordinates(IEnumerable<RowModel> rows)
        {
            var filteredRowModels = rows.OrderByDescending(r => r.Date).Where(r =>
            r.Lat.HasValue &&
            r.Lat.Value != 0 &&
            r.Long_.HasValue &&
            r.Long_.Value != 0);

            var locations = _locationRepository.GetAll();
            foreach (var location in locations)
            {
                if (!location.Latitude.HasValue || !location.Longitude.HasValue)
                {
                    var rowModel = filteredRowModels.FirstOrDefault(r => string.Compare(r.Country_Region, location.Province.Region.Name, StringComparison.OrdinalIgnoreCase) == 0 && CompareLocationToRowModel(r, location));

                    if (rowModel != null)
                    {
                        location.Latitude = rowModel.Lat;
                        location.Longitude = rowModel.Long_;
                        _locationRepository.Update(location);
                    }
                }
            }
        }

        private static bool CompareLocationToRowModel(RowModel rowModel, Location location)
        {
            return
            string.Compare(rowModel.Country_Region, location.Province.Region.Name, StringComparison.OrdinalIgnoreCase) == 0 &&
            (
                (
                    //Province is empty
                    location.Province.Name == EmptyValue &&
                    string.IsNullOrEmpty(rowModel.Province_State)
                ) ||
                //Province matches
                string.Compare(rowModel.Province_State, location.Province.Name, StringComparison.OrdinalIgnoreCase) == 0
            ) &&
            (
                //Location is empty
                (((location.Name == EmptyValue) || string.Compare(location.Name, "unassigned", StringComparison.OrdinalIgnoreCase) == 0) && string.IsNullOrEmpty(rowModel.Admin2)) ||
                //Location matches 
                string.Compare(rowModel.Admin2, location.Name, StringComparison.OrdinalIgnoreCase) == 0
            );
        }

        private void LogRowInbalance(RowModel rowModel, string message)
        {
            _logger.Log(
                LogLevel.Warning,
                default,
                new CasesRowInbalance
                {
                    Date = rowModel.Date,
                    CsvRowNumber = rowModel.CsvRowNumber,
                    Confirmed = rowModel.Confirmed,
                    Recoveries = rowModel.Recovered,
                    Deaths = rowModel.Deaths,
                    Active = rowModel.Active,
                    Url = GetRowUrl(rowModel.Date, rowModel.CsvRowNumber),
                    Message = message
                },
                null,
                null);
        }

        private int? GetDailyValue(Dictionary<Guid, RollingTotalsInfo> lastValuesByLocationId, Location location, int? rowValue, string columnName, int csvRowNumber, DateTimeOffset date)
        {
            _ = lastValuesByLocationId.TryGetValue(location.Id, out var lastRollingTotalsInfo);
            int? returnValue = null;

            if (rowValue.HasValue)
            {
                var newRollingTotalsInfo = new RollingTotalsInfo { PreviousDate = date, RollingTotal = rowValue, PreviousDateRowNumber = csvRowNumber };

                if (lastRollingTotalsInfo != null && lastRollingTotalsInfo.RollingTotal.HasValue)
                {
                    returnValue = rowValue - lastRollingTotalsInfo.RollingTotal;
                    lastValuesByLocationId[location.Id] = newRollingTotalsInfo;
                }
                else
                {
                    returnValue = rowValue;
                    lastValuesByLocationId.Add(location.Id, newRollingTotalsInfo);
                }
            }

            if (returnValue.HasValue && returnValue < 0)
            {
                _logger.Log(LogLevel.Warning,
                    default,
                    new CaseRowAdjustment
                    {
                        Column = columnName,
                        Date = date,
                        Location = location.Name,
                        Provice = location.Province.Name,
                        Region = location.Province.Region.Name,
                        Url = GetRowUrl(date, csvRowNumber),
                        PreviousUrl = GetRowUrl(lastRollingTotalsInfo.PreviousDate, lastRollingTotalsInfo.PreviousDateRowNumber),
                        Discrepancy = returnValue.Value
                    },
                    null,
                    null);
            }

            return returnValue;
        }

        private static string ReplaceEmpty(string name)
        {
            return name == null ||
                string.Compare(name, None, StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(name, string.Empty, StringComparison.OrdinalIgnoreCase) == 0
                //string.Compare(name, Unassigned, StringComparison.OrdinalIgnoreCase) == 0
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

        private static string GetRowUrl(DateTimeOffset date, int csvRowNumber)
        {
            var month = date.Month.ToString().PadLeft(2, '0');
            var day = date.Day.ToString().PadLeft(2, '0');
            return $"[{csvRowNumber}](https://github.com/CSSEGISandData/COVID-19/blob/master/csse_covid_19_data/csse_covid_19_daily_reports/{month}-{day}-2020.csv#L{csvRowNumber})";
        }
        #endregion
    }
}
