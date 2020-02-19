using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class Section : Block
    {
        [JsonProperty("text")]
        private Text _text;
        public Section(Text text) : base("section")
        {
            _text = text;
        }
    }
}