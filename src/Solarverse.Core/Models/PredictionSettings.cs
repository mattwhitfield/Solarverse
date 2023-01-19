namespace Solarverse.Core.Models
{
    public class PredictionSettings
    {
        public string? MethodName { get; set; }

        public int NumberOfDays { get; set; } = 4;
    }
}