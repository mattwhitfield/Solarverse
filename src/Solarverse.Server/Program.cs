using Microsoft.Extensions.DependencyInjection.Extensions;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Models.Settings;
using IConfigurationProvider = Solarverse.Core.Helper.IConfigurationProvider;

namespace Solarverse.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddSignalR();

            builder.Services.Configure<Configuration>(
                builder.Configuration.GetSection("Solarverse"));

            builder.Services.AddLogging(builder =>
            {
                builder.Services.TryAddEnumerable(
                    ServiceDescriptor.Singleton<ILoggerProvider, MemoryLoggerProvider>());
            });
            builder.Services.AddTransient<IConfigurationProvider, OptionsConfigurationProvider>();
            builder.Services.AddTransient<ICachePathProvider, ContainerCachePathProvider>();
            builder.Services.AddSolarverse();
            builder.Services.AddHostedService<ControlLoopService>();
            builder.Services.AddHostedService<CurrentDataServiceMonitor>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<ApiKeyMiddleware>();

            app.MapControllers();
            app.MapHub<DataHub>("DataHub");

            app.Run();
        }
    }
}