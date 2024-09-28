using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.SM.Model.Data
{
    public class HMG
    {
        [JsonProperty("hmg_id")]
        public string HmgId { get; set; }

        [JsonProperty("hmg_name")]
        public string HmgName { get; set; }

        [JsonProperty("entity_id")]
        public int? EntityId { get; set; }

        [JsonProperty("myp_id")]
        public int? MYPId { get; set; }

        [JsonProperty("cmp_id")]
        public int? CMPId { get; set; }

        [JsonProperty("signature_type")]
        public string SignatureType { get; set; }

        [JsonProperty("smartscan_required")]
        public bool SmartscanRequired { get; set; }

        [JsonProperty("hmg_22")]
        public bool Hmg22 { get; set; }

        [JsonProperty("solutions")]
        public List<Solution> Solutions { get; set; }
    }
}
