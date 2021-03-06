using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SlackJobPosterReceiver.API
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(
            this HttpClient httpClient, string url, T data, string apiKey = null, string authType = "basic")
        {
            var dataAsString = JsonConvert.SerializeObject(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            if (!(apiKey is null))
            {
                if (authType == "basic")
                {
                    //add Basic Auth
                    var authToken = Encoding.ASCII.GetBytes($"{apiKey}:");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
                }
                else if (authType == "token")
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }
            }

            return httpClient.PostAsync(url, content);
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var dataAsString = await content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(dataAsString);
        }

        public static Task<HttpResponseMessage> GetAsJsonAsync(
            this HttpClient httpClient, string url, string apiKey = null)
        {
            if (!(apiKey is null))
            {
                //add Basic Auth
                var authToken = Encoding.ASCII.GetBytes($"{apiKey}:");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
            }

            return httpClient.GetAsync(url);
        }
    }
}