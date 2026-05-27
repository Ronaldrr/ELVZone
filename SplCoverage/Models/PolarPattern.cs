using System.Collections.Generic;
using Newtonsoft.Json;

namespace ELVZone.SplCoverage.Models
{
    public class PolarPattern
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("angleUnit")]
        public string AngleUnit { get; set; }

        [JsonProperty("levelUnit")]
        public string LevelUnit { get; set; }

        [JsonProperty("frequencies")]
        public Dictionary<string, List<PolarPoint>> Frequencies { get; set; }
    }
}
