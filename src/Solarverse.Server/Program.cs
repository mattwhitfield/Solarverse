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

            builder.Services.AddLogging(); // TODO - route to in-memory store
            builder.Services.AddTransient<IConfigurationProvider, OptionsConfigurationProvider>();
            builder.Services.AddSolarverse();
            builder.Services.AddHostedService<ControlLoopService>();
            builder.Services.AddHostedService<CurrentDataServiceMonitor>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

//            app.UseAuthentication();
//            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<DataHub>("DataHub");

            app.Run();
        }
    }
}