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

        public async Task<JObject> PostOpportunity(string msgHeader, string leadId)
        {
            JObject opportunityObj = new JObject
            {
                ["note"] = msgHeader,
                ["lead_id"] = leadId,
                ["status_id"] = "stat_1FXxBFpJT4gWhs8zdMezZuNNT0VPbkjiIL0tLupmK4Q",
                ["confidence"] = 0
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync("https://api.close.com/api/v1/opportunity/", opportunityObj, GlobalVars.CLOSE_TOKEN);

            return await response.Content.ReadAsJsonAsync<JObject>();
        }
    }
}