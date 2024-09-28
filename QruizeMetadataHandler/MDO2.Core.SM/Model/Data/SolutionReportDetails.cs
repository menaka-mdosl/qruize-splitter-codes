using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.SM.Model.Data
{
    public class SolutionReportDetails
    {
        public SolutionReportDetails()
        {
            ReportsList = new List<SolutionReport>();
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("reportsList")]
        public List<SolutionReport> ReportsList { get; set; }
    }
}
