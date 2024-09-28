using MDO2.Core.SM.Model.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.SM.Model
{
    public class GetIndexMapDataResponse : BaseSMResponse
    {
        [JsonProperty("hmg_id")]
        public string HmgId { get; set; }

        [JsonProperty("entity_id")]
        public int EntityId { get; set; }

        [JsonProperty("hmg_name")]
        public string HmgName { get; set; }

        [JsonProperty("projects")]
        public List<GetIndexMapDataProject> Projects { get; set; }
    }
}
