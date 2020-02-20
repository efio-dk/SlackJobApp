using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;

using SlackJobPoster;
using SlackJobPoster.SlackMessageBuilder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static SlackJobPoster.SlackMessageBuilder.Button;

namespace SlackJobPoster.Tests
{
    public class SlackMessageBuilderTest
    {
        [Fact]
        public void EmptyBuilderTest()
        {
            var builder = new SlackMsgBuilder();
            Assert.Throws<JsonException>(() => builder.GetJObject());
        }

        [Fact]
        public void AddBlockTest()
        {
            var builder = new SlackMsgBuilder();
            builder.AddBlock(new Divider());

            Assert.True(builder.GetBlocksCount() == 1);
        }

        [Fact]
        public void DividerTest()
        {
            var expectedJson = $@"{{
                                    ""blocks"": [
                                        {{
                                            ""type"": ""divider""
                                        }}
                                    ]
                                }}";
            JObject expectedJObject = JObject.Parse(expectedJson);

            var builder = new SlackMsgBuilder();
            builder.AddBlock(new Divider());
            JObject result = builder.GetJObject();

            Assert.True(JToken.DeepEquals(expectedJObject, result));
        }

        [Fact]
        public void SectionTest()
        {
            var expectedJson = $@"{{
                                    ""blocks"": [
                                        {{
                                            ""type"": ""section"",
                                            ""text"":
                                                {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""test""
                                                }}
                                        }}
                                    ]
                                }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            var builder = new SlackMsgBuilder();
            builder.AddBlock(new Section(new Text("test")));
            JObject result = builder.GetJObject();

            Assert.True(JToken.DeepEquals(expectedJObject, result));

        }

        [Fact]
        public void ActionTest()
        {
            var expectedJson = $@"{{
                                    ""blocks"": [
                                        {{
                                            ""type"": ""actions"",
                                            ""block_id"": ""actions"",
                                            ""elements"": []
                                        }}
                                    ]
                                }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            var builder = new SlackMsgBuilder();
            builder.AddBlock(new SlackAction("actions"));
            JObject result = builder.GetJObject();

            Assert.True(JToken.DeepEquals(expectedJObject, result));

        }

        [Fact]
        public void ActionAddWithoutPlaceholderTest()
        {
            var expectedJson = $@"{{
                                    ""blocks"": [
                                        {{
                                            ""type"": ""actions"",
                                            ""block_id"": ""actions"",
                                            ""elements"": [
                                                {{
                                                    ""type"": ""static_select"",
                                                    ""action_id"": ""customer_select"",
                                                    ""options"": [
                                                    {{
                                                    ""text"": {{
                                                        ""type"": ""plain_text"",
                                                        ""text"": ""Efio""
                                                    }},
                                                    ""value"": ""Efio""
                                                    }}
                                                    ]
                                                }}
                                            ]
                                        }}
                                    ]
                                }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            var builder = new SlackMsgBuilder();
            List<Option> customers = new List<Option>();
            customers.Add(new Option("Efio", "Efio"));

            builder.AddBlock(new SlackAction("actions").AddElement(new StaticSelect("customer_select", customers)));
            JObject result = builder.GetJObject();

            Assert.True(JToken.DeepEquals(expectedJObject, result));
        }

        [Fact]
        public void ActionAddWithPlaceholderTest()
        {
            var expectedJson = $@"{{
                                    ""blocks"": [
                                        {{
                                            ""type"": ""actions"",
                                            ""block_id"": ""actions"",
                                            ""elements"": [
                                                {{
                                                    ""placeholder"": {{
                                                        ""type"": ""plain_text"",
                                                        ""text"": ""Customer""
                                                    }},
                                                    ""type"": ""static_select"",
                                                    ""action_id"": ""customer_select"",
                                                    ""options"": [
                                                    {{
                                                    ""text"": {{
                                                        ""type"": ""plain_text"",
                                                        ""text"": ""Efio""
                                                    }},
                                                    ""value"": ""Efio""
                                                    }}
                                                    ]
                                                }}
                                            ]
                                        }}
                                    ]
                                }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            var builder = new SlackMsgBuilder();
            List<Option> customers = new List<Option>();
            customers.Add(new Option("Efio", "Efio"));

            builder.AddBlock(new SlackAction("actions").AddElement(new StaticSelect("customer_select", customers, "Customer")));
            JObject result = builder.GetJObject();

            Assert.True(JToken.DeepEquals(expectedJObject, result));
        }

        [Fact]
        public void AddPlaceholderTest()
        {
            var expectedJson = $@"{{
                                    ""blocks"": [
                                        {{
                                            ""type"": ""actions"",
                                            ""block_id"": ""actions"",
                                            ""elements"": [
                                                {{
                                                    ""placeholder"": {{
                                                        ""type"": ""plain_text"",
                                                        ""text"": ""Customer""
                                                    }},
                                                    ""type"": ""static_select"",
                                                    ""action_id"": ""customer_select"",
                                                    ""options"": [
                                                    {{
                                                    ""text"": {{
                                                        ""type"": ""plain_text"",
                                                        ""text"": ""Efio""
                                                    }},
                                                    ""value"": ""Efio""
                                                    }}
                                                    ]
                                                }}
                                            ]
                                        }}
                                    ]
                                }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            var builder = new SlackMsgBuilder();
            List<Option> customers = new List<Option>();
            customers.Add(new Option("Efio", "Efio"));

            builder.AddBlock(new SlackAction("actions").AddElement(new StaticSelect("customer_select", customers).AddPlaceholder("Customer")));
            JObject result = builder.GetJObject();

            Assert.True(JToken.DeepEquals(expectedJObject, result));
        }

        [Fact]
        public void GetButtonStyleTest()
        {
            JObject expectedJObjectPrimary = JObject.Parse(GetExpectedButtonJson("primary"));
            var builderPrimary = new SlackMsgBuilder();
            builderPrimary.AddBlock(new SlackAction("actions").AddElement(new Button("test_btn", "Test", ButtonStyle.PRIMARY)));
            JObject resultPrimary = builderPrimary.GetJObject();

            JObject expectedJObjectDanger = JObject.Parse(GetExpectedButtonJson("danger"));
            var builderDanger = new SlackMsgBuilder();
            builderDanger.AddBlock(new SlackAction("actions").AddElement(new Button("test_btn", "Test", ButtonStyle.DANGER)));
            JObject resultDanger = builderDanger.GetJObject();

            JObject expectedJObjectDefault = JObject.Parse(GetExpectedButtonJson(""));
            var builderDefault = new SlackMsgBuilder();
            builderDefault.AddBlock(new SlackAction("actions").AddElement(new Button("test_btn", "Test")));
            JObject resultDefault = builderDefault.GetJObject();


            Assert.True(JToken.DeepEquals(expectedJObjectPrimary, resultPrimary));
            Assert.True(JToken.DeepEquals(expectedJObjectDanger, resultDanger));
            Assert.True(JToken.DeepEquals(expectedJObjectDefault, resultDefault));
        }

        private string GetExpectedButtonJson(string buttonStyle)
        {
            var styleJson = $@",
                                ""style"": ""{buttonStyle}""";

            if (buttonStyle == "")
                styleJson = "";

            var expectedJson = $@"{{
                                    ""blocks"": [
                                        {{
                                            ""type"": ""actions"",
                                            ""block_id"": ""actions"",
                                            ""elements"": [
                                                {{
                                                    ""type"": ""button"",
                                                    ""action_id"": ""test_btn"",
                                                    ""text"": {{
                                                        ""type"": ""plain_text"",
                                                        ""text"": ""Test""
                                                    }}{styleJson}
                                                }}
                                            ]
                                        }}
                                    ]
                                }}";

            return expectedJson;
        }
    }
}
