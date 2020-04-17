using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Covid19DB.Utilities
{
    public static class CsvHelpers
    {
        /// <summary>
        /// Writes a collection of data out to a CSV file
        /// </summary>
        public static void ToCsv<T>(this IEnumerable<T> models, string filePath)
        {
            if (models == null) throw new ArgumentNullException(nameof(models));

            using var fileStream = File.OpenWrite(filePath);

            var headers = typeof(T).GetProperties();
            var headerRow = string.Join(',', headers.Select(h => h.Name)) + "\r\n";

            WriteText(fileStream, headerRow);

            foreach (var row in models)
            {
                var rowText = string.Join(',', headers.Select(h =>
                {
                    var value = h.GetValue(row, null);
                    return value?.ToString();
                })) + "\r\n";
                WriteText(fileStream, rowText);
            }
        }

        public static void ToMarkdownTable<T>(this IEnumerable<T> models, string filePath)
        {
            if (models == null) throw new ArgumentNullException(nameof(models));

            using var fileStream = File.OpenWrite(filePath);

            var headers = typeof(T).GetProperties();
            var headerRow = "|" + string.Join('|', headers.Select(h => h.Name)) + "|\r\n";

            var intermediateRow = "|" + string.Join('|', headers.Select(h => "--")) + "|\r\n";

            WriteText(fileStream, headerRow);
            WriteText(fileStream, intermediateRow);

            foreach (var row in models)
            {
                var rowText = string.Join('|', headers.Select(h =>
                {
                    var value = h.GetValue(row, null);

                    if (value is DateTimeOffset dateTimeOffset)
                    {
                        value = dateTimeOffset.ToString("yyyy/MM/dd");
                    }

                    return value?.ToString();
                }));
                WriteText(fileStream, "|" + rowText + "|\r\n");
            }
        }

        private static void WriteText(FileStream fileStream, string headerRow)
        {
            var bytes = Encoding.UTF8.GetBytes(headerRow);
            fileStream.Write(bytes, 0, bytes.Length);
        }
    }
}
