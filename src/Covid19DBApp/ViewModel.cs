using Covid19DB.Entities;
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
        IEnumerable<LocationDay> _locationDays;
        private Dictionary<DateTimeOffset, IEnumerable<LocationDay>> _locationDaysByDate = new Dictionary<DateTimeOffset, IEnumerable<LocationDay>>();
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
        public ViewModel(IEnumerable<Location> locations, IEnumerable<LocationDay> locationDays)
        {
            _locations = locations;
            _locationDays = locationDays;

            var locationDayGroupings = locationDays.GroupBy(ld => ld.DateOfCount).ToList();
            DayCount = locationDayGroupings.Count;
            SelectedDay = DayCount - 1;

            _minDate = locationDayGroupings.Min(d => d.Key);

            foreach (var locationDayGrouping in locationDayGroupings)
            {
                _locationDaysByDate.Add(locationDayGrouping.Key, locationDayGrouping.Where(ld => ld.DateOfCount <= locationDayGrouping.Key).ToList());
            }

            Update();
        }

        private void Update()
        {
            foreach (var location in _locations)
            {
                if (!location.Latitude.HasValue) continue;

                if (!_locationDaysByDate.ContainsKey(SelectedDate)) return;

                var sumOfNewCases = _locationDaysByDate[SelectedDate].Where(ld => ld.Location != null && ld.Location.Id == location.Id).Sum(ld => ld.NewCases);

                MapIcon mapIcon = null;

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
                        Tag = new LocationInformation(location) { Confirmed = sumOfNewCases.Value }
                    };

                    _mapIconsByLocation.Add(location.Id, mapIcon);

                    MapElements.Add(mapIcon);
                }

                mapIcon.Title = $"{location?.Province?.Name} - {sumOfNewCases}";
            }
        }
        #endregion
    }
}
