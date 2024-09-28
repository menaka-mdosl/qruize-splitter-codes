using Newtonsoft.Json;

namespace MDO2.Core.SM.Model.Data
{
    public partial class HMGProjectDetails
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("project_id")]
        public long? ProjectId { get; set; }

        [JsonProperty("project_name")]
        public string ProjectName { get; set; }

        [JsonProperty("project_type")]
        public string ProjectType { get; set; }

        [JsonProperty("smartscan_required")]
        public bool SmartscanRequired { get; set; }

        [JsonProperty("smart_scaning_timed_out")]
        public string SmartScaningTimedOut { get; set; }

        [JsonIgnore]
        public string ProjectNameWithType
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ProjectType))
                    return $"{ProjectName} - {ProjectType}";
                else
                    return ProjectName;
            }
        }
    }
}
