using Newtonsoft.Json;

namespace MDO2.Core.SM.Model.Data
{
    public class ProjectHotelList_Hotel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        //[JsonProperty("can_sign")]
        //public string CanSign { get; set; }

        //[JsonProperty("can_upload")]
        //public string CanUpload { get; set; }

        //[JsonProperty("go_live_date")]
        //public string GoLiveDate { get; set; }

        [JsonProperty("ISName")]
        public string ISName { get; set; }

        //[JsonProperty("signature_go_live_date")]
        //public string SignatureGoLiveDate { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("pms")]
        public string[] PMS { get; set; }
        //[JsonProperty("sign_type")]
        //public string SignType { get; set; }
    }
}
