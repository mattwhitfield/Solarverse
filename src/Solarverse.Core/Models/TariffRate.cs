using System.Diagnostics;

namespace Solarverse.Core.Models
{
    [DebuggerDisplay("ValidFrom = {ValidFrom}, ValidTo = {ValidTo}, Value = {Value}")]
    public class TariffRate
    {
        public TariffRate(double value, DateTime validFrom, DateTime validTo)
        {
            Value = value;
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        public double Value { get; }

        public DateTime ValidFrom { get; }

        public DateTime ValidTo { get; }
    }
}
