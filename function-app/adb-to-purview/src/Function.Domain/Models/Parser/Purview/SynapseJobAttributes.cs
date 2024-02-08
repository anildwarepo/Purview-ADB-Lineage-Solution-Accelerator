using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
    public class SynapseJobAttributes
    {
        [JsonProperty("name")]
        public string Name = "";
        [JsonProperty("qualifiedName")]
        public string QualifiedName = "";
        [JsonProperty("jobId")]
        public long JobId = 0;
        [JsonProperty("submitter")]
        public string Submitter = "";
    }
}