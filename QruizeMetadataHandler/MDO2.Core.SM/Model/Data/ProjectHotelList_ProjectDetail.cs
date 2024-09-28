using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.SM.Model.Data
{
    public class ProjectHotelList_ProjectDetail
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("project_id")]
        public int ProjectId { get; set; }

        [JsonProperty("entity_id")]
        public int EntityId { get; set; }

        [JsonProperty("project_name")]
        public string ProjectName { get; set; }

        [JsonProperty("project_type")]
        public string ProjectType { get; set; }

        [JsonProperty("smartscan_required")]
        public bool SmartScanRequired { get; set; }

        [JsonProperty("hotel_list")]
        public List<ProjectHotelList_Hotel> HotelList { get; set; }
    }
}
