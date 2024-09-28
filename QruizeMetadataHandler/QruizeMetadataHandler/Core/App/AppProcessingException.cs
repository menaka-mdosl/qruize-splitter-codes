namespace QruizeMetadataHandler.Core.App
{

    [Serializable]
    public class AppProcessingException : Exception
    {
        public AppProcessingException() { }
        public AppProcessingException(string message) : base(message) { }
        public AppProcessingException(string message, Exception inner) : base(message, inner) { }
        protected AppProcessingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
