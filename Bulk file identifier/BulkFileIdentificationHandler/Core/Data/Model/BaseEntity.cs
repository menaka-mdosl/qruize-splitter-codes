using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkFileIdentificationHandler.Core.Data.Model
{
    [BsonIgnoreExtraElements]
    public abstract class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("create_at")]
        public DateTime? CreatedAt { get; set; }

        [BsonElement("create_by")]
        public string CreateBy { get; set; }

        [BsonElement("update_at")]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("update_by")]
        public string UpdatedBy { get; set; }
    }
}
