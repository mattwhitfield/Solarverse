using System;

namespace Solarverse.Client
{
    public class ClientConfiguration
    {
        public string? Url { get; set; }

        public Guid ApiKey { get; set; }

        public bool DevMode { get; set; }
    }
}
