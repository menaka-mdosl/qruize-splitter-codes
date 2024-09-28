using QruizeMetadataHandler.Core.App.Model;
using MDO2.Core.Model.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace QruizeMetadataHandler.Core.Splitting.Model
{
    public class ApiData
    {
        
        public string newDocId { get; set; }
        public string metaBucketName { get; set; }
        public string metaKey { get; set; }
        public string fileBucketName { get; set; }
        public string fileKey { get; set; }
        public string s3Key { get; set; }
        public string s3Url { get; set; }
        public Meta meta { get; set; }

    }

    public class Meta
    {
        [JsonProperty("Report Name")]
        public string ReportName { get; set; }
        [JsonProperty("Report Category")]
        public string ReportCategory { get; set; }
        [JsonProperty("Category")]
        public string Category { get; set; }
    }
}
