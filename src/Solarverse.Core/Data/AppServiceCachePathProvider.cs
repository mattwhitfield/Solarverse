namespace Solarverse.Core.Data
{
    public class AppServiceCachePathProvider : ICachePathProvider
    {
        public string CachePath
        {
            get
            {
                var folder = Environment.GetEnvironmentVariable("HOME");

                if (string.IsNullOrEmpty(folder))
                {
                    folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Solarverse.AppService");
                }

                return folder;
            }
        }
    }
}