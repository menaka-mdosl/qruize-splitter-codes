using System;
using System.Linq;

namespace MDO2.Core.Model.Metadata
{
    public static class MetadataExtentions
    {
        public static MetadataIndexNameAttribute GetIndexNameAttrib(this MetadataIndexName idx)
        {
            var members = idx.GetType().GetMember(idx.ToString());
            if (members != null && members.Length > 0)
            {
                var attrib = members.First().GetCustomAttributes(typeof(MetadataIndexNameAttribute), true)?.FirstOrDefault();
                if (attrib != null)
                {
                    return (MetadataIndexNameAttribute)attrib;
                }
            }

            return null;
        }

        public static MetadataIndex GetIndex(this MetadataFile file, string indexName)
        {
            if (file == null || file.Indexes == null || file.Indexes.Count == 0) return null;
            if (string.IsNullOrWhiteSpace(indexName)) return null;
            return file.Indexes.FirstOrDefault(x => x.IndexName == indexName);
        }
        public static MetadataIndex GetIndex(this MetadataFile file, MetadataIndexName indexName)
        {
            return file.Indexes.FirstOrDefault(x => x.IndexNameAsEnum() == indexName);
        }
        public static DateTime GetBusinessDate(this MetadataFile file)
        {
            if (file == null || file.Indexes == null)
                return DateTime.Now.AddDays(-1).Date;
            else
            {
                var bdIndex = file.Indexes.FirstOrDefault(x => x.ToIndexNameEnum() == MetadataIndexName.BusinessDate);
                var bdIndexAttribute = MetadataIndexName.BusinessDate.GetIndexNameAttrib();
                DateTime parsed;
                if (!DateTime.TryParseExact(bdIndex.IndexValue, bdIndexAttribute.Format, null, System.Globalization.DateTimeStyles.None, out parsed))
                    return DateTime.Now.AddDays(-1).Date;
                else
                    return parsed;
            }
        }

        public static string GetIndexNameByEnum(this MetadataIndexName idx)
        {

            var attrib = GetIndexNameAttrib(idx);
            if (attrib == null)
                return string.Empty;
            else
                return attrib.IndexName;
        }
        public static MetadataIndexName? ToIndexNameEnum(this string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            else
            {
                var members = typeof(MetadataIndexName)
                    .GetMembers();
                var member = members
                    .FirstOrDefault((m) =>
                    {
                        var attribs = m.GetCustomAttributes(typeof(MetadataIndexNameAttribute), true) ?? new object[0];
                        if (attribs.Length > 0)
                        {
                            var attrib = (MetadataIndexNameAttribute)attribs[0];
                            if (attrib.IndexName?.ToLower() == name?.ToLower())
                                return true;
                            else
                                return false;
                        }
                        else
                        {
                            return false;
                        }
                    });

                if (member != null)
                {
                    return (MetadataIndexName)Enum.Parse(typeof(MetadataIndexName), member.Name);
                }
                else
                {
                    return null;
                }
            }
        }
        public static MetadataIndexName? ToIndexNameEnum(this MetadataIndex index)
        {
            if (index == null) return null;
            else return ToIndexNameEnum(index.IndexName);
        }

        public static string GetIndexValue(this MetadataIndexName indexName, int value)
        {
            return value.ToString();
        }
        public static string GetIndexValue(this MetadataIndexName indexName, long value)
        {
            return value.ToString();
        }
        public static string GetIndexValue(this MetadataIndexName indexName, decimal value)
        {
            return value.ToString();
        }
        public static string GetIndexValue(this MetadataIndexName indexName, DateTime value)
        {
            var attrib = indexName.GetIndexNameAttrib();
            if (!string.IsNullOrWhiteSpace(attrib.Format))
                return value.ToString(attrib.Format);
            else
                return value.ToString();
        }
    }
}
