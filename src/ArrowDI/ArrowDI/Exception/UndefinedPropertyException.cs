using System;

namespace ArrowDI
{
    public class UndefinedPropertyException : Exception
    {
        public UndefinedPropertyException() { }
        public UndefinedPropertyException(string message) : base(message) { }
        public UndefinedPropertyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
