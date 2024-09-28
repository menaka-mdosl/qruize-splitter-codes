namespace QruizeMetadataHandler.Core.App.Model
{
    public sealed class QmsErrorEventData
    {
        public string? ErrorMessage { get; set; }
        public string? FailedAt { get; set; }
        public Exception? Exception { get; set; }
        public string? ErrorCode { get; set; }
    }
}
