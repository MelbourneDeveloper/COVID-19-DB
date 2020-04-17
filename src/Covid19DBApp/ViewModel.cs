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

            var days = locationDays.GroupBy(ld => ld.DateOfCount).ToList();
            DayCount = days.Count;
            SelectedDay = DayCount - 1;

            _minDate = days.Min(d => d.Key);

            Update();
        }

        private void Update()
        {
            foreach (var location in _locations)
            {
                if (!location.Latitude.HasValue) continue;

                var sumOfNewCases = _locationDays.Where(ld => ld.Location != null && ld.Location.Id == location.Id).Sum(ld => ld.NewCases);

                var basicGeoposition = new BasicGeoposition
                {
                    Latitude = (double)location.Latitude.Value,
                    Longitude = (double)location.Longitude.Value
                };

                var Geopoint = new Geopoint(basicGeoposition);

                var mapIcon = new MapIcon
                {
                    Location = Geopoint,
                    NormalizedAnchorPoint = new Point(0.5, 1.0),
                    ZIndex = 0,
                    Title = $"{location?.Province?.Name} - {sumOfNewCases}",
                    Tag = new LocationInformation(location) { Confirmed = sumOfNewCases.Value }
                };

                _mapIconsByLocation.Add(location.Id, mapIcon);

                MapElements.Add(mapIcon);
            }
        }
        #endregion
    }
}
