using Newtonsoft.Json;

namespace MDO2.Core.SM.Model
{
    public abstract class BaseSMResponse
    {

        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

}
