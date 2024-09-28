using Newtonsoft.Json;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public class ConverterFileProcessedEventData : ConverterEventDataBase
    {
        [JsonProperty("convertedDocId")]
        public string ConvertedDocId { get; set; }
    }
}
