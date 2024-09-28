using Newtonsoft.Json;

namespace MDO2.Core.SM.Model.Data
{
    public class GetIndexMapDataAutoPopulate
    {
        [JsonProperty("indexName")]
        public string IndexName { get; set; }

        [JsonProperty("indexType")]
        public string IndexType { get; set; }

        [JsonProperty("indexFormat")]
        public string IndexFormat { get; set; }

        [JsonProperty("indexValue")]
        public string IndexValue { get; set; }
    }
}
