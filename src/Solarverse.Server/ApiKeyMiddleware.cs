using Solarverse.Core.Helper;
using IConfigurationProvider = Solarverse.Core.Helper.IConfigurationProvider;

namespace Solarverse.Server
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.Equals("/api/health"))
            {
                if (!context.Request.Headers.TryGetValue(Headers.ApiKey, out var extractedApiKey))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("API Key required");
                    return;
                }

                if (!Guid.TryParse(extractedApiKey, out var providedApiKey))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("API Key invalid");
                    return;
                }

                var appSettings = context.RequestServices.GetRequiredService<IConfigurationProvider>();
                var configuredApiKey = appSettings.Configuration.ApiKey;
                if (configuredApiKey != providedApiKey)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized client");
                    return;
                }
            }

            await _next(context);
        }
    }
}
