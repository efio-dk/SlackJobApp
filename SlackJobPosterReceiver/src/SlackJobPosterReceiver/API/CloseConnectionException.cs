using System;

namespace SlackJobPosterReceiver.API
{
    public class CloseConnectionException : Exception
    {
        public CloseConnectionException() : base()
        {
        }

        public CloseConnectionException(string message) : base(message)
        {
        }

        public CloseConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}