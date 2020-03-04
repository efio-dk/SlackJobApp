using Xunit;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using SlackMessageBuilder;

namespace SlackJobPosterReceiver.Tests
{
    public class FunctionTest
    {
        public FunctionTest()
        {
        }

        [Fact]
        public async Task TestGetMethod()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Function functions = new Function();

            request = new APIGatewayProxyRequest
            {
                Body = "cGF5bG9hZD0lN0IlMjJ0eXBlJTIyJTNBJTIwJTIyYmxvY2tfYWN0aW9ucyUyMiUyQyUyMnRva2VuJTIyJTNBJTIwJTIyeThvTUxtNmRRa2J1VWREZEpxMVFUbUZmJTIyJTJDJTIyY29udGFpbmVyJTIyJTNBJTIwJTdCJTIybWVzc2FnZV90cyUyMiUzQSUyMCUyMjE1ODI2MjUxMjAuMDAwNDAwJTIyJTdEJTJDJTIydHJpZ2dlcl9pZCUyMiUzQSUyMCUyMjk1NjI1NDM0MDc3MS4xNjI0NDczODE1ODUuZjJjMGYxYTYyODM2MTdmNTMwMDEzYzg5MzNkMzEwNTclMjIlMkMlMjJtZXNzYWdlJTIyJTNBJTIwJTdCJTIyYmxvY2tzJTIyJTNBJTIwJTVCJTdCJTIydHlwZSUyMiUzQSUyMCUyMnNlY3Rpb24lMjIlMkMlMjJibG9ja19pZCUyMiUzQSUyMCUyMklQMGglMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyJTJCJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTdEJTJDJTdCJTIydHlwZSUyMiUzQSUyMCUyMnNlY3Rpb24lMjIlMkMlMjJibG9ja19pZCUyMiUzQSUyMCUyMkxZTCUyRiUyMiUyQyUyMnRleHQlMjIlM0ElMjAlN0IlMjJ0eXBlJTIyJTNBJTIwJTIycGxhaW5fdGV4dCUyMiUyQyUyMnRleHQlMjIlM0ElMjAlMjIlMkIlMjIlMkMlMjJlbW9qaSUyMiUzQSUyMHRydWUlN0QlN0QlMkMlN0IlMjJ0eXBlJTIyJTNBJTIwJTIyc2VjdGlvbiUyMiUyQyUyMmJsb2NrX2lkJTIyJTNBJTIwJTIySlI4VSUyMiUyQyUyMnRleHQlMjIlM0ElMjAlN0IlMjJ0eXBlJTIyJTNBJTIwJTIybXJrZHduJTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMiUyQVdob29vaG9vJTJCSSUyN20lMkJ3b3JraW5nJTJBbiUzQ2h0dHBzJTNBJTJGJTJGZG9lc2l0d29yazg3MDQuam9icy5jb20lM0UlMjIlMkMlMjJ2ZXJiYXRpbSUyMiUzQSUyMGZhbHNlJTdEJTdEJTJDJTdCJTIydHlwZSUyMiUzQSUyMCUyMmRpdmlkZXIlMjIlMkMlMjJibG9ja19pZCUyMiUzQSUyMCUyMk5ERjMlMjIlN0QlMkMlN0IlMjJ0eXBlJTIyJTNBJTIwJTIyYWN0aW9ucyUyMiUyQyUyMmJsb2NrX2lkJTIyJTNBJTIwJTIyYWN0aW9ucyUyMiUyQyUyMmVsZW1lbnRzJTIyJTNBJTIwJTVCJTdCJTIydHlwZSUyMiUzQSUyMCUyMnN0YXRpY19zZWxlY3QlMjIlMkMlMjJhY3Rpb25faWQlMjIlM0ElMjAlMjJjdXN0b21lcl9zZWxlY3QlMjIlMkMlMjJwbGFjZWhvbGRlciUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMkN1c3RvbWVyJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTJDJTIyaW5pdGlhbF9vcHRpb24lMjIlM0ElMjAlN0IlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyRFNCJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTJDJTIydmFsdWUlMjIlM0ElMjAlMjJEU0IlMjIlN0QlMkMlMjJvcHRpb25zJTIyJTNBJTIwJTVCJTdCJTIydGV4dCUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMkRTQiUyMiUyQyUyMmVtb2ppJTIyJTNBJTIwdHJ1ZSU3RCUyQyUyMnZhbHVlJTIyJTNBJTIwJTIyRFNCJTIyJTdEJTJDJTdCJTIydGV4dCUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMkVmaW8lMjIlMkMlMjJlbW9qaSUyMiUzQSUyMHRydWUlN0QlMkMlMjJ2YWx1ZSUyMiUzQSUyMCUyMkVmaW8lMjIlN0QlNUQlN0QlMkMlN0IlMjJ0eXBlJTIyJTNBJTIwJTIyYnV0dG9uJTIyJTJDJTIyYWN0aW9uX2lkJTIyJTNBJTIwJTIyYWRkVG9DbG9zZV9idG4lMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyQWRkJTJCdG8lMkJDbG9zZSUyMiUyQyUyMmVtb2ppJTIyJTNBJTIwdHJ1ZSU3RCUyQyUyMnN0eWxlJTIyJTNBJTIwJTIycHJpbWFyeSUyMiU3RCUyQyU3QiUyMnR5cGUlMjIlM0ElMjAlMjJidXR0b24lMjIlMkMlMjJhY3Rpb25faWQlMjIlM0ElMjAlMjJxdWFsaWZ5TGVhZF9idG4lMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyUXVhbGlmeSUyQkxlYWQlMjIlMkMlMjJlbW9qaSUyMiUzQSUyMHRydWUlN0QlN0QlNUQlN0QlNUQlN0QlMkMlMjJyZXNwb25zZV91cmwlMjIlM0ElMjAlMjJodHRwcyUzQSUyRiUyRmhvb2tzLnNsYWNrLmNvbSUyRmFjdGlvbnMlMkZUNFNENUI3SDclMkY5NjcyNjgyMjYyNzYlMkZXNnZsZjZEVFhBMnViblBWSVA1VnhwblUlMjIlMkMlMjJhY3Rpb25zJTIyJTNBJTIwJTVCJTdCJTIyYWN0aW9uX2lkJTIyJTNBJTIwJTIycXVhbGlmeUxlYWRfYnRuJTIyJTJDJTIyYmxvY2tfaWQlMjIlM0ElMjAlMjJhY3Rpb25zJTIyJTJDJTIydGV4dCUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMlF1YWxpZnklMkJMZWFkJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTJDJTIydHlwZSUyMiUzQSUyMCUyMmJ1dHRvbiUyMiUyQyUyMmFjdGlvbl90cyUyMiUzQSUyMCUyMjE1ODI2MjU3NjcuNDUxNTQxJTIyJTdEJTVEJTdE"
            };

