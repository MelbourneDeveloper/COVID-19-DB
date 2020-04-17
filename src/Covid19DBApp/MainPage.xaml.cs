using Covid19DB.Db;
using Covid19DB.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;


namespace Covid19DBApp
{
    public class asdasd
    {
        public Location Location { get; set; }
        public DateTimeOffset Date { get; set; }
    }

    public sealed partial class MainPage : Page
    {
        private List<MapIcon> _geoPoints = new List<MapIcon>();

        public MainPage()
        {
            InitializeComponent();

            TheSlider.ValueChanged += TheSlider_ValueChanged;

            var locationMapElements = new List<MapElement>();

            using (var covid19DbContext = new Covid19DbContext())
            {
                var locations = covid19DbContext.Locations.Where(p => p.Province.Region.Name == "Australia").Include(d=>d.Province);

                foreach (var location in locations)
                {
                    if (!location.Latitude.HasValue) continue;

                    var asdfasd = covid19DbContext.LocationDays.Where(ld => ld.Location.Id == location.Id).Sum(ld => ld.NewCases);
                    var asdasdsd = covid19DbContext.LocationDays.Where(ld => ld.Location.Id == location.Id).Max(dsd => dsd.DateOfCount);

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
                        Title = $"{location?.Province?.Name} - {asdfasd}",
                        Tag = new asdasd { Location = location, Date = asdasdsd }
                    };

                    _geoPoints.Add(mapIcon);

                    locationMapElements.Add(mapIcon);
                }
            }

            var locationsLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = locationMapElements
            };

            TheMapControl.Layers.Add(locationsLayer);

        }

        private void TheSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            
        }
    }
}
