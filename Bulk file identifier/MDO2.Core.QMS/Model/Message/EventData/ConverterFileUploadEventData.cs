using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public class ConverterFileUploadEventData : ConverterEventDataBase
    {
        [JsonProperty("convertedDocId")]
        public string ConvertedDocId { get; set; }

        [JsonProperty("uploadStatus")]
        public string UploadStatus { get; set; }

        [JsonProperty("statusMessage")]
        public string StatusMessage { get; set; }

        [JsonProperty("reportList")]
        public List<IndexedReportDataElements> ReportList { get; set; } = new List<IndexedReportDataElements>();
    }
}
