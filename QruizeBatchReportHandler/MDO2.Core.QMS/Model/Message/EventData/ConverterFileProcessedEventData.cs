using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public class ConverterFileProcessedEventData : ConverterEventDataBase
    {

        [JsonProperty("reportList")]
        public List<IndexedReportDataElements> ReportList { get; set; } = new List<IndexedReportDataElements>();
    }

    public class IndexedReportDataElements
    {
        [JsonProperty("docId")]
        public string DocId { get; set; }
        [JsonProperty("reportName")]
        public string ReportName { get; set; }
        [JsonProperty("businessDate")]
        public string BusinessDate { get; set; }
    }
}
