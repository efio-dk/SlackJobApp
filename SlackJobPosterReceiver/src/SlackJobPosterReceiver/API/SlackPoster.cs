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

        public async Task<HttpResponseMessage> TriggerModalOpen(string token, string triggerId, string view)
        {
            //url to open a view in Slack
            string url = "https://slack.com/api/views.open?token=" + token + "&trigger_id=" + triggerId + "&view=" + HttpUtility.UrlEncode(view);

            HttpResponseMessage responses = await _client.GetAsync(url);

            return responses;
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

        internal async Task<HttpResponseMessage> EphemeralMessage(string errorText, string channelId, string userId)
        {
            const string url = "https://slack.com/api/chat.postEphemeral";
            BlocksBuilder builder = new BlocksBuilder();
            builder.AddBlock(new Section(new Text(errorText)));

            JObject ephemeralMessage = new JObject();
            ephemeralMessage.Add("channel", channelId);
            ephemeralMessage.Add("text", errorText);
            ephemeralMessage.Add("user", userId);
            ephemeralMessage.Add("blocks", builder.GetJObject());

            return await _client.PostAsJsonAsync(url, ephemeralMessage, GlobalVars.SLACK_TOKEN, "token");
        }
    }
}