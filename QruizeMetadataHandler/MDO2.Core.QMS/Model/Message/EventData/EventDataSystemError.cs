using Newtonsoft.Json;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public class EventDataSystemError : IEventData
    {
        [JsonProperty("failedAt")]
        public string FailedAt { get; set; }

        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty("exceptionData")]
        public string ExceptionData { get; set; }

        [JsonProperty("docId")]
        public string DocId { get; set; }

        [JsonProperty("chainId")]
        public string ChainId { get; set; }

        [JsonProperty("parentDocId")]
        public string ParentDocId { get; set; }
    }
}
