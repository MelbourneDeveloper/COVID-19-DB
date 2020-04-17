using Covid19DB.Db;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Covid19DBApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            var locationMapElements = new List<MapElement>();

            using (var covid19DbContext = new Covid19DbContext())
            {
                var locations = covid19DbContext.Locations.Where(p => p.Province.Region.Name == "Australia");

                foreach (var location in locations)
                {
                    if (!location.Latitude.HasValue) continue;

                    var snPosition = new BasicGeoposition 
                    { 
                        Latitude = (double)location.Latitude.Value, 
                        Longitude = (double)location.Longitude.Value
                    };

                    var snPoint = new Geopoint(snPosition);

                    var mapIcon = new MapIcon
                    {
                        Location = snPoint,
                        NormalizedAnchorPoint = new Point(0.5, 1.0),
                        ZIndex = 0,
                        Title = location.Name
                    };

                    locationMapElements.Add(mapIcon);
                }
            }

            var locationsLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = locationMapElements
            };

            TheMapControl.Layers.Add(locationsLayer);

            //TheMapControl.Center = snPoint;
            TheMapControl.ZoomLevel = 14;

        }
    }
}
