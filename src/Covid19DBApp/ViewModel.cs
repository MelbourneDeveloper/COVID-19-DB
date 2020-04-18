using Covid19DB.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Maps;

namespace Covid19DBApp
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Fields
        private Dictionary<Guid, MapIcon> _mapIconsByLocation = new Dictionary<Guid, MapIcon>();
        private int selectedDay;
        private DateTimeOffset _minDate;
        IEnumerable<Location> _locations;
        DbSet<LocationDay> _locationDays;
        //private Dictionary<DateTimeOffset, IEnumerable<LocationDay>> _locationDaysByDate = new Dictionary<DateTimeOffset, IEnumerable<LocationDay>>();
        private Dictionary<Guid, IEnumerable<LocationDay>> _locationDaysByLocation = new Dictionary<Guid, IEnumerable<LocationDay>>();
        private Dictionary<string, double> _totalConfirmedByLocation = new Dictionary<string, double>();
        #endregion

        #region Public Properties
        public int SelectedDay
        {
            get => selectedDay;
            set
            {
                selectedDay = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDay)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDate)));
                Update();
            }
        }

        public int DayCount { get; }
        public ObservableCollection<MapElement> MapElements { get; } = new ObservableCollection<MapElement>();
        public DateTimeOffset SelectedDate => _minDate.AddDays(SelectedDay);
        #endregion

        #region Constructor
        public ViewModel(IEnumerable<Location> locations, DbSet<LocationDay> locationDays)
        {
            _locations = locations;
            _locationDays = locationDays;

            var locationDayGroupings = locationDays.Where(ld => ld.NewCases.HasValue && ld.NewCases > 0).Include(ld => ld.Location).ToList().GroupBy(ld => ld.DateOfCount);
            var locationDayLocationGroupings = locationDays.Include(ld => ld.Location).ToList().GroupBy(ld => ld.Location);

            _minDate = locationDayGroupings.First().Key;

            foreach (var locationDayGrouping in locationDayGroupings.OrderBy(ld => ld.Key))
            {
                var lds = locationDayGrouping.Where(ld => ld.DateOfCount <= locationDayGrouping.Key).ToList();
                //_locationDaysByDate.Add(locationDayGrouping.Key, lds);
                DayCount++;
            }

            foreach (var locationDayLocationGrouping in locationDayLocationGroupings)
            {
                var locationsDays = locationDayLocationGrouping.ToList();
                _locationDaysByLocation.Add(locationDayLocationGrouping.Key.Id, locationsDays);
            }

            SelectedDay = DayCount - 1;

            Update();
        }

        private void Update()
        {
            foreach (var location in _locations)
            {
                if (!location.Latitude.HasValue) continue;

                var key = $"{SelectedDate}.{location.Id}";

                double sumOfNewCases = 0;

                MapIcon mapIcon = null;

                if (_totalConfirmedByLocation.ContainsKey(key))
                {
                    mapIcon = _mapIconsByLocation[location.Id];
                    sumOfNewCases = _totalConfirmedByLocation[key];
                }
                else
                {
                    var locationDays = _locationDaysByLocation[location.Id];

                    //var locationDay = locationDays.FirstOrDefault(ld => ld.DateOfCount == SelectedDate);
                    sumOfNewCases = (double)locationDays.Where(ld => ld.DateOfCount <= SelectedDate).Sum(ld => ld.NewCases);

                    if (_mapIconsByLocation.ContainsKey(location.Id))
                    {
                        mapIcon = _mapIconsByLocation[location.Id];
                    }
                    else
                    {
                        var basicGeoposition = new BasicGeoposition
                        {
                            Latitude = (double)location.Latitude.Value,
                            Longitude = (double)location.Longitude.Value
                        };

                        var Geopoint = new Geopoint(basicGeoposition);

                        mapIcon = new MapIcon
                        {
                            Location = Geopoint,
                            NormalizedAnchorPoint = new Point(0.5, 1.0),
                            ZIndex = 0,
                            //Tag = new LocationInformation(location) { Confirmed = sumOfNewCases }
                        };

                        _mapIconsByLocation.Add(location.Id, mapIcon);

                        MapElements.Add(mapIcon);
                    }

                    _totalConfirmedByLocation.Add(key, sumOfNewCases);
                }

                mapIcon.Title = $"{location?.Name} - {sumOfNewCases}";
                //mapIcon.Title = $"{location?.Province?.Region?.Name} - {sumOfNewCases.ToString("0.##")}";
            }
        }
        #endregion
    }
}
