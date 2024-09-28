using Newtonsoft.Json;

namespace MDO2.Core.SM.Model.Data
{
    public class Solution
    {
        [JsonProperty("solution_name")]
        public string SolutionName { get; set; }

        [JsonProperty("solution_slug")]
        public string SolutionSlug { get; set; }
    }
}
