using MDO2.Core.SM.Model.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.SM.Model
{
    public class GetAllHMGResponse : BaseSMResponse
    {
        public GetAllHMGResponse()
        {
            HMGList = new List<HMG>();
        }

        [JsonProperty("hmg_list")]
        public List<HMG> HMGList { get; set; }
    }
}
