using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Array
    {
        [JsonProperty("array")]
        public int ArrayNumber { get; set; }

        [JsonProperty("voltage")]
        public double Voltage { get; set; }

        [JsonProperty("current")]
        public double Current { get; set; }

        [JsonProperty("power")]
        public int Power { get; set; }
    }

    public class Battery
    {
        [JsonProperty("percent")]
        public int Percent { get; set; }

        [JsonProperty("power")]
        public int Power { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }
    }

    public class Data
    {
        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("solar")]
        public Solar? Solar { get; set; }

        [JsonProperty("battery")]
        public Battery? Battery { get; set; }
    }

    public class CurrentState
    {
        [JsonProperty("data")]
        public Data? Data { get; set; }
    }

    public class Solar
    {
        [JsonProperty("power")]
        public int Power { get; set; }

        [JsonProperty("arrays")]
        public List<Array>? Arrays { get; set; }
    }


}
