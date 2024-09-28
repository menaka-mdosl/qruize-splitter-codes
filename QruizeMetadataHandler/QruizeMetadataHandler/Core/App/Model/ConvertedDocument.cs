using QruizeMetadataHandler.Core.App.Model;
using MDO2.Core.Model.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QruizeMetadataHandler.Core.Splitting.Model
{
    public class ConvertedDocument
    {
        
        public string NewDocId { get; set; }
        public string NewFileName { get; set; }
        public string NewMetadataName { get; set; }
        public string ConvertedBucketName { get; set; }
        public string NewS3Key { get; set; }
    }
}
