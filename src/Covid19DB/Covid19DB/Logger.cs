using Covid19DB.Models.Logging;
using Covid19DB.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Covid19DB
{
    public class Logger<T> : ILogger<T>
    {
        private readonly List<CasesRowInbalance> _casesRowInbalances = new List<CasesRowInbalance>();
        private readonly List<CaseRowAdjustment> _casesRowAdjustments = new List<CaseRowAdjustment>();

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var stateObject = (object)state;

            if (stateObject is CaseRowAdjustment countAnomaly)
            {
                _casesRowAdjustments.Add(countAnomaly);
            }

            if (stateObject is CasesRowInbalance incorrectCount)
            {
                _casesRowInbalances.Add(incorrectCount);
            }
        }

        public void ToCsv()
        {
            _casesRowInbalances.ToCsv("CasesRowInbalances.csv");
            _casesRowAdjustments.ToCsv("CasesRowAdjustments.csv");
        }
    }

}
