using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public abstract class Element
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("action_id")]
        public string ActionId { get; set; }
    }
}