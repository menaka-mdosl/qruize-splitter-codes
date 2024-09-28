using Newtonsoft.Json;

namespace MDO2.Core.SM.Model.Data
{
    public class SolutionBulkFile
    {
        [JsonProperty("bulkFileId")]
        public string BulkFileId { get; set; }

        [JsonProperty("bulkSourceFileName")]
        public string BulkSourceFileName { get; set; }
    }
}
