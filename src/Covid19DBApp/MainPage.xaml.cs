using Covid19DB.Db;
using Covid19DB.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;


namespace Covid19DBApp
{
    public class LocationInformation : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Fields
        private int confirmed;
        private int deaths;
        private int recoveries;
        #endregion

        #region Public Properties
        public Location Location;

        public LocationInformation(Location location)
        {
            Location = location;
        }

        public int Confirmed
        {
            get => confirmed;
            set
            {
                confirmed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Confirmed)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
            }
        }

        public int Deaths
        {
            get => deaths;
            set
            {
                deaths = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Deaths)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
            }
        }

        public int Recoveries
        {
            get => recoveries;
            set
            {
                recoveries = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Recoveries)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
            }
        }

        public int Active => Confirmed - (Deaths + Recoveries);
        #endregion
    }


    public class ViewModel
    {
        //private 

        public ViewModel(IEnumerable<Location> locations)
        {

        }

        public ObservableCollection<MapElement> MapElements { get; } = new ObservableCollection<MapElement>();
    }

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            ViewModel viewModel = null;

            using (var covid19DbContext = new Covid19DbContext())
            {
                var locations = covid19DbContext.Locations.Where(p => p.Province.Region.Name == "Australia").Include(d => d.Province).ToList();

                viewModel = new ViewModel(locations);

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
