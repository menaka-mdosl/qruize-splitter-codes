using MDO2.Core.SM.Model.Data;
using Newtonsoft.Json;

namespace MDO2.Core.SM.Model
{
    public class GetHMGProjectListResponse : BaseSMResponse
    {
        [JsonProperty("hmg_details")]
        public HmgDetails HmgDetails { get; set; }
    }
}
