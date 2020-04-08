using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
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
                var numbers = fileNameWithoutExtension.Split('-').Select(t => int.Parse(t)).ToList();
                var date = new DateTimeOffset(numbers[2], numbers[0], numbers[1], 0, 0, 0, default);

                //Read the text
                var text = File.ReadAllText(fileName);

                var lines = text.Split("\r\n").ToList();

                var headerNames = lines[0].Split(',').ToList();

                var confirmedIndex = headerNames.IndexOf(nameof(RawModel.Confirmed));
                var deathsIndex = headerNames.IndexOf(nameof(RawModel.Deaths));

                var rowModels = new List<RawModel>();

                for (var i = 1; i< lines.Count;i++)
                {
                    var tokens = lines[i].Split(',').ToList();

                    if (tokens.Count != headerNames.Count) continue;

                    var confirmedText = tokens[confirmedIndex];
                    var deathsText = tokens[deathsIndex];

                    if (string.IsNullOrEmpty(confirmedText) && string.IsNullOrEmpty(deathsText)) continue;

                    var rawModel = new RawModel { Confirmed = int.Parse(confirmedText) };

                    rowModels.Add(rawModel);
                }


            }
        }
    }
}
