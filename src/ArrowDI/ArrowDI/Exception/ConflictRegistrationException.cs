using System;

namespace ArrowDI
{
    public class ConflictRegistrationException : Exception
    {
        public ConflictRegistrationException() { }
        public ConflictRegistrationException(string message) : base(message) { }
        public ConflictRegistrationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
