using Newtonsoft.Json;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public class ConverterFileInEventData : ConverterEventDataBase
    {
        public ConverterFileInEventData()
        {
            Hotel = "";
            MgmtGroup = "";
            BusinessDate = "";
        }
        [JsonProperty("hotel")]
        public string Hotel { get; set; }
        [JsonProperty("mgmtGroup")]
        public string MgmtGroup { get; set; }
        [JsonProperty("businessDate")]
        public string BusinessDate { get; set; }
    }
}
