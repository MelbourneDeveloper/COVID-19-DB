using Covid19DB.Db;
using Microsoft.EntityFrameworkCore;
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

                viewModel = new ViewModel(locations, covid19DbContext.LocationDays);
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
