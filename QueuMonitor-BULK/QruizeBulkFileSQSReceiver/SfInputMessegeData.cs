using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QruizeBulkFileSQSReceiver
{
    public class SfInputMessegeData
    {
        public string s3BucketName { get; set; }
        public string s3KeyMedatada { get; set; }
        public string docId { get; set; }
        public string chainId { get; set; }
        public string fileExtension { get; set; }
        public string s3Key { get; set; }
        public string s3Url { get; set; }
        public string hotel { get; set; }
        public string hmg { get; set; }
        public string reportName { get; set; }
        public string connector { get; set; }
        public string entityId { get; set; }
        public string projectId { get; set; }
        public string newDocId { get; set; }
        public string newS3Key { get; set; }
        public string newS3Bucket { get; set; }
        public bool processed { get; set; }
        public string bulkType { get; set; }
    }
}
