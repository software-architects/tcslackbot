using System;

namespace TCSlackbot.Logic.Authentication.Exceptions
{
    /// <summary>
    /// Thrown whenever the users gets logged out because of an error.
    /// </summary>
    public class LoggedOutException : Exception
    {
        public LoggedOutException(string message) : base(message)
        {
        }

        public LoggedOutException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public LoggedOutException()
        {
        }
    }
}
