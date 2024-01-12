using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Domain.Models.Purview
{
    public class SynapseProcessAttributes
    {
        [JsonProperty("name")]
        public string Name = "";
        [JsonProperty("qualifiedName")]
        public string QualifiedName = "";
        [JsonProperty("sparkPoolName")]
        public string SparkPoolName = "";
        [JsonProperty("user")]
        public string User = "";
        [JsonProperty("sparkVersion")]
        public string SparkVersion = "";
        [JsonProperty("columnMapping")]
        public string ColumnMapping = "";
        [JsonProperty("sparkPlan")]
        public string SparkPlan = "";
        [JsonProperty("inputs")]
        public List<InputOutput>? Inputs = new List<InputOutput>();
        [JsonProperty("outputs")]
        public List<InputOutput>? Outputs = new List<InputOutput>();
    }
}