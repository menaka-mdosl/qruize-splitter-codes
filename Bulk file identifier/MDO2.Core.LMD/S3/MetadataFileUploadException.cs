using System;

namespace MDO2.Core.LMD.S3
{

    [Serializable]
    public class MetadataFileUploadException : Exception
    {
        public MetadataFileUploadException() { }
        public MetadataFileUploadException(string message) : base(message) { }
        public MetadataFileUploadException(string message, Exception inner) : base(message, inner) { }
        protected MetadataFileUploadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
