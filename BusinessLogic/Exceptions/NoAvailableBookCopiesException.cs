using System;

namespace BusinessLayer.Exceptions
{
    public class NoAvailableBookCopiesException: Exception
    {
        public NoAvailableBookCopiesException(string message): base(message)
        {
        }
    }
}