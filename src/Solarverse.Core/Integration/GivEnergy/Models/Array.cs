﻿using Newtonsoft.Json;

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


}
