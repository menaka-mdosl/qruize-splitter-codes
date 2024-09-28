using QruizeMetadataHandler.Core.Splitting.Model;

namespace QruizeMetadataHandler.Core.App.Model
{
    internal class UploadFailure
    {
        public ConvertedDocument Document { get; internal set; }
        public Exception Error { get; internal set; }
    }
}
