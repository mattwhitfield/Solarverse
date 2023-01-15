using Newtonsoft.Json;
using Solarverse.Core.Models;

namespace Solarverse.Core.Integration.Octopus.Models
{
    public class AgileRate
    {
        [JsonProperty("value_inc_vat")]
        public double Value { get; set; }

        [JsonProperty("valid_from")]
        public DateTime ValidFrom { get; set; }

        [JsonProperty("valid_to")]
        public DateTime ValidTo { get; set; }

        internal TariffRate ToTariffRate()
        {
            return new TariffRate(Value, ValidFrom, ValidTo);
        }
    }
}
