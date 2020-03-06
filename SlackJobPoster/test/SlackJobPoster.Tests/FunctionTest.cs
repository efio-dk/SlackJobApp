using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Xunit;

namespace SlackJobPoster.Tests
{
    public class FunctionTest
    {
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
                                                ""value"": ""lead_q9WAvUeMbAj9zBsINtZgzxBTXfMwxixGyYmR9rk0ovP""
                                                }},
                                                {{
                                                ""text"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""Efio""
                                                }},
                                                ""value"": ""lead_Xb8JdJdPYo7YfJ7oXro1E4IrcG983NLZYABhWTcSiOq""
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

            var function = new Function();
            JObject payload = await function.BuildSlackPayload("header", "http://test.com");

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
                                                ""value"": ""lead_q9WAvUeMbAj9zBsINtZgzxBTXfMwxixGyYmR9rk0ovP""
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
                                                ""value"": ""lead_q9WAvUeMbAj9zBsINtZgzxBTXfMwxixGyYmR9rk0ovP""
                                                }},
                                                {{
                                                ""text"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""Efio""
                                                }},
                                                ""value"": ""lead_Xb8JdJdPYo7YfJ7oXro1E4IrcG983NLZYABhWTcSiOq""
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
            var function = new Function();
            JObject payload = await function.BuildSlackPayload("header", "http://test.com", "DSB");

            Assert.True(JToken.DeepEquals(expectedJObject, payload));
        }

        [Fact]
        public async Task AnotherTest()
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

            var function = new Function();
            JObject payload = await function.BuildSlackPayload("header", "http://test.com");

            var expectedResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            var actualResponse = await httpClient.PostAsJsonAsync("", payload);

            Assert.Equal(expectedResponse.StatusCode, actualResponse.StatusCode);
        }
    }
}