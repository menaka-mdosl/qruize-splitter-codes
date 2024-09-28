using System;

namespace MDO2.Core.Model.Metadata
{
    [System.AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MetadataIndexNameAttribute : Attribute
    {
        public MetadataIndexNameAttribute()
        {
            Type = typeof(string);
        }
        public MetadataIndexNameAttribute(string indexName)
        {
            IndexName = indexName;
            Type = typeof(string);
        }

        public string IndexName { get; set; }
        public string Format { get; set; }
        public Type Type { get; set; }
    }
}
