using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Covid19DB
{
    public class CsvReader
    {
        public IEnumerable<RowModel> ReadCsvFiles(string dailyReportsFolder)
        {
            var modelsByDate = new Dictionary<DateTimeOffset, List<RowModel>>();

            //Iterate through the files
            foreach (var fileName in Directory.GetFiles(dailyReportsFolder, "*.csv"))
            {
                //Get the date
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var numbers = fileNameWithoutExtension.Split('-').Select(int.Parse).ToList();
                var date = new DateTimeOffset(numbers[2], numbers[0], numbers[1], 0, 0, 0, default);

                var rawModels = ProcessFile(fileName, date);

                modelsByDate.Add(date, rawModels);
            }

            //Sort lists by date
            var modelsLists = modelsByDate.Keys.OrderBy(k => k).Select(key => modelsByDate[key]);

            //Aggregate all data
            var rows = modelsLists.Aggregate((a, b) =>
            {
                var aggregatedList = new List<RowModel>(a);
                aggregatedList.AddRange(b);
                return aggregatedList;
            });

            return rows;
        }

        private List<RowModel> ProcessFile(string fileName, DateTimeOffset date)
        {
            using var parser = new TextFieldParser(fileName) {TextFieldType = FieldType.Delimited};

            parser.SetDelimiters(",");

            var headerNames = parser.ReadFields().ToList();

            var confirmedIndex = headerNames.IndexOf(nameof(RowModel.Confirmed));
            var deathsIndex = headerNames.IndexOf(nameof(RowModel.Deaths));

            var countryRegionIndex = headerNames.IndexOf(nameof(RowModel.Country_Region));
            //ISSUE: Deal with inconsistent header names
            if (countryRegionIndex == -1) countryRegionIndex = headerNames.IndexOf("Country/Region");

            var provinceStateIndex = headerNames.IndexOf(nameof(RowModel.Province_State));
            //ISSUE: Deal with inconsistent header names
            if (provinceStateIndex == -1) provinceStateIndex = headerNames.IndexOf("Province/State");

            var latitudeIndex = headerNames.IndexOf(nameof(RowModel.Lat));
            var longitudeIndex = headerNames.IndexOf(nameof(RowModel.Long_));

            var admin2Index = headerNames.IndexOf(nameof(RowModel.Admin2));

            var recoveredIndex = headerNames.IndexOf(nameof(RowModel.Recovered));


            var rawModels = new List<RowModel>();

            //Number is 1 based and matches tyhe Github line
            var i = 2;

            //Iterate through the lines in the file
            while (!parser.EndOfData)
            {
                var tokens = parser.ReadFields().ToList();

                if (tokens.Count != headerNames.Count)
                {
                    throw new Exception($"Filename: {fileName} Headers: {headerNames.Count} Tokens: {tokens.Count} Line: {i + 1}");
                }

                var rawModel = ProcessRow(date, confirmedIndex, deathsIndex, countryRegionIndex, provinceStateIndex, latitudeIndex, longitudeIndex, admin2Index, recoveredIndex, tokens, headerNames);

                if (rawModel != null) rawModels.Add(rawModel);

                i++;
            }

            return rawModels;
        }

        private static RowModel ProcessRow(DateTimeOffset date, int confirmedIndex, int deathsIndex, int countryRegionIndex, int provinceStateIndex, int latitudeIndex, int longitudeIndex, int admin2Index, int recoveredIndex, IReadOnlyList<string> tokens, List<string> headerNames)
        {
            var confirmedText = tokens[confirmedIndex];
            var deathsText = tokens[deathsIndex];
            var recoveredText = tokens[recoveredIndex];

            string latitudeText = null;
            if (latitudeIndex > -1)
            {
                latitudeText = tokens[latitudeIndex];
            }

            string longitudeText = null;
            if (longitudeIndex > -1)
            {
                longitudeText = tokens[longitudeIndex];
            }

            if (string.IsNullOrEmpty(confirmedText) && string.IsNullOrEmpty(deathsText) && string.IsNullOrEmpty(recoveredText)) return null;

            decimal? latitude = null;
            if (!string.IsNullOrEmpty(latitudeText))
            {
                latitude = decimal.Parse(latitudeText);
            }

            decimal? longitude = null;
            if (!string.IsNullOrEmpty(longitudeText))
            {
                longitude = decimal.Parse(longitudeText);
            }

            string admin2Text = null;
            if (admin2Index > -1)
            {
                admin2Text = tokens[admin2Index];
            }

            return new RowModel
            {
                Confirmed = !string.IsNullOrEmpty(confirmedText) ? int.Parse(confirmedText) : (int?)null,
                Deaths = !string.IsNullOrEmpty(deathsText) ? int.Parse(deathsText) : (int?)null,
                Recovered = !string.IsNullOrEmpty(recoveredText) ? int.Parse(recoveredText) : (int?)null,
                Country_Region = tokens[countryRegionIndex],
                Province_State = tokens[provinceStateIndex],
                Lat = latitude,
                Long_ = longitude,
                Date = date,
                Admin2 = admin2Text
            };
        }

    }
}
