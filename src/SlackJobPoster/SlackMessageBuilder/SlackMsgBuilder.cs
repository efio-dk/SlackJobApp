using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class SlackMsgBuilder
    {
        [JsonProperty("blocks")]
        private List<Block> _blocks;

        public SlackMsgBuilder()
        {
            _blocks = new List<Block>();
        }

        public void AddBlock(Block block)
        {
            _blocks.Add(block);
        }
        public int GetBlocksCount()
        {
            return _blocks.Count;
        }
        public JObject GetJObject()
        {
            if (_blocks.Count <= 0)
                throw new JsonException("Empty Slack message");
            JObject jsonObj = JObject.FromObject(this);
            return jsonObj;
        }
    }
}