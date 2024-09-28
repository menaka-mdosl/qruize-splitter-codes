using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public class FailuresReportDataElements
    {
        [JsonProperty("page")]
        public int Page { get; set; }
        [JsonProperty("indexName")]
        public string IndexName { get; set; }
        [JsonProperty("capturedText")]
        public string CapturedText { get; set; }
        [JsonProperty("confidenceValue")]
        public double ConfidenceValue { get; set; }
        [JsonProperty("substitutedValue")]
        public string SubstitutedValue { get; set; }

    }
    public class ConverterQcEventData : ConverterEventDataBase
    {
        [JsonProperty("reportList")]
        public List<FailuresReportDataElements> ReportList { get; set; } = new List<FailuresReportDataElements>();

        public const string QC_TYPE_FORMAT_PROTECTED_FILE = "FORMAT_PROTECTED_FILE";
        public const string QC_TYPE_FORMAT_EMPTY_DOCUMENT = "ZERO_FILE_SIZE";
        public const string QC_TYPE_FORMAT_CORRUPTED_FILE = "FORMAT_CORRUPTED_FILE";
        public const string QC_TYPE_FORMAT_SUBSTITUE_REPORT_NAME = "SUBSTITUE_REPORT_NAME";

        [JsonProperty("qcType")]
        public string QcType { get; set; }
        [JsonProperty("fileExtension")]
        public string FileExtension { get; set; }

        [JsonProperty("fileType")]
        public string FileType { get; set; }
        [JsonProperty("bulkType")]
        public string BulkType { get; set; }
        public string FileSize { get; set; }
        public string protectionTypeDesc { get; set; }

        public bool canContinue { get; set; }
    }

}
