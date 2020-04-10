using Covid19DB.Entities;
using Covid19DB.Repositories;

namespace Covid19DB
{
    internal partial class Program
    {
        private static void Main(string[] args)
        {
            var csvReader = new CsvReader();

            var rows = csvReader.ReadCsvFiles(@"C:\Code\COVID-19\csse_covid_19_data\csse_covid_19_daily_reports");

            using (var covid19DbContext = new Covid19DbContext())
            {
                var provinceRepository = new ProvinceRepository(covid19DbContext);
                var regionRepository = new RegionRepository(covid19DbContext);
                var locationRepository = new LocationRepository(covid19DbContext);
                var locationDayRepository = new LocationDayRepository(covid19DbContext);

                var processor = new Processor(provinceRepository, regionRepository, locationRepository, locationDayRepository);

                processor.Process(rows);

                covid19DbContext.SaveChanges();
            }
        }
    }
}
