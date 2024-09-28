using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.SM.Model.Data
{
    public class ProjectIndexDetails
    {
        [JsonProperty("_id")]
        public long Id { get; set; }

        [JsonProperty("entityId")]
        public long? EntityId { get; set; }

        [JsonProperty("projectId")]
        public long? ProjectId { get; set; }

        [JsonProperty("projectName")]
        public string ProjectName { get; set; }

        [JsonProperty("indexFields")]
        public List<ProjectIndexField> IndexFields { get; set; }

        [JsonProperty("hotelLocations")]
        public Dictionary<string, string> HotelLocations { get; set; }
    }
}
