using System;
using System.Collections.Generic;
using System.Linq;

namespace MDO2.Core.Model.Metadata
{
    public class MetadataIndexCollection : List<MetadataIndex>
    {
        public string this[string indexName]
        {
            get
            {
                return this.FirstOrDefault(x => x.IndexName == indexName)?.IndexValue;
            }
            set
            {
                var entry = Find(x => x.IndexName == indexName);
                var index = IndexOf(entry);
                if (index > -1) this[index].IndexValue = value;
            }
        }
        public string this[MetadataIndexName indexName]
        {
            get
            {
                return this[indexName.GetIndexNameByEnum()];
            }
            set
            {
                this[indexName.GetIndexNameByEnum()] = value;
            }
        }

        public virtual MetadataIndexCollection AddOrUpdate(MetadataIndex index)
        {
            var exiting = this.FirstOrDefault(x => x.IndexName == index.IndexName);
            if (exiting != null)
            {
                this[exiting.IndexName] = index.IndexValue;
            }
            else
            {
                Add(index);
            }
            return this;
        }
        public virtual MetadataIndexCollection AddOrUpdate(MetadataIndexName indexName, string value)
        {
            var exiting = this.FirstOrDefault(x => x.IndexNameAsEnum() == indexName);
            if (exiting != null)
            {
                this[exiting.IndexName] = value;
            }
            else
            {
                var indexNameText = indexName.GetIndexNameByEnum();
                Add(new MetadataIndex() { IndexName = indexNameText, IndexValue = value });
            }
            return this;
        }
        public virtual MetadataIndexCollection AddOrUpdate(string indexName, string value)
        {
            var exiting = this.FirstOrDefault(x => x.IndexName == indexName);
            if (exiting != null)
            {
                this[exiting.IndexName] = value;
            }
            else
            {
                Add(new MetadataIndex() { IndexName = indexName, IndexValue = value });
            }
            return this;
        }
        public virtual MetadataIndexCollection AddOrUpdate(string indexName, object value)
        {
            var exiting = this.FirstOrDefault(x => x.IndexName == indexName);
            if (exiting != null)
            {
                this[exiting.IndexName] = value?.ToString();
            }
            else
            {
                Add(new MetadataIndex() { IndexName = indexName, IndexValue = value?.ToString() });
            }
            return this;
        }
        public virtual MetadataIndexCollection AddOrUpdate(MetadataIndexName indexName, DateTime dateTime)
        {
            var attrib = indexName.GetIndexNameAttrib();
            if (!string.IsNullOrWhiteSpace(attrib.Format))
                AddOrUpdate(indexName, dateTime.ToString(attrib.Format));
            else
                AddOrUpdate(indexName, dateTime.ToString());
            return this;
        }
        public virtual MetadataIndexCollection AddOrUpdate(string indexName, DateTime dateTime, string dateTimeFormat = null)
        {
            if (!string.IsNullOrWhiteSpace(dateTimeFormat))
                AddOrUpdate(indexName, dateTime.ToString(dateTimeFormat));
            else
                AddOrUpdate(indexName, dateTime.ToString());
            return this;
        }
        public virtual MetadataIndexCollection AddOrUpdate(MetadataIndexName indexName, int value)
        {
            return AddOrUpdate(indexName, value.ToString());
        }
        public virtual MetadataIndexCollection AddOrUpdate(string indexName, int value)
        {
            return AddOrUpdate(indexName, value.ToString());
        }
        public virtual MetadataIndexCollection AddOrUpdate(MetadataIndexName indexName, long value)
        {
            return AddOrUpdate(indexName, value.ToString());
        }
        public virtual MetadataIndexCollection AddOrUpdate(string indexName, long value)
        {
            return AddOrUpdate(indexName, value.ToString());
        }
        public virtual MetadataIndexCollection AddOrUpdate(MetadataIndexName indexName, decimal value)
        {
            return AddOrUpdate(indexName, value.ToString());
        }
        public virtual MetadataIndexCollection AddOrUpdate(string indexName, decimal value)
        {
            return AddOrUpdate(indexName, value.ToString());
        }

        public virtual MetadataIndexCollection Remove(MetadataIndexName indexName)
        {
            var exiting = this.FirstOrDefault(x => x.IndexNameAsEnum() == indexName);
            if (exiting != null)
                Remove(exiting);
            return this;
        }
        public virtual MetadataIndexCollection Remove(string indexName)
        {
            var exiting = this.FirstOrDefault(x => x.IndexName == indexName);
            if (exiting != null)
                Remove(exiting);
            return this;
        }
    }
}
