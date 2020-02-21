using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class StaticSelect : Element
    {
        [JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
        private Text _placeholder;
        [JsonProperty("intial_option", NullValueHandling = NullValueHandling.Ignore)]
        private Option _initialOption;
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

        public StaticSelect AddInitialOption(Option initialOption)
        {
            _initialOption = initialOption;
            return this;
        }
    }
}