using System;
using System.IO;
using System.Linq;

namespace Covid19DB
{
    class Program
    {
        static void Main(string[] args)
        {
            var dailyReportsFolder = @"C:\Code\COVID-19\csse_covid_19_data\csse_covid_19_daily_reports";

            foreach (var fileName in Directory.GetFiles(dailyReportsFolder, "*.csv"))
            {
                //Get the date
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var tokens = fileNameWithoutExtension.Split('-').Select(t => int.Parse(t)).ToList();
                var date = new DateTimeOffset(tokens[2], tokens[0], tokens[1], 0, 0, 0, default);

                //Read the text
                var text = File.ReadAllText(fileName);
            }
        }
    }
}
