using System;

namespace MDO2.Core.SM
{

    [Serializable]
    public class SMClientException : Exception
    {
        public SMClientException() { }
        public SMClientException(string message) : base(message) { }
        public SMClientException(string message, Exception inner) : base(message, inner) { }
        protected SMClientException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
