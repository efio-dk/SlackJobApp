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

namespace SlackJobPoster.Tests
{
    public class FunctionTest
    {
        /*
        [Fact]
        public async Task TestSQSEventLambdaFunction()
        {
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = "foobar"
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.FunctionHandler(sqsEvent, context);

            Assert.Contains("Processed message foobar", logger.Buffer.ToString());
        }*/

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
        public void ActionAddTest()
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

            builder.AddBlock(new SlackAction("actions").AddElement(new StaticSelect("customer_select",customers, "Customer")));
            JObject result = builder.GetJObject();

            Console.WriteLine(expectedJObject.ToString());
            Console.WriteLine("n\n\n");
            Console.WriteLine(result.ToString());

            Assert.True(JToken.DeepEquals(expectedJObject, result));

        }

        /* [Fact]
        public void DividerTest()
        {
            var expectedJObject = new JObject();
            var test = new JObject();
            test.Add("type", "divider");
            List<JObject> blocks = new List<JObject>();
            blocks.Add(test);
            expectedJObject.Add("blocks", JToken.FromObject(blocks));

            var builder = new SlackMsgBuilder();
            builder.AddBlock(new Divider());
            JObject result = builder.GetJObject();

            Assert.Equal(expectedJObject, result);
        } */
    }
}
