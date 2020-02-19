using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class SlackMsgBuilder
    {
        [JsonProperty("blocks")]
        public List<Block> Blocks;

        public SlackMsgBuilder()
        {
            Blocks = new List<Block>();
        }

        public void AddBlock(Block block)
        {
            Blocks.Add(block);
        }

        public JObject GetJObject()
        {
            JObject jsonObj = JObject.FromObject(this);

            return jsonObj;
        }
    }
}