﻿using Newtonsoft.Json;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class SettingMutation
    {
        [JsonProperty("data")]
        public SettingMutationValues? Data { get; set; }
    }
}
