using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using SlackJobPoster.API;
using SlackJobPoster.Database;
using SlackMessageBuilder;
using Xunit;

namespace SlackJobPoster.Tests
{
    public class FunctionTest
    {
        private readonly Dictionary<string, Option> customers = new Dictionary<string, Option>
            {
                { "DSB", new Option("DSB", "DSB") },
                { "Efio", new Option("Efio", "Efio") }
            };

        [Fact]
        public async Task SlackPayloadWithoutCustomerTest()
        {
            var expectedJson = $@"{{
                                    ""blocks"": [
                                        {{
                                        ""text"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": "" ""
                                        }},
                                        ""type"": ""section""
                                        }},
                                        {{
                                        ""text"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": "" ""
                                        }},
                                        ""type"": ""section""
                                        }},
                                        {{
                                        ""text"": {{
                                            ""type"": ""mrkdwn"",
                                            ""text"": ""*header*\nhttp://test.com""
                                        }},
                                        ""type"": ""section"",
                                        ""block_id"": ""msg_header""
                                        }},
                                        {{
                                        ""block_id"": ""actions"",
                                        ""elements"": [
                                            {{
                                            ""placeholder"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""Customer""
                                                }},
                                            ""options"": [
                                                {{
                                                ""text"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""DSB""
                                                }},
                                                ""value"": ""DSB""
                                                }},
                                                {{
                                                ""text"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""Efio""
                                                }},
                                                ""value"": ""Efio""
                                                }}
                                            ],
                                            ""type"": ""static_select"",
                                            ""action_id"": ""customer_select""
                                            }},
                                            {{
                                            ""type"": ""button"",
                                            ""action_id"": ""qualifyLead_btn"",
                                            ""text"": {{
                                                ""type"": ""plain_text"",
                                                ""text"": ""Qualify Lead""
                                                }},
                                            ""style"": ""primary""
                                            }}
                                        ],
                                        ""type"": ""actions""
                                        }},
                                        {{
                                        ""type"": ""divider""
                                        }}
                                    ]
                                    }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            Mock<ICloseClient> closeApi = new Mock<ICloseClient>();
            closeApi.Setup(x => x.GetListOfCustomers()).Returns(Task.FromResult(customers));

            var function = new Function();
            JObject payload = await function.BuildSlackPayload("header", "http://test.com", closeApi.Object);

            Assert.True(JToken.DeepEquals(expectedJObject, payload));
        }

        [Fact]
        public async Task SlackPayloadWithCustomerTest()
        {
            var expectedJson = $@"{{
                                    ""blocks"": [
                                        {{
                                        ""text"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": "" ""
                                        }},
                                        ""type"": ""section""
                                        }},
                                        {{
                                        ""text"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": "" ""
                                        }},
                                        ""type"": ""section""
                                        }},
                                        {{
                                        ""text"": {{
                                            ""type"": ""mrkdwn"",
                                            ""text"": ""*header*\nhttp://test.com""
                                        }},
                                        ""type"": ""section"",
                                        ""block_id"": ""msg_header""
                                        }},
                                        {{
                                        ""block_id"": ""actions"",
                                        ""elements"": [
                                            {{
                                            ""initial_option"": {{
                                                ""text"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""DSB""
                                                }},
                                                ""value"": ""DSB""
                                            }},
                                            ""placeholder"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""Customer""
                                                }},
                                            ""options"": [
                                                {{
                                                ""text"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""DSB""
                                                }},
                                                ""value"": ""DSB""
                                                }},
                                                {{
                                                ""text"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""Efio""
                                                }},
                                                ""value"": ""Efio""
                                                }}
                                            ],
                                            ""type"": ""static_select"",
                                            ""action_id"": ""customer_select""
                                            }},
                                            {{
                                            ""type"": ""button"",
                                            ""action_id"": ""addToClose_btn"",
                                            ""text"": {{
                                                ""type"": ""plain_text"",
                                                ""text"": ""Add to Close""
                                                }},
                                            ""style"": ""primary""
                                            }},
                                            {{
                                            ""type"": ""button"",
                                            ""action_id"": ""qualifyLead_btn"",
                                            ""text"": {{
                                                ""type"": ""plain_text"",
                                                ""text"": ""Qualify Lead""
                                                }}
                                            }}
                                        ],
                                        ""type"": ""actions""
                                        }},
                                        {{
                                        ""type"": ""divider""
                                        }}
                                    ]
                                    }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            Mock<ICloseClient> closeApi = new Mock<ICloseClient>();
            closeApi.Setup(x => x.GetListOfCustomers()).Returns(Task.FromResult(customers));

            var function = new Function();
            JObject payload = await function.BuildSlackPayload("header", "http://test.com", closeApi.Object, "DSB");

            Assert.True(JToken.DeepEquals(expectedJObject, payload));
        }

        [Fact]
        public async Task PostAsJsonAsyncTest()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            Mock<ICloseClient> closeApi = new Mock<ICloseClient>();
            closeApi.Setup(x => x.GetListOfCustomers()).Returns(Task.FromResult(customers));

            var function = new Function();
            JObject payload = await function.BuildSlackPayload("header", "http://test.com", closeApi.Object);

            var expectedResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            var actualResponse = await httpClient.PostAsJsonAsync("", payload);

            Assert.Equal(expectedResponse.StatusCode, actualResponse.StatusCode);
        }

        [Fact]
        public async Task CloseGetListOfCustomersTest()
        {
            var expectedJson = $@"{{
                                    ""blocks"": [
                                        {{
                                        ""text"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": "" ""
                                        }},
                                        ""type"": ""section""
                                        }},
                                        {{
                                        ""text"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": "" ""
                                        }},
                                        ""type"": ""section""
                                        }},
                                        {{
                                        ""text"": {{
                                            ""type"": ""mrkdwn"",
                                            ""text"": ""*header*\nhttp://test.com""
                                        }},
                                        ""type"": ""section"",
                                        ""block_id"": ""msg_header""
                                        }},
                                        {{
                                        ""block_id"": ""actions"",
                                        ""elements"": [
                                            {{
                                            ""placeholder"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""Customer""
                                                }},
                                            ""options"": [
                                                {{
                                                ""text"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""DSB""
                                                }},
                                                ""value"": ""DSB""
                                                }},
                                                {{
                                                ""text"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""Efio""
                                                }},
                                                ""value"": ""Efio""
                                                }}
                                            ],
                                            ""type"": ""static_select"",
                                            ""action_id"": ""customer_select""
                                            }},
                                            {{
                                            ""type"": ""button"",
                                            ""action_id"": ""qualifyLead_btn"",
                                            ""text"": {{
                                                ""type"": ""plain_text"",
                                                ""text"": ""Qualify Lead""
                                                }},
                                            ""style"": ""primary""
                                            }}
                                        ],
                                        ""type"": ""actions""
                                        }},
                                        {{
                                        ""type"": ""divider""
                                        }}
                                    ]
                                    }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            var closeJson = $@"{{
                                    ""data"": [
                                        {{
                                            ""display_name"" : ""DSB"",
                                            ""id"" : ""DSB""
                                        }},
                                        {{
                                            ""display_name"" : ""Efio"",
                                            ""id"" : ""Efio""
                                        }}
                                    ]
                                }}";

            JObject expectedCloseJObject = JObject.Parse(closeJson);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(expectedCloseJObject.ToString())
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/")
            };

            ICloseClient closeApi = new CloseClient(httpClient);

            var function = new Function();
            JObject payload = await function.BuildSlackPayload("header", "http://test.com", closeApi);

            Assert.True(JToken.DeepEquals(expectedJObject, payload));
        }

        [Fact]
        public async Task SkillFilterExistsTest()
        {
            Table skillsTable = null;
            Mock<IDBFacade> db = new Mock<IDBFacade>();
            db.Setup(x => x.ItemExists(It.IsAny<string>(), It.IsAny<Table>())).Returns(Task.FromResult(true));

            List<string> skills = new List<string>
            {
                "java",
                "c#"
            };

            var function = new Function();
            bool result = await function.SkillFilterExists(skills, skillsTable, db.Object);

            Assert.True(result);
        }

        [Fact]
        public async Task SkillFilterDoesnotExistTest()
        {
            Table skillsTable = null;
            Mock<IDBFacade> db = new Mock<IDBFacade>();
            db.Setup(x => x.ItemExists(It.IsAny<string>(), It.IsAny<Table>())).Returns(Task.FromResult(false));

            List<string> skills = new List<string>
            {
                "java",
                "c#"
            };

            var function = new Function();
            bool result = await function.SkillFilterExists(skills, skillsTable, db.Object);

            Assert.False(result);
        }
    }
}