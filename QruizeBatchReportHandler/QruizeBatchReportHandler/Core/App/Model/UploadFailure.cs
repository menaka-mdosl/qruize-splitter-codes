using QruizeBatchReportHandler.Core.Splitting.Model;

namespace QruizeBatchReportHandler.Core.App.Model
{
    internal class UploadFailure
    {
        public ConvertedDocument Document { get; internal set; }
        public Exception Error { get; internal set; }
    }
}
