using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class StaticSelect : Element
    {
        [JsonProperty("placeholder")]
        public Text Placeholder { get; set; }
        [JsonProperty("options")]
        public List<Option> Options { get; set; }
        public StaticSelect(string actionId, string placeholder = null)
        {
            if(!(placeholder is null))
                Placeholder = new Text(placeholder);

            Type = "static_select";
            ActionId = actionId;
            Options = new List<Option>();
        }

        public StaticSelect AddOption(Option option)
        {
            Options.Add(option);
            return this;
        }

        public StaticSelect AddPlaceholder(string placeholder)
        {
            Placeholder = new Text(placeholder);
            return this;
        }
    }
}