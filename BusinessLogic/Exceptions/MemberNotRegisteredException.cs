using System;

namespace BusinessLayer.Exceptions
{
    public class MemberNotRegisteredException : Exception
    {
        public MemberNotRegisteredException(string message) : base(message)
        {
        }
    }
}