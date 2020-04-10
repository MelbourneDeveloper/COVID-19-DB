using System;
using System.Runtime.Serialization;

namespace Covid19DB
{
    [Serializable]
    public class RowValidationException : Exception
    {
        public RowValidationException()
        {
        }

        public RowValidationException(string message) : base(message)
        {
        }

        public RowValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RowValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}