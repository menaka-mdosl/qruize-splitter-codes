using Amazon.S3.Util;
using MDO2.Core.Model.Metadata;
using QruizeBatchReportHandler.Core.Splitting.Model;
using System.Text.Json.Serialization;
namespace QruizeBatchReportHandler.Core.App.Model
{
    public class ProcessedFileEntry
    {
        public bool Processed { get; set; }

        public List<MessegeSettings> ProcessEvent { get; internal set; }
        public string FailedAt { get;  set; }
        public Exception Error { get;  set; }
        public ConvertedDocument ConvertedDocument { get; internal set; }
    }
}