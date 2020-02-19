using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class SlackAction : Block
    {
        [JsonProperty("block_id")]
        private string _blockId;
        [JsonProperty("elements")]
        private List<Element> _elements;
        public SlackAction(string blockId) : base("actions")
        {
            _elements = new List<Element>();
            _blockId = blockId;
        }

        public SlackAction AddElement(Element element)
        {
            _elements.Add(element);
            return this;
        }
    }
}