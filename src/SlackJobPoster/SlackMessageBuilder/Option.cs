using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class Option
    {
        [JsonProperty("text")]
        public Text Text { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }

        public Option(string text, string value)
        {
            Text = new Text(text);
            Value = value;
        }
    }
}