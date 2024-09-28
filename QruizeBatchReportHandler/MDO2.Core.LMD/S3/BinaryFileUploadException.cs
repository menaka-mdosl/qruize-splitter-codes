using System;

namespace MDO2.Core.LMD.S3
{

    [Serializable]
    public class BinaryFileUploadException : Exception
    {
        public BinaryFileUploadException() { }
        public BinaryFileUploadException(string message) : base(message) { }
        public BinaryFileUploadException(string message, Exception inner) : base(message, inner) { }
        protected BinaryFileUploadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
