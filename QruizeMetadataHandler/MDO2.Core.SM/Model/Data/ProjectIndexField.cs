using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.SM.Model.Data
{
    public class ProjectIndexField
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("wildcard")]
        public long? Wildcard { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("required")]
        public bool IndexFieldRequired { get; set; }

        [JsonProperty("defaultIndex")]
        public bool DefaultIndex { get; set; }

        [JsonProperty("availableInIS")]
        public bool AvailableInIs { get; set; }

        [JsonProperty("editable", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Editable { get; set; }

        [JsonProperty("values")]
        public List<string> Values { get; set; }

        [JsonProperty("valuesType")]
        public string ValuesType { get; set; }

        [JsonProperty("appLevelFormat", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> AppLevelFormat { get; set; }

        [JsonProperty("order", NullValueHandling = NullValueHandling.Ignore)]
        public long? Order { get; set; }
    }
}
