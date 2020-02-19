using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public abstract class Block
    {
        [JsonProperty("type")]
        private string _type;
        public Block(string type)
        {
            _type = type;
        }
    }
}