using Covid19DB.Models.Logging;
using Microsoft.Extensions.Logging;
using System;

namespace Covid19DB
{
    public class Logger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        private int Count = 0;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Count++;
            var stateObject = (object)state;

            if (stateObject is CountAnomaly countAnomaly)
            {
                Console.Write($"Count Anomaly LocationId: {countAnomaly.LocationId} Date: {countAnomaly.Date} Type: {countAnomaly.ColumnName} Count: {Count} ");
            }

            if (stateObject is IncorrectCountValue incorrectCount)
            {
                Console.Write($"Count Anomaly Date: {incorrectCount.Date} Confirmed: {Count} ");
            }

            if (stateObject is ValidationWarningBase validationWarning)
            {
                var month = validationWarning.Date.Month.ToString().PadLeft(2, '0');
                var day = validationWarning.Date.Day.ToString().PadLeft(2, '0');
                var url = $"https://github.com/CSSEGISandData/COVID-19/blob/master/csse_covid_19_data/csse_covid_19_daily_reports/{month}-{day}-2020.csv#L{validationWarning.CsvRowNumber}";
                Console.Write($"{url}\r\n");
            }
        }
    }

}
