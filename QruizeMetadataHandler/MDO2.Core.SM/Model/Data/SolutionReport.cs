using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.SM.Model.Data
{
    public class SolutionReport
    {
        [JsonProperty("reportId")]
        public string ReportId { get; set; }

        [JsonProperty("reportName")]
        public string ReportName { get; set; }

        [JsonProperty("matchCondition")]
        public string MatchCondition { get; set; }

        [JsonProperty("schedule")]
        public string Schedule { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("pmsReportName")]
        public string PMSReportName { get; set; }

        [JsonProperty("fileName")]
        public List<string> FileName { get; set; }

        [JsonProperty("reportCategory")]
        public List<string> ReportCategory { get; set; }

        [JsonProperty("extensionMatchCondition")]
        public string ExtensionMatchCondition { get; set; }

        [JsonProperty("fileExtension")]
        public List<string> FileExtension { get; set; }

        [JsonProperty("smartscanRequired")]
        public bool SmartscanRequired { get; set; }

        [JsonProperty("signature")]
        public bool? Signature { get; set; }

        [JsonProperty("businessDate")]
        public string BusinessDateCalc { get; set; }
    }
}
