using Newtonsoft.Json;

namespace MDO2.Core.QMS.Model.Message.EventData.DataElements
{
    public class DataElementProject : IDataElement
    {
        [JsonProperty("entityId")]
        public string EntityID { get; set; }
        [JsonProperty("projectId")]
        public string ProjectID { get; set; }
        [JsonProperty("projectType")]
        public string ProjectType { get; set; }
    }
}