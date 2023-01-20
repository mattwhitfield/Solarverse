using System.Diagnostics;

namespace Solarverse.Core.Data.CacheModels
{
    [DebuggerDisplay("ValidFrom = {ValidFrom}, ValidTo = {ValidTo}, Value = {Value}")]
    public class TariffRateCache
    {
        public double Value { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }
    }
}
