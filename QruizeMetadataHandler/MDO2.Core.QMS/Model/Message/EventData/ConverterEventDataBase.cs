using Newtonsoft.Json;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public abstract class ConverterEventDataBase : IEventData
    {
        [JsonProperty("docId")]
        public string DocId { get; set; }
        [JsonProperty("chainId")]
        public string ChainId { get; set; }
        [JsonProperty("parentDocId")]
        public string ParentDocId { get; set; }
    }
}
