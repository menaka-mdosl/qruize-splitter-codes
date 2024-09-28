using MDO2.Core.QMS.Model.Message.EventData;
using MDO2.Core.QMS.Model.Message.EventData.DataElements;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.QMS.Model.Message
{
    public class EventDataFileAcceptance : IEventData
    {
        public EventDataFileAcceptance()
        {
            Projects = new List<DataElementProject>();
            Indexes = new List<DataElementMetadataIndex>();
        }

        [JsonProperty("mgmtGroup")]
        public string ManagementGroup { get; set; }
        [JsonProperty("project")]
        public List<DataElementProject> Projects { get; set; }
        [JsonProperty("docId")]
        public string DocId { get; set; }
        [JsonProperty("chainId")]
        public string ChainId { get; set; }
        [JsonProperty("parentDocId")]
        public string ParentDocId { get; set; }
        [JsonProperty("fileName")]
        public string FileName { get; set; }
        [JsonProperty("fileExtension")]
        public string FileExtension { get; set; }
        [JsonProperty("fileSize")]
        public long FileSize { get; set; }
        [JsonProperty("smartscanRequired")]
        public bool SmartscanRequired { get; set; }
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("userEmail")]
        public string UserEmail { get; set; }
        [JsonProperty("userId")]
        public string UserId { get; set; }
        [JsonProperty("acceptanceState")]
        public string AcceptanceState { get; set; }
        [JsonProperty("acceptanceMeta")]
        public string AcceptanceMeta { get; set; }

        [JsonProperty("s3BucketName")]
        public string S3Bucket { get; set; }
        [JsonProperty("s3Key")]
        public string S3Key { get; set; }
        [JsonProperty("s3KeyMedatada")]
        public string S3KeyMetadata { get; set; }

        [JsonProperty("solutions")]
        public string Solutions { get; set; }

        [JsonProperty("indexes")]
        public List<DataElementMetadataIndex> Indexes { get; set; }
    }
}
