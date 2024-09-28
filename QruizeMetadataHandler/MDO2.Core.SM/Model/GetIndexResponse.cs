using MDO2.Core.SM.Model.Data;
using Newtonsoft.Json;

namespace MDO2.Core.SM.Model
{
    public class GetIndexResponse : BaseSMResponse
    {
        [JsonProperty("indexes")]
        public ProjectIndexDetails Indexes { get; set; }
    }
}
