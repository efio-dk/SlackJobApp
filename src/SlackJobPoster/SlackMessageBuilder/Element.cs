using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public abstract class Element
    {
        [JsonProperty("type")]
        private string _type;
        [JsonProperty("action_id")]
        private string _actionId;

        public Element(string type, string actionId)
        {
            _type = type;
            _actionId = actionId;
        }
    }
}