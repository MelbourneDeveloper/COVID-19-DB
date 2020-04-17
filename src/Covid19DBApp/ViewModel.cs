using Covid19DB.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Maps;

namespace Covid19DBApp
{
    public class ViewModel
    {
        #region Fields
        private Dictionary<Guid, MapIcon> _mapIconsByLocation = new Dictionary<Guid, MapIcon>();
        #endregion

        #region Public Properties
        public int DayCount { get; }
        public ObservableCollection<MapElement> MapElements { get; } = new ObservableCollection<MapElement>();
        #endregion

        #region Constructor
        public ViewModel(IEnumerable<Location> locations, IEnumerable<LocationDay> locationDays)
        {
            foreach (var location in locations)
            {
                if (!location.Latitude.HasValue) continue;

                var sumOfNewCases = locationDays.Where(ld => ld.Location!=null && ld.Location.Id == location.Id).Sum(ld => ld.NewCases);

                var days = locationDays.GroupBy(ld => ld.DateOfCount).ToList();
                DayCount = days.Count;

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
                    Title = $"{location?.Province?.Name} - {sumOfNewCases}"
                };

                _mapIconsByLocation.Add(location.Id, mapIcon);

                MapElements.Add(mapIcon);
            }
        }
        #endregion
    }
}
