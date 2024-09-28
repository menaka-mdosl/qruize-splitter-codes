using System;

namespace MDO2.Core.LMD.S3
{

    [Serializable]
    public class S3ClientException : Exception
    {
        public S3ClientException() { }
        public S3ClientException(string message) : base(message) { }
        public S3ClientException(string message, Exception inner) : base(message, inner) { }
        protected S3ClientException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
