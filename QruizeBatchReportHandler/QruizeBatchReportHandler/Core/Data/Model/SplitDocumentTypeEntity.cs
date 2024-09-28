using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QruizeBatchReportHandler.Core.Data.Model
{
    public class SplitDocumentTypeEntity:BaseEntity
    {

        public SplitDocumentTypeEntity()
        {
            ReportNames = new List<string>();
        }

        [BsonElement("PMS")]
        public string PMS { get; set; }

        [BsonElement("ManagementGroup")]
        public string ManagementGroup { get; set; }

        [BsonElement("Hotel")]
        public string Hotel { get; set; }

        [BsonElement("ReportSeperatorType")]
        public string ReportSeperatorType { get; set; }

        [BsonElement("FileExtension")]
        public string FileExtension { get; set; }

        [BsonElement("ReportNames")]
        public List<string> ReportNames { get; set; }

        [BsonElement("KeyWords")]
        public string KeyWords { get; set; }

        [BsonElement("WordCount")]
        public string WordCount { get; set; }

        [BsonElement("UnmatchedLinesCount")]
        public string UnmatchedLinesCount { get; set; }

        [BsonElement("LineCount")]
        public string LineCount { get; set; }

        [BsonElement("BarcodeValuesExceptReportName")]
        public string BarcodeValuesExceptReportName { get; set; }
        
        [BsonElement("SplittingTypeOrder")]
        public string SplittingTypeOrder { get; set; }

    }
}
