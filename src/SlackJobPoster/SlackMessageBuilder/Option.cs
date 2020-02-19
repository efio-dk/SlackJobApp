using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class Option
    {
        [JsonProperty("text")]
        private Text _text;
        [JsonProperty("value")]
        private string _value;

        public Option(string text, string value)
        {
            _text = new Text(text);
            _value = value;
        }
    }
}