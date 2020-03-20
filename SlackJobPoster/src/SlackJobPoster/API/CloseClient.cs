using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SlackMessageBuilder;

namespace SlackJobPoster.API
{
    public class CloseClient : ICloseClient
    {
        private readonly HttpClient _client;

        public CloseClient(HttpClient client = null)
        {
            _client = client ?? new HttpClient();
        }

        public async Task<Dictionary<string, Option>> GetListOfCustomers()
        {
            Dictionary<string, Option> customers = new Dictionary<string, Option>();
            JObject leads = await GetLeads();

            foreach (JObject lead in leads.SelectToken("data").Value<JArray>())
            {
                customers.Add(lead["display_name"].Value<string>(), new Option(lead["display_name"].Value<string>(), lead["id"].Value<string>()));
            }

            return customers;
        }

        private async Task<JObject> GetLeads()
        {
            HttpResponseMessage response = await _client.GetAsJsonAsync("https://api.close.com/api/v1/lead/", Environment.GetEnvironmentVariable("CLOSE_TOKEN"));

            JObject responseJObj = await response.Content.ReadAsJsonAsync<JObject>();

            return responseJObj;
        }
    }
}