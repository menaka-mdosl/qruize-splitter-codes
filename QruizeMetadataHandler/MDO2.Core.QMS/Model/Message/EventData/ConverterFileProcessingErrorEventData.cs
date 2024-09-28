using Newtonsoft.Json;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public class ConverterFileProcessingErrorEventData : ConverterEventDataBase
    {
        [JsonProperty("failedAt")]
        public string FailedAt { get; set; }
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
        [JsonProperty("exceptionData")]
        public string ExceptionData { get; set; }
    }
}
