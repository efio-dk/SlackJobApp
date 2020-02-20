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
        private Text _text;
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        private string _style;
        public Button(string actionId, string text, ButtonStyle style = ButtonStyle.DEFAULT) : base("button", actionId)
        {
            _text = new Text(text);
            _style = GetStyleFromEnum(style);
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