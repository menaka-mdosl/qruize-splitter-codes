using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public class QruizeXpsToPdfSystemErrorEventData : EventDataSystemError
    {
        [JsonProperty("s3BucketName")]
        public string S3BucketName { get; set; }

        [JsonProperty("s3Key")]
        public string S3Key { get; set; }

        [JsonProperty("s3KeyMedatada")]
        public string S3KeyMedatada { get; set; }

        [JsonProperty("maxAttempts")]
        public int MaxAttempts { get; set; }
    }
}
