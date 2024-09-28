using System;

namespace MDO2.Core.SM
{

    [Serializable]
    public class SMCryptographyOperationException : Exception
    {
        public SMCryptographyOperationException() { }
        public SMCryptographyOperationException(string message) : base(message) { }
        public SMCryptographyOperationException(string message, Exception inner) : base(message, inner) { }
        protected SMCryptographyOperationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
