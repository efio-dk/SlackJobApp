using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class SlackAction : Block
    {
        [JsonProperty("block_id")]
        public string BlockId;
        [JsonProperty("elements")]
        public List<Element> Elements;
        public SlackAction(string blockId)
        {
            Type = "actions";
            Elements = new List<Element>();
            BlockId = blockId;
        }

        public SlackAction AddElement(Element element)
        {
            Elements.Add(element);
            return this;
        }
    }
}