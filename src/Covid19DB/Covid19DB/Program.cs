using Covid19DB.Db;
using Covid19DB.Repositories;
using System;
using System.Linq;

namespace Covid19DB
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var directoryPath = args.FirstOrDefault();

            if (string.IsNullOrEmpty(directoryPath)) throw new ArgumentException("Daily Reports Directory not specified");

            var rows = CsvReader.ReadCsvFiles(directoryPath);

            using var covid19DbContext = new Covid19DbContext();

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
