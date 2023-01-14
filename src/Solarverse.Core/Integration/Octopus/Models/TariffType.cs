using Newtonsoft.Json;

namespace Solarverse.Core.Integration.Octopus.Models
{
    public class TariffType
    {
        [JsonProperty("direct_debit_monthly")]
        public Tariff? DirectDebitMonthly { get; set; }
    }
}
