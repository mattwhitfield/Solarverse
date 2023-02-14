namespace Solarverse.Core.Data
{
    public class WindowsCachePathProvider : ICachePathProvider
    {
        public string CachePath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }
}