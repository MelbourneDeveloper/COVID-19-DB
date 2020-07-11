using Covid19DB.Db;
using Covid19DBApp;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Covid19DBUno
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            ViewModel viewModel = null;

            using (var covid19DbContext = new Covid19DbContext())
            {
                //Is Czechia the czech republic?

                var europeanCountries = new List<string> { "United Kingdom", "Ukraine", "Greece", "Vatican City", "Switzerland", "Sweden", "Slovakia", "Slovenia", "Serbia", "Russian Federation", "Romania", "Republic of Moldova", "Republic of Ireland", "Poland", "Norway", "North Macedonia", "Netherlands", "Montenegro", "Moldova", "Monaco", "Luxembourg", "Lithuania", "Liechtenstein", "Germany", "Finland", "Turkey", "Italy", "France", "Spain", "Russia", "Ireland", "North Ireland", "Belarus", "Belgium", "Bulgaria", "Cyprus", "Czech Republic", "Czechia", "Denmark" };

                var locations = covid19DbContext.Locations.Where(p => europeanCountries.Contains(p.Province.Region.Name)).Include(d => d.Province).ThenInclude(p => p.Region).ToList();
                //var locations = covid19DbContext.Locations.Include(d => d.Province).ToList();

                viewModel = new ViewModel(locations, covid19DbContext.LocationDays);

                DataContext = viewModel;
            }

            var locationsLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = viewModel.MapElements
            };

            TheMapControl.Layers.Add(locationsLayer);

            TheSlider.Value = 0;

        }
    }
}
