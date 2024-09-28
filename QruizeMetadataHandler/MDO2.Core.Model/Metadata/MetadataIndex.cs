using Newtonsoft.Json;

namespace MDO2.Core.Model.Metadata
{
    public class MetadataIndex
    {
        [JsonProperty("indexName")]
        public string IndexName { get; set; }
        [JsonProperty("indexValue")]
        public string IndexValue { get; set; }


        public MetadataIndexName? IndexNameAsEnum()
        {
            return this.ToIndexNameEnum();
        }
        public MetadataIndexNameAttribute GetIndexNameAttribute()
        {
            var indexNameEnum = IndexNameAsEnum();
            if (indexNameEnum.HasValue)
            {
                return indexNameEnum.Value.GetIndexNameAttrib();
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            return $"Key={IndexName},Value={IndexValue}";
        }
    }
}
