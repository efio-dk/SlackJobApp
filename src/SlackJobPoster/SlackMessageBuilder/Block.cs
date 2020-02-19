using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public abstract class Block
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}