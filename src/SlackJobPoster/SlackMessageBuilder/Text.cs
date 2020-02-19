using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class Text
    {
        [JsonProperty("type")]
        public string Type; 

        [JsonProperty("text")]
        public string Txt;

        public Text(string text, string type = "plain_text")
        {
            Txt = text;
            Type = type;
        }
    }
}