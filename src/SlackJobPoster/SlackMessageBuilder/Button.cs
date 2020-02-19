using System;
using Newtonsoft.Json;

namespace SlackJobPoster.SlackMessageBuilder
{
    public class Button : Element
    {
        public enum ButtonStyle
        {
            PRIMARY,
            DANGER,
            DEFAULT
        }

        [JsonProperty("text")]
        public Text Text { get; set; }
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public string Style { get; set; }
        public Button(string actionId, string text, ButtonStyle style = ButtonStyle.DEFAULT)
        {
            Type = "button";
            ActionId = actionId;
            Text = new Text(text);
            Style = GetStyleFromEnum(style);
        }

        private string GetStyleFromEnum(ButtonStyle style)
        {
            switch (style)
            {
                case ButtonStyle.PRIMARY:
                    return "primary";
                case ButtonStyle.DANGER:
                    return "danger";
                default:
                    return null;
            }
        }
    }
}