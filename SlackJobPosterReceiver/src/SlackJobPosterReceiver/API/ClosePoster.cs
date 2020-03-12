using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SlackJobPosterReceiver.API
{
    public class ClosePoster
    {
        private readonly HttpClient _client;

        public ClosePoster(HttpClient client = null)
        {
            _client = client ?? new HttpClient();
        }

        public async Task<JObject> PostLead(string leadName)
        {
            JObject leadObj = new JObject
            {
                ["name"] = leadName
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync("https://api.close.com/api/v1/lead/", leadObj, GlobalVars.CLOSE_TOKEN);

            return await response.Content.ReadAsJsonAsync<JObject>();
        }

        public async Task<JObject> PostOpportunity(string msgHeader, string leadId, string opportunityStatusName)
        {
            string statusId = await GetStatusId(opportunityStatusName);

            JObject opportunityObj = new JObject
            {
                ["note"] = msgHeader,
                ["lead_id"] = leadId,
                ["confidence"] = 0
            };

            if (!string.IsNullOrEmpty(statusId))
                opportunityObj.Add("status_id", statusId);

            HttpResponseMessage response = await _client.PostAsJsonAsync("https://api.close.com/api/v1/opportunity/", opportunityObj, GlobalVars.CLOSE_TOKEN);

            return await response.Content.ReadAsJsonAsync<JObject>();
        }

        public async Task<string> GetStatusId(string statusName)
        {
            HttpResponseMessage response = await _client.GetAsJsonAsync("https://api.close.com/api/v1/status/opportunity/", GlobalVars.CLOSE_TOKEN);

            JObject responseJObj = await response.Content.ReadAsJsonAsync<JObject>();

            string statusId = (string)responseJObj.SelectToken($"$..data[?(@.label=='{statusName}')].id");

            return statusId;
        }

        public async Task<JObject> GetLeads()
        {
            HttpResponseMessage response = await _client.GetAsJsonAsync("https://api.close.com/api/v1/lead/", GlobalVars.CLOSE_TOKEN);

            JObject responseJObj = await response.Content.ReadAsJsonAsync<JObject>();

            return responseJObj;
        }
    }
}