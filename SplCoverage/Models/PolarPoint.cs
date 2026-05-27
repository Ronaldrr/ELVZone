using Newtonsoft.Json;

namespace ELVZone.SplCoverage.Models
{
    public class PolarPoint
    {
        [JsonProperty("angle")]
        public double Angle { get; set; }

        [JsonProperty("db")]
        public double Db { get; set; }
    }
}
