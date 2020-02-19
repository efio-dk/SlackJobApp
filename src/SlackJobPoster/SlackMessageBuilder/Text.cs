using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class Text
    {
        [JsonProperty("type")]
        private string _type; 

        [JsonProperty("text")]
        private string _txt;

        public Text(string text, string type = "plain_text")
        {
            _txt = text;
            _type = type;
        }
    }
}