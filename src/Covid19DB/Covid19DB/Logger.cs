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
            var countAnomaly = (CountAnomaly)stateObject;
            Console.WriteLine($"LocationId: {countAnomaly.LocationId} Date: {countAnomaly.Date} Type: {countAnomaly.ColumnName} Count: {Count}");
        }
    }

}
