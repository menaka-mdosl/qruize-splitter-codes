using Newtonsoft.Json;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public class ConverterQcEventData : ConverterEventDataBase
    {
        public const string QC_TYPE_FORMAT_PROTECTED_FILE = "FORMAT_PROTECTED_FILE";
        public const string QC_TYPE_FORMAT_EMPTY_DOCUMENT = "ZERO_FILE_SIZE";
        public const string QC_TYPE_FORMAT_CORRUPTED_FILE = "FORMAT_CORRUPTED_FILE";

        [JsonProperty("qcType")]
        public string QcType { get; set; }
        [JsonProperty("fileExtension")]
        public string FileExtension { get; set; }
        public string FileSize { get; set; }
        public string protectionTypeDesc { get; set; }

        public bool canContinue { get; set; }
    }

}
