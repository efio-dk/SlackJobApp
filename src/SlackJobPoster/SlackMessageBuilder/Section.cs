using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class Section : Block
    {
        [JsonProperty("text")]
        public Text Text;
        public Section(Text text)
        {
            Type = "section";
            Text = text;
        }
    }
}