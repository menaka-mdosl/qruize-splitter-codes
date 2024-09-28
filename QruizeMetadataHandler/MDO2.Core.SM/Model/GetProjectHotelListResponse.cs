using MDO2.Core.SM.Model.Data;
using Newtonsoft.Json;

namespace MDO2.Core.SM.Model
{
    public class GetProjectHotelListResponse : BaseSMResponse
    {
        [JsonProperty("project_details")]
        public ProjectHotelList_ProjectDetail ProjectDetails { get; set; }
    }
}
