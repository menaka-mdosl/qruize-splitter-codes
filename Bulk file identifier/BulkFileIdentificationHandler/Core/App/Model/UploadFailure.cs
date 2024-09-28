using BulkFileIdentificationHandler.Core.Splitting.Model;

namespace BulkFileIdentificationHandler.Core.App.Model
{
    internal class UploadFailure
    {
        public ConvertedDocument Document { get; internal set; }
        public Exception Error { get; internal set; }
    }
}
