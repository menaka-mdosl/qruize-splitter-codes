using MDO2.Core.SM.Model.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.SM.Model
{
    public class GetReportListResponse : BaseSMResponse
    {
        public GetReportListResponse()
        {
            ReportInformation = new List<SolutionReportDetails>();
        }


        [JsonProperty("reports_information")]
        public List<SolutionReportDetails> ReportInformation { get; set; }

        [JsonProperty("file_mapping_information")]
        public SolutionFileMappingInformation FileMapping { get; set; }
    }
}
