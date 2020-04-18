using Covid19DB.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;


namespace Covid19DBApp
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            ViewModel viewModel = null;

            using (var covid19DbContext = new Covid19DbContext())
            {
                var locations = covid19DbContext.Locations.Where(p => p.Province.Region.Name == "Australia").Include(d => d.Province).ToList();

                //var locationDayGroupings = covid19DbContext.LocationDays.Include(ld => ld.Location).ToList().GroupBy(ld => ld.DateOfCount);

                //var _minDate = locationDayGroupings.First().Key;

                viewModel = new ViewModel(locations, covid19DbContext.LocationDays);

                DataContext = viewModel;
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
