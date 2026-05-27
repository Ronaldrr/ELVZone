using Newtonsoft.Json;

namespace ELVZone.SplCoverage.Models
{
    public class SpeakerDefinition
    {
        [JsonIgnore]
        public string LibraryFile { get; set; }

        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("ratedPowerW")]
        public double RatedPowerW { get; set; }

        [JsonProperty("powerTapsW")]
        public double[] PowerTapsW { get; set; }

        [JsonProperty("sensitivityDb_1W_1m")]
        public double SensitivityDb1W1M { get; set; }

        [JsonProperty("frequencyRangeHz")]
        public int[] FrequencyRangeHz { get; set; }

        [JsonProperty("polarPattern")]
        public PolarPattern PolarPattern { get; set; }

        [JsonIgnore]
        public string DisplayName => $"{Manufacturer} {Model}".Trim();
    }
}
