using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.SM.Model.Data
{
    public class GetIndexMapDataProject
    {
        [JsonProperty("project_id")]
        public int ProjectId { get; set; }

        [JsonProperty("project_name")]
        public string ProjectName { get; set; }

        [JsonProperty("project_type")]
        public string ProjectType { get; set; }

        [JsonProperty("indexes_mapping")]
        public List<GetIndexMapDataIndexesMapping> IndexMappings { get; set; }
    }
}
