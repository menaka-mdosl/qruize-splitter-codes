using Newtonsoft.Json;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public class ConverterFileInEventData : ConverterEventDataBase
    {
        public ConverterFileInEventData()
        {
            S3BucketName = "";
            S3KeyMedatada = "";
            S3Key = "";
        }

        [JsonProperty("s3BucketName")]
        public string S3BucketName { get; set; }

        [JsonProperty("s3KeyMedatada")]
        public string S3KeyMedatada { get; set; }

        [JsonProperty("s3Key")]
        public string S3Key { get; set; }
    }
}
