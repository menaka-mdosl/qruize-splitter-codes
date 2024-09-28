using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDO2.Core.SM.Model.Data
{
    public class SolutionFileMappingInformation
    {
        [JsonProperty("bulkFileMapping")]
        public List<SolutionBulkFile> BulkFileMapping { get; set; }
    }
}