            context = new TestLambdaContext();
            response = await functions.Get(request, context);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public void TestGetBodyJObject()
        {
            // json obj, url encoded and base64 encoded after
            const string payloadEncoded = "cGF5bG9hZD0lN0IlMEElMjAlMjAlMjAlMjAlMjJ0ZXN0JTIyJTNBJTIwJTIydGVzdCUyMiUwQSU3RA==";
            JObject expectedObj = new JObject
            {
                { "test", "test" }
            };

            JObject actualObj = Utility.GetBodyJObject(payloadEncoded);

            Assert.True(JToken.DeepEquals(expectedObj, actualObj));
        }

        [Fact]
        public void TestGetModal()
        {
            var expectedJson = $@"{{
                                    ""type"": ""modal"",
                                    ""title"": {{
                                        ""type"": ""plain_text"",
                                        ""text"": ""Qualify Lead""
                                    }},
                                    ""submit"": {{
                                        ""type"": ""plain_text"",
                                        ""text"": ""Submit""
                                    }},
                                    ""close"": {{
                                        ""type"": ""plain_text"",
                                        ""text"": ""Cancel""
                                    }},
                                    ""callback_id"": ""0"",
                                    ""private_metadata"": ""test.url"",
                                    ""blocks"": [
                                        {{
                                            ""type"": ""input"",
                                            ""block_id"": ""customer_block"",
                                            ""element"": {{
                                                ""type"": ""plain_text_input"",
                                                ""action_id"": ""customer_name"",
                                                ""placeholder"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""Customer name goes here""
                                                }}
                                            }},
                                            ""label"": {{
                                                ""type"": ""plain_text"",
                                                ""text"": ""Customer name""
                                            }},
                                            ""hint"": {{
                                                ""type"": ""plain_text"",
                                                ""text"": ""Customer name as it will appear in Close""
                                            }}
                                        }}
                                    ]
                                }}";

            JObject expectedJObject = JObject.Parse(expectedJson);
            JObject payload = JObject.Parse(SlackHelper.GetModal("0", "test.url"));

            Assert.True(JToken.DeepEquals(expectedJObject, payload));
        }

        [Fact]
        public void InitialSlackPayloadOptionTest()
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
                                        ""text"": ""Test Job Post""
                                    }},
                                    ""type"": ""section""
                                    }},
                                    {{
                                    ""type"": ""divider""
                                    }}
                                ]
                                }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            JObject payload = SlackHelper.BuildDefaultSlackPayload("Test Job Post", null, SlackPostState.INITIAL);

            Assert.True(JToken.DeepEquals(expectedJObject, payload));
        }

        [Fact]
        public void ActionsSlackPayloadWithOptionTest()
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
                                        ""text"": ""Test Job Post""
                                    }},
                                    ""type"": ""section""
                                    }},
                                    {{
                                    ""elements"": [
                                        {{
                                        ""placeholder"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": ""Customer""
                                        }},
                                        ""initial_option"": {{
                                            ""text"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": ""test""
                                            }},
                                            ""value"": ""test""
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
                                        ""text"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": ""Add to Close""
                                        }},
                                        ""style"": ""primary"",
                                        ""type"": ""button"",
                                        ""action_id"": ""addToClose_btn""
                                        }},
                                        {{
                                        ""text"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": ""Qualify Lead""
                                        }},
                                        ""type"": ""button"",
                                        ""action_id"": ""qualifyLead_btn""
                                        }}
                                    ],
                                    ""type"": ""actions"",
                                    ""block_id"": ""actions""
                                    }},
                                    {{
                                    ""type"": ""divider""
                                    }}
                                ]
                                }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            Option option = new Option("test", "test");
            JObject payload = SlackHelper.BuildDefaultSlackPayload("Test Job Post", option, SlackPostState.ACTIONS);

            Assert.True(JToken.DeepEquals(expectedJObject, payload));
        }

        [Fact]
        public void ActionsSlackPayloadWithoutOptionTest()
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
                                        ""text"": ""Test Job Post""
                                    }},
                                    ""type"": ""section""
                                    }},
                                    {{
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
                                        ""text"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": ""Add to Close""
                                        }},
                                        ""style"": ""primary"",
                                        ""type"": ""button"",
                                        ""action_id"": ""addToClose_btn""
                                        }},
                                        {{
                                        ""text"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": ""Qualify Lead""
                                        }},
                                        ""type"": ""button"",
                                        ""action_id"": ""qualifyLead_btn""
                                        }}
                                    ],
                                    ""type"": ""actions"",
                                    ""block_id"": ""actions""
                                    }},
                                    {{
                                    ""type"": ""divider""
                                    }}
                                ]
                                }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            JObject payload = SlackHelper.BuildDefaultSlackPayload("Test Job Post", null, SlackPostState.ACTIONS);

            Assert.True(JToken.DeepEquals(expectedJObject, payload));
        }

        [Fact]
        public void FinalSlackPayloadTest()
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
                                        ""text"": ""Test Job Post""
                                    }},
                                    ""type"": ""section""
                                    }},
                                    {{
                                    ""text"": {{
                                        ""type"": ""mrkdwn"",
                                        ""text"": "":white_check_mark: *Opportunity added to <https://app.close.com/lead/test-lead|Close.com>*""
                                    }},
                                    ""type"": ""section""
                                    }},
                                    {{
                                    ""type"": ""divider""
                                    }}
                                ]
                                }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            JObject payload = SlackHelper.BuildDefaultSlackPayload("Test Job Post", null, SlackPostState.FINAL, "test-lead");

            Assert.True(JToken.DeepEquals(expectedJObject, payload));
        }

        [Fact]
        public void PayloadRouterQualifyLeadTest()
        {
        }
    }
}
