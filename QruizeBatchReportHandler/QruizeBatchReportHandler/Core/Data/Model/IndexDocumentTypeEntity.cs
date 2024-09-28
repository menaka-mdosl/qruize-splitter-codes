using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QruizeBatchReportHandler.Core.Data.Model
{
    public class IndexDocumentTypeEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public IndexDocumentTypeEntity()
        {
            SubstitutionWords = new Dictionary<string, string>();
            ReportNames = new List<string>();
        }

        [BsonElement("ManagementGroup")]
        public string ManagementGroup { get; set; }

        [BsonElement("Hotel")]
        public string Hotel { get; set; }

        [BsonElement("PMS")]
        public string PMS { get; set; }

        [BsonElement("FileExtension")]
        public string FileExtension { get; set; }

        [BsonElement("ReportSeperatorType")]
        public string ReportSeperatorType { get; set; }

        [BsonElement("PageNumber")]
        public int PageNumber { get; set; }

        [BsonElement("IndexName")]
        public string IndexName { get; set; }

        [BsonElement("IndexingRule")]
        public string IndexingRule { get; set; }

        [BsonElement("Regex")]
        public string Regex { get; set; }

        [BsonElement("BeforeWord")]
        public string BeforeWord { get; set; }

        [BsonElement("AfterWord")]
        public string AfterWord { get; set; }

        [BsonElement("SubstitutionWords")]
        public Dictionary<string, string> SubstitutionWords { get; set; }

        [BsonElement("BeforeWordWhichMatchingPattern")]
        public string BeforeWordWhichMatchingPattern { get; set; }

        [BsonElement("WhichMatchingPattern")]
        public string WhichMatchingPattern { get; set; }

        [BsonElement("FetchNthLine")]
        public string FetchNthLine { get; set; }

        [BsonElement("FetchNWords")]
        public string FetchNWords { get; set; }

        [BsonElement("X")]
        public string X { get; set; }

        [BsonElement("Y")]
        public string Y { get; set; }

        [BsonElement("Height")]
        public string Height { get; set; }

        [BsonElement("SplittingTypeOrder")]
        public string SplittingTypeOrder { get; set; }

        [BsonElement("Width")]
        public string Width { get; set; }

        [BsonElement("ExpectedDateFormat")]
        public string ExpectedDateFormat { get; set; }

        [BsonElement("OutputDateFormat")]
        public string OutputDateFormat { get; set; }

        [BsonElement("ReportNames")]
        public List<string> ReportNames { get; set; }

        [BsonElement("FetchNLines")]
        public string FetchNLines { get; set; }

    }
}
