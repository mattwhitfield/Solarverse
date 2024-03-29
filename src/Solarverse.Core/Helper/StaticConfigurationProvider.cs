﻿using Newtonsoft.Json;
using Solarverse.Core.Models.Settings;

namespace Solarverse.Core.Helper
{
    public static class StaticConfigurationProvider
    {
        private static Lazy<Configuration> ConfigStore = new Lazy<Configuration>(() =>
        {
            var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Solarverse", "Configuration.json");
            if (!File.Exists(fileName))
            {
                return new Configuration();
            }

            var fileContent = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<Configuration>(fileContent) ?? new Configuration();
        });

        public static Configuration Configuration => ConfigStore.Value;
    }
}
