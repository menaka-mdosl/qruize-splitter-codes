using Newtonsoft.Json;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public class ConverterQcEventData : ConverterEventDataBase
    {
        public const string QC_TYPE_FORMAT_NOT_SUPPORTED = "FORMAT_NOT_SUPPORTED";

        [JsonProperty("qcType")]
        public string QcType { get; set; }
        [JsonProperty("fileExtension")]
        public string FileExtension { get; set; }
    }

}
