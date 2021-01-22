using System;

namespace BusinessLayer.Exceptions
{
    public class MaxBorrowedBooksExceededException : Exception
    {
        public MaxBorrowedBooksExceededException(string message) : base(message)
        {
        }
    }
}