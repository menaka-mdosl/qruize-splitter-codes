using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.SM.Model.Data
{
    public class GetIndexMapDataIndexesMapping
    {
        [JsonProperty("selection")]
        public List<GetIndexMapDataSelection> Selection { get; set; }

        [JsonProperty("autoPopulate")]
        public List<GetIndexMapDataAutoPopulate> AutoPopulate { get; set; }
    }
}
