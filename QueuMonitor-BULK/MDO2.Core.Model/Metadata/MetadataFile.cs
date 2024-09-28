using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MDO2.Core.Model.Metadata
{
    public class MetadataFile : ICloneable
    {
        #region static functions
        public static MetadataFile Parse(string metadataString)
        {
            return (MetadataFile)JsonConvert.DeserializeObject(metadataString, typeof(MetadataFile));
        }
        public static MetadataFile Read(string path)
        {
            if (File.Exists(path))
            {
                using (var stream = new StreamReader(path))
                {
                    return (MetadataFile)JsonConvert.DeserializeObject(stream.ReadToEnd(), typeof(MetadataFile));
                }
            }
            else
            {
                return null;
            }
        }
        public static MetadataFile Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return (MetadataFile)JsonConvert.DeserializeObject(sr.ReadToEnd(), typeof(MetadataFile));
            }
        }
        public static MetadataFile Create()
        {
            MetadataFile metadataFile = new MetadataFile();
            var indexNameEnumValues = (int[])Enum.GetValues(typeof(MetadataIndexName));
            foreach (var idxVal in indexNameEnumValues)
            {
                metadataFile.Indexes.AddOrUpdate((MetadataIndexName)idxVal, string.Empty);
            }
            return metadataFile;
        }
        public static string[] ListOfKnownIndexNames()
        {
            var enumVal = (MetadataIndexName[])Enum.GetValues(typeof(MetadataIndexName));
            return enumVal.Select(x => x.GetIndexNameByEnum()).ToArray();
        }
        public static string[] ListOfKnownSystemIndexNames()
        {
            var indexNameList = new List<string>();
            indexNameList.Add(MetadataIndexName.FileExtension.GetIndexNameByEnum());
            indexNameList.Add(MetadataIndexName.FileName.GetIndexNameByEnum());
            indexNameList.Add(MetadataIndexName.FileSizeInBytes.GetIndexNameByEnum());
            indexNameList.Add(MetadataIndexName.FileUploadedDate.GetIndexNameByEnum());
            indexNameList.Add(MetadataIndexName.ConnectorID.GetIndexNameByEnum());
            indexNameList.Add(MetadataIndexName.SourceFileName.GetIndexNameByEnum());
            return indexNameList.ToArray();
        }
        #endregion

        public MetadataFile()
        {
            Page = null;
            IsAttachment = false;
            SmartscanRequired = true;
            Solutions = "";
            Indexes = new MetadataIndexCollection();
        }

        [JsonProperty("docId")]
        //[RulePropertyName("Doc ID", "Document ID", "Doc ID", "DocID")]
        public string DocID { get; set; }

        [JsonProperty("parentDocId")]
        //[RulePropertyName("Parent Document ID", "Parent DocumentID", "Parent Doc ID", "ParentDocID")]
        public string ParentDocID { get; set; }


        [JsonProperty("chainId")]
        //[RulePropertyName("Chain ID", "ChainID")]
        public string ChainID { get; set; }


        [JsonProperty("entityId")]
        //[RulePropertyName("Entity ID", "EntityID")]
        public string EntityID { get; set; }


        [JsonProperty("projectId")]
        public string ProjectID { get; set; }
        [JsonProperty("s3Key")]
        public string S3Key { get; set; }
        [JsonProperty("s3Url")]
        public string S3Url { get; set; }
        [JsonProperty("isHistorical")]
        public bool IsHistorical { get; set; }
        [JsonProperty("docVersion")]
        public decimal DocVersion { get; set; }
        [JsonProperty("solutions")]
        public string Solutions { get; set; }

        [JsonProperty("siloDocId")]
        public string SiloDocID { get; set; }

        //---- Only for IS annotations
        [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
        public int? Page { get; set; }

        [JsonProperty("isAttachment", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAttachment { get; set; }
        //----

        [JsonProperty("smartscanRequired", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SmartscanRequired { get; set; }

        [JsonProperty("indexes")]
        public MetadataIndexCollection Indexes { get; set; }

        public object Clone()
        {
            var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject(jsonStr, GetType());
        }
        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}
