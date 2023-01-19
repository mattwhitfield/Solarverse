using Newtonsoft.Json;

namespace Solarverse.Core.Helper
{
    internal static class HttpClientHelpers
    {
        // TODO - add logging and cancellation

        public static async Task<T> Get<T>(this HttpClient client, string url) where T : class
        {
            int attempts = 0;
            TimeSpan delayTime = TimeSpan.FromSeconds(0.75);
            while (attempts < 10)
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
                            return obj;
                        }
                    }
                }

                attempts++;
                await Task.Delay(delayTime);
                delayTime *= 1.5;
            }

            throw new InvalidOperationException("Could not get response from GET to " + url + " after 10 attempts");
        }

        public static async Task<T> Post<T>(this HttpClient client, string url, object? body = null) where T : class
        {
            int attempts = 0;
            TimeSpan delayTime = TimeSpan.FromSeconds(0.75);
            while (attempts < 10)
            {
                HttpContent? postContent = null;
                if (body != null)
                {
                    var postContentString = JsonConvert.SerializeObject(body, Formatting.None, new JsonSerializerSettings { });
                    postContent = new StringContent(postContentString, null, "application/json");
                }

                using HttpResponseMessage response = await client.PostAsync(url, postContent);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var obj = JsonConvert.DeserializeObject<T>(content);
                        if (obj != null)
                        {
                            return obj;
                        }
                    }
                }

                attempts++;
                await Task.Delay(delayTime);
                delayTime *= 1.5;
            }

            throw new InvalidOperationException("Could not get response from POST to " + url + " after 10 attempts");
        }
    }
}
