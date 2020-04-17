using Covid19DB.Db;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;


namespace Covid19DBApp
{
    public class ViewModel
    {
        public ObservableCollection<MapElement> MapElements { get; } = new ObservableCollection<MapElement>();
    }

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            var viewModel = new ViewModel();

            using (var covid19DbContext = new Covid19DbContext())
            {
                var locations = covid19DbContext.Locations.Where(p => p.Province.Region.Name == "Australia").Include(d=>d.Province);

                foreach (var location in locations)
                {
                    if (!location.Latitude.HasValue) continue;

                    var sumOfNewCases = covid19DbContext.LocationDays.Where(ld => ld.Location.Id == location.Id).Sum(ld => ld.NewCases);

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

                    viewModel.MapElements.Add(mapIcon);
                }
            }

            var locationsLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = viewModel.MapElements
            };

            TheMapControl.Layers.Add(locationsLayer);

        }
    }
}
