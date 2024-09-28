namespace QruizeMetadataHandler.Core.App
{
    [Serializable]
    public class S3FileDownloadException : Exception
    {
        public S3FileDownloadException() { }
        public S3FileDownloadException(string message) : base(message) { }
        public S3FileDownloadException(string message, Exception inner) : base(message, inner) { }
        protected S3FileDownloadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
