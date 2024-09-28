using Newtonsoft.Json;

namespace MDO2.Core.QMS.Model.Message.EventData.DataElements
{
    public class DataElementMetadataIndex : IDataElement
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}