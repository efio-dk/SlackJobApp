using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class StaticSelect : Element
    {
        [JsonProperty("placeholder")]
        private Text _placeholder;
        [JsonProperty("options")]
        private List<Option> _options;
        public StaticSelect(string actionId, List<Option> options, string placeholder = null) : base("static_select", actionId)
        {
            if(!(placeholder is null))
                _placeholder = new Text(placeholder);

            _options = options;
        }

        public StaticSelect AddPlaceholder(string placeholder)
        {
            _placeholder = new Text(placeholder);
            return this;
        }
    }
}