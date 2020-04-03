using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using SlackMessageBuilder;

namespace SlackJobPosterReceiver.API
{
    public class SlackPoster
    {
        private readonly HttpClient _client;

        public SlackPoster(HttpClient client = null)
        {
            _client = client ?? new HttpClient();
        }

        public async Task<JObject> TriggerModalOpen(string token, string triggerId, string view, bool responseAction = false)
        {
            JObject newView;
            if (responseAction)
            {
                newView = new JObject
                {
                    { "response_action", "push" },
                    { "view", JObject.Parse(view) }
                };
            }
            else
            {
                newView = JObject.Parse(view);
            }

            //url to open a view in Slack
            string url = "https://slack.com/api/views.open?token=" + token + "&trigger_id=" + triggerId + "&view=" + HttpUtility.UrlEncode(newView.ToString());

            await _client.GetAsync(url);

            return newView;
        }

        public async Task<HttpResponseMessage> UpdateMessage(JObject updatedMsg, string hookUrl)
        {
            return await _client.PostAsJsonAsync(hookUrl, updatedMsg);
        }

        public async Task<HttpResponseMessage> UpdateHomePage(JObject updatedMsg)
        {
            const string url = "https://slack.com/api/views.publish";

            return await _client.PostAsJsonAsync(url, updatedMsg, GlobalVars.SLACK_TOKEN, "token");
        }

        public async Task<HttpResponseMessage> TestAPI()
        {
            const string url = "https://slack.com/api/api.test";

            return await _client.GetAsync(url);
        }
    }
}