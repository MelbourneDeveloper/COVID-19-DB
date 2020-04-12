using Covid19DB.Db;
using Covid19DB.Repositories;
using Covid19DB.Services;
using System;
using System.IO;
using System.Linq;

namespace Covid19DB
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new Logger<Processor>();

            var directoryPath = args.FirstOrDefault();

            if (string.IsNullOrEmpty(directoryPath)) throw new ArgumentException("Daily Reports Directory not specified");

            if (!Directory.Exists(directoryPath)) throw new ArgumentException($"The directory {directoryPath} does not exist. Please check the path");

            var rows = CsvReader.ReadCsvFiles(directoryPath);

            using var covid19DbContext = new Covid19DbContext();

            var provinceRepository = new ProvinceRepository(covid19DbContext);
            var regionRepository = new RegionRepository(covid19DbContext);
            var locationRepository = new LocationRepository(covid19DbContext);
            var locationDayRepository = new LocationDayRepository(covid19DbContext);

            var processor = new Processor(
                provinceRepository,
                regionRepository,
                locationRepository,
                locationDayRepository,
                logger,
                new ProvinceLookupService());

            processor.Process(rows);

            covid19DbContext.SaveChanges();

            logger.ToCsv();

            Console.WriteLine("Done");
        }
    }
}
