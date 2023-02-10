using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Solarverse.Core.Helper
{
    public static class HttpClientHelpers
    {
        public static async Task<T> Get<T>(this HttpClient client, ILogger logger, string url) where T : class
        {
            int attempts = 0;
            TimeSpan delayTime = TimeSpan.FromSeconds(0.75);

            logger.LogInformation($"Sending HTTP Get to {url}...");

            while (attempts < 10)
            {
                try
                {
                    using HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            var obj = JsonConvert.DeserializeObject<T>(content);
                            if (obj != null)
                            {
                                logger.LogInformation($"HTTP Get to {url} succeeded, returning {typeof(T).GetFormattedName()}");
                                return obj;
                            }
                        }
                    }
                    else
                    {
                        logger.LogWarning($"HTTP Get to {url} failed - status code {response.StatusCode} - (attempt {attempts}) - waiting {delayTime}");
                    }
                }
                catch (HttpRequestException exc)
                {
                    logger.LogWarning(exc, $"HTTP Get to {url} failed with exception");
                }

                attempts++;
                if (attempts < 10)
                {
                    await Task.Delay(delayTime);
                    delayTime *= 1.5;
                }
            }

            throw new InvalidOperationException("Could not get response from GET to " + url + " after 10 attempts");
        }

        public static async Task<T> Post<T>(this HttpClient client, ILogger logger, string url, object? body = null) where T : class
        {
            int attempts = 0;
            TimeSpan delayTime = TimeSpan.FromSeconds(0.75);

            var hasBody = body != null ? "with" : "without";
            logger.LogInformation($"Sending HTTP Post to {url} {hasBody} body...");

            while (attempts < 10)
            {
                HttpContent? postContent = null;
                if (body != null)
                {
                    var postContentString = JsonConvert.SerializeObject(body, Formatting.None, new JsonSerializerSettings { });
                    postContent = new StringContent(postContentString, null, "application/json");
                }

                try
                {
                    using HttpResponseMessage response = await client.PostAsync(url, postContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            var obj = JsonConvert.DeserializeObject<T>(content);
                            if (obj != null)
                            {
                                logger.LogInformation($"HTTP Post to {url} succeeded, returning {typeof(T).GetFormattedName()}");

                                return obj;
                            }
                        }
                    }
                    else
                    {
                        logger.LogWarning($"HTTP Post to {url} failed - status code {response.StatusCode} - (attempt {attempts}) - waiting {delayTime}");
                    }
                }
                catch (HttpRequestException exc)
                {
                    logger.LogWarning(exc, $"HTTP Get to {url} failed with exception");
                }

                attempts++;
                if (attempts < 10)
                {
                    await Task.Delay(delayTime);
                    delayTime *= 1.5;
                }
            }

            throw new InvalidOperationException("Could not get response from POST to " + url + " after 10 attempts");
        }
    }
}
