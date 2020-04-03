using Xunit;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using SlackMessageBuilder;
using Moq;
using SlackJobPosterReceiver.Database;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Moq.Protected;
using System.Net;
using System.Web;
using Amazon.DynamoDBv2.DocumentModel;

namespace SlackJobPosterReceiver.Tests
{
    public class FunctionTest
    {
        private readonly Dictionary<string, Option> customers = new Dictionary<string, Option>
            {
                { "DSB", new Option("DSB", "DSB") },
                { "Efio", new Option("Efio", "Efio") }
            };

        private readonly List<string> skills = new List<string>
            {
                "c#",
                ".net"
            };

        public FunctionTest()
        {
        }

        [Fact]
        public async Task GetMethodOKTest()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();
            Function functions = new Function(mockedDB.Object, mockedDB.Object);

            GlobalVars.SLACK_VERIFICATION_TOKEN = "test";

            request = new APIGatewayProxyRequest
            {
                Body = "cGF5bG9hZD0lN0IlMjJ0eXBlJTIyJTNBJTIwJTIyYmxvY2tfYWN0aW9ucyUyMiUyQyUyMnRva2VuJTIyJTNBJTIwJTIyeThvTUxtNmRRa2J1VWREZEpxMVFUbUZmJTIyJTJDJTIyY29udGFpbmVyJTIyJTNBJTIwJTdCJTIybWVzc2FnZV90cyUyMiUzQSUyMCUyMjE1ODI2MjUxMjAuMDAwNDAwJTIyJTdEJTJDJTIydHJpZ2dlcl9pZCUyMiUzQSUyMCUyMjk1NjI1NDM0MDc3MS4xNjI0NDczODE1ODUuZjJjMGYxYTYyODM2MTdmNTMwMDEzYzg5MzNkMzEwNTclMjIlMkMlMjJtZXNzYWdlJTIyJTNBJTIwJTdCJTIyYmxvY2tzJTIyJTNBJTIwJTVCJTdCJTIydHlwZSUyMiUzQSUyMCUyMnNlY3Rpb24lMjIlMkMlMjJibG9ja19pZCUyMiUzQSUyMCUyMklQMGglMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyJTJCJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTdEJTJDJTdCJTIydHlwZSUyMiUzQSUyMCUyMnNlY3Rpb24lMjIlMkMlMjJibG9ja19pZCUyMiUzQSUyMCUyMkxZTCUyRiUyMiUyQyUyMnRleHQlMjIlM0ElMjAlN0IlMjJ0eXBlJTIyJTNBJTIwJTIycGxhaW5fdGV4dCUyMiUyQyUyMnRleHQlMjIlM0ElMjAlMjIlMkIlMjIlMkMlMjJlbW9qaSUyMiUzQSUyMHRydWUlN0QlN0QlMkMlN0IlMjJ0eXBlJTIyJTNBJTIwJTIyc2VjdGlvbiUyMiUyQyUyMmJsb2NrX2lkJTIyJTNBJTIwJTIySlI4VSUyMiUyQyUyMnRleHQlMjIlM0ElMjAlN0IlMjJ0eXBlJTIyJTNBJTIwJTIybXJrZHduJTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMiUyQVdob29vaG9vJTJCSSUyN20lMkJ3b3JraW5nJTJBbiUzQ2h0dHBzJTNBJTJGJTJGZG9lc2l0d29yazg3MDQuam9icy5jb20lM0UlMjIlMkMlMjJ2ZXJiYXRpbSUyMiUzQSUyMGZhbHNlJTdEJTdEJTJDJTdCJTIydHlwZSUyMiUzQSUyMCUyMmRpdmlkZXIlMjIlMkMlMjJibG9ja19pZCUyMiUzQSUyMCUyMk5ERjMlMjIlN0QlMkMlN0IlMjJ0eXBlJTIyJTNBJTIwJTIyYWN0aW9ucyUyMiUyQyUyMmJsb2NrX2lkJTIyJTNBJTIwJTIyYWN0aW9ucyUyMiUyQyUyMmVsZW1lbnRzJTIyJTNBJTIwJTVCJTdCJTIydHlwZSUyMiUzQSUyMCUyMnN0YXRpY19zZWxlY3QlMjIlMkMlMjJhY3Rpb25faWQlMjIlM0ElMjAlMjJjdXN0b21lcl9zZWxlY3QlMjIlMkMlMjJwbGFjZWhvbGRlciUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMkN1c3RvbWVyJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTJDJTIyaW5pdGlhbF9vcHRpb24lMjIlM0ElMjAlN0IlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyRFNCJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTJDJTIydmFsdWUlMjIlM0ElMjAlMjJEU0IlMjIlN0QlMkMlMjJvcHRpb25zJTIyJTNBJTIwJTVCJTdCJTIydGV4dCUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMkRTQiUyMiUyQyUyMmVtb2ppJTIyJTNBJTIwdHJ1ZSU3RCUyQyUyMnZhbHVlJTIyJTNBJTIwJTIyRFNCJTIyJTdEJTJDJTdCJTIydGV4dCUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMkVmaW8lMjIlMkMlMjJlbW9qaSUyMiUzQSUyMHRydWUlN0QlMkMlMjJ2YWx1ZSUyMiUzQSUyMCUyMkVmaW8lMjIlN0QlNUQlN0QlMkMlN0IlMjJ0eXBlJTIyJTNBJTIwJTIyYnV0dG9uJTIyJTJDJTIyYWN0aW9uX2lkJTIyJTNBJTIwJTIyYWRkVG9DbG9zZV9idG4lMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyQWRkJTJCdG8lMkJDbG9zZSUyMiUyQyUyMmVtb2ppJTIyJTNBJTIwdHJ1ZSU3RCUyQyUyMnN0eWxlJTIyJTNBJTIwJTIycHJpbWFyeSUyMiU3RCUyQyU3QiUyMnR5cGUlMjIlM0ElMjAlMjJidXR0b24lMjIlMkMlMjJhY3Rpb25faWQlMjIlM0ElMjAlMjJxdWFsaWZ5TGVhZF9idG4lMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyUXVhbGlmeSUyQkxlYWQlMjIlMkMlMjJlbW9qaSUyMiUzQSUyMHRydWUlN0QlN0QlNUQlN0QlNUQlN0QlMkMlMjJyZXNwb25zZV91cmwlMjIlM0ElMjAlMjJodHRwcyUzQSUyRiUyRmhvb2tzLnNsYWNrLmNvbSUyRmFjdGlvbnMlMkZUNFNENUI3SDclMkY5NjcyNjgyMjYyNzYlMkZXNnZsZjZEVFhBMnViblBWSVA1VnhwblUlMjIlMkMlMjJhY3Rpb25zJTIyJTNBJTIwJTVCJTdCJTIyYWN0aW9uX2lkJTIyJTNBJTIwJTIycXVhbGlmeUxlYWRfYnRuJTIyJTJDJTIyYmxvY2tfaWQlMjIlM0ElMjAlMjJhY3Rpb25zJTIyJTJDJTIydGV4dCUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMlF1YWxpZnklMkJMZWFkJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTJDJTIydHlwZSUyMiUzQSUyMCUyMmJ1dHRvbiUyMiUyQyUyMmFjdGlvbl90cyUyMiUzQSUyMCUyMjE1ODI2MjU3NjcuNDUxNTQxJTIyJTdEJTVEJTdE",
                Headers = new Dictionary<string, string>()
                    {
                        {"X-Slack-Signature", "v0=7d6a7eebd76639d18775db23b56495725fddf00a86e99ff1963fb83d8d3c28d8"},
                        {"X-Slack-Request-Timestamp", "0"}
                    }
            };

            context = new TestLambdaContext();
            response = await functions.Get(request, context);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task GetMethodUnauthorizedTest()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();
            Function functions = new Function(mockedDB.Object, mockedDB.Object);

            GlobalVars.SLACK_VERIFICATION_TOKEN = "test";

            request = new APIGatewayProxyRequest
            {
                Body = "cGF5bG9hZD0lN0IlMjJ0eXBlJTIyJTNBJTIwJTIyYmxvY2tfYWN0aW9ucyUyMiUyQyUyMnRva2VuJTIyJTNBJTIwJTIyeThvTUxtNmRRa2J1VWREZEpxMVFUbUZmJTIyJTJDJTIyY29udGFpbmVyJTIyJTNBJTIwJTdCJTIybWVzc2FnZV90cyUyMiUzQSUyMCUyMjE1ODI2MjUxMjAuMDAwNDAwJTIyJTdEJTJDJTIydHJpZ2dlcl9pZCUyMiUzQSUyMCUyMjk1NjI1NDM0MDc3MS4xNjI0NDczODE1ODUuZjJjMGYxYTYyODM2MTdmNTMwMDEzYzg5MzNkMzEwNTclMjIlMkMlMjJtZXNzYWdlJTIyJTNBJTIwJTdCJTIyYmxvY2tzJTIyJTNBJTIwJTVCJTdCJTIydHlwZSUyMiUzQSUyMCUyMnNlY3Rpb24lMjIlMkMlMjJibG9ja19pZCUyMiUzQSUyMCUyMklQMGglMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyJTJCJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTdEJTJDJTdCJTIydHlwZSUyMiUzQSUyMCUyMnNlY3Rpb24lMjIlMkMlMjJibG9ja19pZCUyMiUzQSUyMCUyMkxZTCUyRiUyMiUyQyUyMnRleHQlMjIlM0ElMjAlN0IlMjJ0eXBlJTIyJTNBJTIwJTIycGxhaW5fdGV4dCUyMiUyQyUyMnRleHQlMjIlM0ElMjAlMjIlMkIlMjIlMkMlMjJlbW9qaSUyMiUzQSUyMHRydWUlN0QlN0QlMkMlN0IlMjJ0eXBlJTIyJTNBJTIwJTIyc2VjdGlvbiUyMiUyQyUyMmJsb2NrX2lkJTIyJTNBJTIwJTIySlI4VSUyMiUyQyUyMnRleHQlMjIlM0ElMjAlN0IlMjJ0eXBlJTIyJTNBJTIwJTIybXJrZHduJTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMiUyQVdob29vaG9vJTJCSSUyN20lMkJ3b3JraW5nJTJBbiUzQ2h0dHBzJTNBJTJGJTJGZG9lc2l0d29yazg3MDQuam9icy5jb20lM0UlMjIlMkMlMjJ2ZXJiYXRpbSUyMiUzQSUyMGZhbHNlJTdEJTdEJTJDJTdCJTIydHlwZSUyMiUzQSUyMCUyMmRpdmlkZXIlMjIlMkMlMjJibG9ja19pZCUyMiUzQSUyMCUyMk5ERjMlMjIlN0QlMkMlN0IlMjJ0eXBlJTIyJTNBJTIwJTIyYWN0aW9ucyUyMiUyQyUyMmJsb2NrX2lkJTIyJTNBJTIwJTIyYWN0aW9ucyUyMiUyQyUyMmVsZW1lbnRzJTIyJTNBJTIwJTVCJTdCJTIydHlwZSUyMiUzQSUyMCUyMnN0YXRpY19zZWxlY3QlMjIlMkMlMjJhY3Rpb25faWQlMjIlM0ElMjAlMjJjdXN0b21lcl9zZWxlY3QlMjIlMkMlMjJwbGFjZWhvbGRlciUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMkN1c3RvbWVyJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTJDJTIyaW5pdGlhbF9vcHRpb24lMjIlM0ElMjAlN0IlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyRFNCJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTJDJTIydmFsdWUlMjIlM0ElMjAlMjJEU0IlMjIlN0QlMkMlMjJvcHRpb25zJTIyJTNBJTIwJTVCJTdCJTIydGV4dCUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMkRTQiUyMiUyQyUyMmVtb2ppJTIyJTNBJTIwdHJ1ZSU3RCUyQyUyMnZhbHVlJTIyJTNBJTIwJTIyRFNCJTIyJTdEJTJDJTdCJTIydGV4dCUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMkVmaW8lMjIlMkMlMjJlbW9qaSUyMiUzQSUyMHRydWUlN0QlMkMlMjJ2YWx1ZSUyMiUzQSUyMCUyMkVmaW8lMjIlN0QlNUQlN0QlMkMlN0IlMjJ0eXBlJTIyJTNBJTIwJTIyYnV0dG9uJTIyJTJDJTIyYWN0aW9uX2lkJTIyJTNBJTIwJTIyYWRkVG9DbG9zZV9idG4lMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyQWRkJTJCdG8lMkJDbG9zZSUyMiUyQyUyMmVtb2ppJTIyJTNBJTIwdHJ1ZSU3RCUyQyUyMnN0eWxlJTIyJTNBJTIwJTIycHJpbWFyeSUyMiU3RCUyQyU3QiUyMnR5cGUlMjIlM0ElMjAlMjJidXR0b24lMjIlMkMlMjJhY3Rpb25faWQlMjIlM0ElMjAlMjJxdWFsaWZ5TGVhZF9idG4lMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyUXVhbGlmeSUyQkxlYWQlMjIlMkMlMjJlbW9qaSUyMiUzQSUyMHRydWUlN0QlN0QlNUQlN0QlNUQlN0QlMkMlMjJyZXNwb25zZV91cmwlMjIlM0ElMjAlMjJodHRwcyUzQSUyRiUyRmhvb2tzLnNsYWNrLmNvbSUyRmFjdGlvbnMlMkZUNFNENUI3SDclMkY5NjcyNjgyMjYyNzYlMkZXNnZsZjZEVFhBMnViblBWSVA1VnhwblUlMjIlMkMlMjJhY3Rpb25zJTIyJTNBJTIwJTVCJTdCJTIyYWN0aW9uX2lkJTIyJTNBJTIwJTIycXVhbGlmeUxlYWRfYnRuJTIyJTJDJTIyYmxvY2tfaWQlMjIlM0ElMjAlMjJhY3Rpb25zJTIyJTJDJTIydGV4dCUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMlF1YWxpZnklMkJMZWFkJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTJDJTIydHlwZSUyMiUzQSUyMCUyMmJ1dHRvbiUyMiUyQyUyMmFjdGlvbl90cyUyMiUzQSUyMCUyMjE1ODI2MjU3NjcuNDUxNTQxJTIyJTdEJTVEJTdE",
                Headers = new Dictionary<string, string>()
                    {
                        {"X-Slack-Signature", "v0=invalidsignature"},
                        {"X-Slack-Request-Timestamp", "0"}
                    }
            };

            context = new TestLambdaContext();
            response = await functions.Get(request, context);
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public async Task GetMethodUnauthorizedWithNoHeadersTest()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();
            Function functions = new Function(mockedDB.Object, mockedDB.Object);

            GlobalVars.SLACK_VERIFICATION_TOKEN = "test";

            request = new APIGatewayProxyRequest
            {
                Body = "cGF5bG9hZD0lN0IlMjJ0eXBlJTIyJTNBJTIwJTIyYmxvY2tfYWN0aW9ucyUyMiUyQyUyMnRva2VuJTIyJTNBJTIwJTIyeThvTUxtNmRRa2J1VWREZEpxMVFUbUZmJTIyJTJDJTIyY29udGFpbmVyJTIyJTNBJTIwJTdCJTIybWVzc2FnZV90cyUyMiUzQSUyMCUyMjE1ODI2MjUxMjAuMDAwNDAwJTIyJTdEJTJDJTIydHJpZ2dlcl9pZCUyMiUzQSUyMCUyMjk1NjI1NDM0MDc3MS4xNjI0NDczODE1ODUuZjJjMGYxYTYyODM2MTdmNTMwMDEzYzg5MzNkMzEwNTclMjIlMkMlMjJtZXNzYWdlJTIyJTNBJTIwJTdCJTIyYmxvY2tzJTIyJTNBJTIwJTVCJTdCJTIydHlwZSUyMiUzQSUyMCUyMnNlY3Rpb24lMjIlMkMlMjJibG9ja19pZCUyMiUzQSUyMCUyMklQMGglMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyJTJCJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTdEJTJDJTdCJTIydHlwZSUyMiUzQSUyMCUyMnNlY3Rpb24lMjIlMkMlMjJibG9ja19pZCUyMiUzQSUyMCUyMkxZTCUyRiUyMiUyQyUyMnRleHQlMjIlM0ElMjAlN0IlMjJ0eXBlJTIyJTNBJTIwJTIycGxhaW5fdGV4dCUyMiUyQyUyMnRleHQlMjIlM0ElMjAlMjIlMkIlMjIlMkMlMjJlbW9qaSUyMiUzQSUyMHRydWUlN0QlN0QlMkMlN0IlMjJ0eXBlJTIyJTNBJTIwJTIyc2VjdGlvbiUyMiUyQyUyMmJsb2NrX2lkJTIyJTNBJTIwJTIySlI4VSUyMiUyQyUyMnRleHQlMjIlM0ElMjAlN0IlMjJ0eXBlJTIyJTNBJTIwJTIybXJrZHduJTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMiUyQVdob29vaG9vJTJCSSUyN20lMkJ3b3JraW5nJTJBbiUzQ2h0dHBzJTNBJTJGJTJGZG9lc2l0d29yazg3MDQuam9icy5jb20lM0UlMjIlMkMlMjJ2ZXJiYXRpbSUyMiUzQSUyMGZhbHNlJTdEJTdEJTJDJTdCJTIydHlwZSUyMiUzQSUyMCUyMmRpdmlkZXIlMjIlMkMlMjJibG9ja19pZCUyMiUzQSUyMCUyMk5ERjMlMjIlN0QlMkMlN0IlMjJ0eXBlJTIyJTNBJTIwJTIyYWN0aW9ucyUyMiUyQyUyMmJsb2NrX2lkJTIyJTNBJTIwJTIyYWN0aW9ucyUyMiUyQyUyMmVsZW1lbnRzJTIyJTNBJTIwJTVCJTdCJTIydHlwZSUyMiUzQSUyMCUyMnN0YXRpY19zZWxlY3QlMjIlMkMlMjJhY3Rpb25faWQlMjIlM0ElMjAlMjJjdXN0b21lcl9zZWxlY3QlMjIlMkMlMjJwbGFjZWhvbGRlciUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMkN1c3RvbWVyJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTJDJTIyaW5pdGlhbF9vcHRpb24lMjIlM0ElMjAlN0IlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyRFNCJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTJDJTIydmFsdWUlMjIlM0ElMjAlMjJEU0IlMjIlN0QlMkMlMjJvcHRpb25zJTIyJTNBJTIwJTVCJTdCJTIydGV4dCUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMkRTQiUyMiUyQyUyMmVtb2ppJTIyJTNBJTIwdHJ1ZSU3RCUyQyUyMnZhbHVlJTIyJTNBJTIwJTIyRFNCJTIyJTdEJTJDJTdCJTIydGV4dCUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMkVmaW8lMjIlMkMlMjJlbW9qaSUyMiUzQSUyMHRydWUlN0QlMkMlMjJ2YWx1ZSUyMiUzQSUyMCUyMkVmaW8lMjIlN0QlNUQlN0QlMkMlN0IlMjJ0eXBlJTIyJTNBJTIwJTIyYnV0dG9uJTIyJTJDJTIyYWN0aW9uX2lkJTIyJTNBJTIwJTIyYWRkVG9DbG9zZV9idG4lMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyQWRkJTJCdG8lMkJDbG9zZSUyMiUyQyUyMmVtb2ppJTIyJTNBJTIwdHJ1ZSU3RCUyQyUyMnN0eWxlJTIyJTNBJTIwJTIycHJpbWFyeSUyMiU3RCUyQyU3QiUyMnR5cGUlMjIlM0ElMjAlMjJidXR0b24lMjIlMkMlMjJhY3Rpb25faWQlMjIlM0ElMjAlMjJxdWFsaWZ5TGVhZF9idG4lMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTdCJTIydHlwZSUyMiUzQSUyMCUyMnBsYWluX3RleHQlMjIlMkMlMjJ0ZXh0JTIyJTNBJTIwJTIyUXVhbGlmeSUyQkxlYWQlMjIlMkMlMjJlbW9qaSUyMiUzQSUyMHRydWUlN0QlN0QlNUQlN0QlNUQlN0QlMkMlMjJyZXNwb25zZV91cmwlMjIlM0ElMjAlMjJodHRwcyUzQSUyRiUyRmhvb2tzLnNsYWNrLmNvbSUyRmFjdGlvbnMlMkZUNFNENUI3SDclMkY5NjcyNjgyMjYyNzYlMkZXNnZsZjZEVFhBMnViblBWSVA1VnhwblUlMjIlMkMlMjJhY3Rpb25zJTIyJTNBJTIwJTVCJTdCJTIyYWN0aW9uX2lkJTIyJTNBJTIwJTIycXVhbGlmeUxlYWRfYnRuJTIyJTJDJTIyYmxvY2tfaWQlMjIlM0ElMjAlMjJhY3Rpb25zJTIyJTJDJTIydGV4dCUyMiUzQSUyMCU3QiUyMnR5cGUlMjIlM0ElMjAlMjJwbGFpbl90ZXh0JTIyJTJDJTIydGV4dCUyMiUzQSUyMCUyMlF1YWxpZnklMkJMZWFkJTIyJTJDJTIyZW1vamklMjIlM0ElMjB0cnVlJTdEJTJDJTIydHlwZSUyMiUzQSUyMCUyMmJ1dHRvbiUyMiUyQyUyMmFjdGlvbl90cyUyMiUzQSUyMCUyMjE1ODI2MjU3NjcuNDUxNTQxJTIyJTdEJTVEJTdE"
            };

            context = new TestLambdaContext();
            response = await functions.Get(request, context);
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public async Task GetMethodEventOKTest()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            var expectedJson = $@"{{""event"": {{""type"":""test""}}}}";

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();
            Function functions = new Function(mockedDB.Object, mockedDB.Object);

            GlobalVars.SLACK_VERIFICATION_TOKEN = "test";

            request = new APIGatewayProxyRequest
            {
                Body = expectedJson,
                Headers = new Dictionary<string, string>()
                    {
                        {"X-Slack-Signature", "v0=7f3dab54dfbdcdf179dd0512a095ad2e16d95b8d04f3e9edadb6eba736c6bfb4"},
                        {"X-Slack-Request-Timestamp", "0"}
                    }
            };

            context = new TestLambdaContext();
            response = await functions.Get(request, context);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task GetMethodChallengeOKTest()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            var expectedJson = $@"{{""challenge"": ""testChallenge""}}";

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();
            Function functions = new Function(mockedDB.Object, mockedDB.Object);

            GlobalVars.SLACK_VERIFICATION_TOKEN = "test";

            request = new APIGatewayProxyRequest
            {
                Body = expectedJson,
                Headers = new Dictionary<string, string>()
                    {
                        {"X-Slack-Signature", "v0=081bf48f25ad36cca05610070585407624080c7de97eb6c3b85d67fb38d51661"},
                        {"X-Slack-Request-Timestamp", "0"}
                    }
            };

            context = new TestLambdaContext();
            response = await functions.Get(request, context);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("testChallenge", response.Body);
        }

        [Fact]
        public void GetBodyJObjectTest()
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
        public void GetModalTest()
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
            JObject payload = JObject.Parse(SlackHelper.GetQualificationModal("0", "test.url"));

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
                                    ""type"": ""section"",
                                    ""block_id"": ""msg_header""
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
        public void ActionsSlackPayloadWithInitOptionTest()
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
                                    ""type"": ""section"",
                                    ""block_id"": ""msg_header""
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
            JObject payload = SlackHelper.BuildDefaultSlackPayload("Test Job Post", option, SlackPostState.ACTIONS, customers: customers);

            Assert.True(JToken.DeepEquals(expectedJObject, payload));
        }

        [Fact]
        public void ActionsSlackPayloadWithoutInitOptionTest()
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
                                    ""type"": ""section"",
                                    ""block_id"": ""msg_header""
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

            JObject payload = SlackHelper.BuildDefaultSlackPayload("Test Job Post", null, SlackPostState.ACTIONS, customers: customers);

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
                                    ""type"": ""section"",
                                    ""block_id"": ""msg_header""
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
        public void GetAddSkillModalTest()
        {
            var expectedJson = $@"{{
                                ""type"": ""modal"",
                                ""title"": {{
                                    ""type"": ""plain_text"",
                                    ""text"": ""Add Skill""
                                }},
                                ""submit"": {{
                                    ""type"": ""plain_text"",
                                    ""text"": ""Submit""
                                }},
                                ""close"": {{
                                    ""type"": ""plain_text"",
                                    ""text"": ""Cancel""
                                }},
                                ""callback_id"": ""addSkill_view"",
                                ""blocks"": [
                                    {{
                                    ""element"": {{
                                        ""placeholder"": {{
                                        ""type"": ""plain_text"",
                                        ""text"": ""Skill name goes here""
                                        }},
                                        ""type"": ""plain_text_input"",
                                        ""action_id"": ""skill_name""
                                    }},
                                    ""label"": {{
                                        ""type"": ""plain_text"",
                                        ""text"": ""Skill name""
                                    }},
                                    ""hint"": {{
                                        ""type"": ""plain_text"",
                                        ""text"": ""Skill name to be filtered""
                                    }},
                                    ""type"": ""input"",
                                    ""block_id"": ""addSkill_block""
                                    }}
                                ]
                                }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            JObject payload = JObject.Parse(SlackHelper.GetAddSkillModal());

            Assert.True(JToken.DeepEquals(expectedJObject, payload));
        }

        [Fact]
        public void SlackHelperBuildDefaultSlackHomeTest()
        {
            var expectedJson = $@"{{
                                ""user_id"": ""testUserId"",
                                ""view"": {{
                                    ""type"": ""home"",
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
                                        ""fields"": [
                                        {{
                                            ""type"": ""plain_text"",
                                            ""text"": ""c#""
                                        }},
                                        {{
                                            ""type"": ""plain_text"",
                                            ""text"": "".net""
                                        }}
                                        ],
                                        ""type"": ""section""
                                    }},
                                    {{
                                        ""type"": ""divider""
                                    }},
                                    {{
                                        ""text"": {{
                                        ""type"": ""plain_text"",
                                        ""text"": ""Pick one or more skills to be removed""
                                        }},
                                        ""accessory"": {{
                                        ""options"": [
                                            {{
                                            ""text"": {{
                                                ""type"": ""plain_text"",
                                                ""text"": ""c#""
                                            }},
                                            ""value"": ""c#""
                                            }},
                                            {{
                                            ""text"": {{
                                                ""type"": ""plain_text"",
                                                ""text"": "".net""
                                            }},
                                            ""value"": "".net""
                                            }}
                                        ],
                                        ""placeholder"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": ""Select items""
                                        }},
                                        ""type"": ""multi_static_select"",
                                        ""action_id"": ""deleteSkills_select""
                                        }},
                                        ""type"": ""section""
                                    }},
                                    {{
                                        ""elements"": [
                                        {{
                                            ""text"": {{
                                            ""type"": ""plain_text"",
                                            ""text"": ""Add skills""
                                            }},
                                            ""style"": ""primary"",
                                            ""type"": ""button"",
                                            ""action_id"": ""addSkills_btn""
                                        }}
                                        ],
                                        ""type"": ""actions"",
                                        ""block_id"": ""home_actions""
                                    }}
                                    ]
                                }}
                            }}";

            JObject expectedJObject = JObject.Parse(expectedJson);

            JObject payload = SlackHelper.BuildDefaultSlackHome("testUserId", skills);
            Assert.True(JToken.DeepEquals(expectedJObject, payload));
        }

        [Fact]
        public async void UtilityAddSkillTest()
        {
            var payloadJson = $@"{{
                                ""type"": ""block_actions"",
                                ""container"": {{
                                    ""type"": ""view""
                                }},
                                ""actions"": [
                                    {{
                                        ""action_id"": ""addSkills_btn""
                                    }}
                                ],
                                ""trigger_id"": ""testTrigger""
                            }}";

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();
            string payloadUrl = SlackHelper.GetAddSkillModal();
            string expectedUrl = "https://slack.com/api/views.open?token=testToken&trigger_id=testTrigger&view=" + HttpUtility.UrlEncode(payloadUrl);

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

            Utility utils = new Utility(mockedDB.Object, mockedDB.Object, httpClient);

            GlobalVars.SLACK_TOKEN = "testToken";

            JObject payload = JObject.Parse(payloadJson);
            await utils.PayloadRouter(payload);

            handlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        r => r.Method == HttpMethod.Get && r.RequestUri == new Uri(expectedUrl)
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [Fact]
        public async void UtilityDeleteSkillTest()
        {
            var payloadJson = $@"{{
                                ""type"": ""block_actions"",
                                ""container"": {{
                                    ""type"": ""view""
                                }},
                                ""actions"": [
                                    {{
                                        ""action_id"": ""deleteSkills_select"",
                                        ""selected_options"": [
                                            {{
                                                ""text"": {{
                                                    ""type"": ""plain_text"",
                                                    ""text"": ""C#""
                                                }},
                                                ""value"": ""c#""
                                            }}
                                        ]
                                    }}
                                ],
                                ""trigger_id"": ""testTrigger"",
                                ""user"": {{
                                    ""id"": ""testId""
                                }}
                            }}";

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();
            List<Document> skillsFromDb = new List<Document> {
                new Document()
                {
                    { "skill_name", "c#"},
                    { "skill_display_name", "C#"}
                },
                new Document()
                {
                    { "skill_name", "java"},
                    { "skill_display_name", "Java"}
                }
            };
            mockedDB.Setup(db => db.GetAllFromDB(It.IsAny<string>())).ReturnsAsync(skillsFromDb);

            List<string> updatedSkills = new List<string> { "Java" };
            JObject updatedMsg = SlackHelper.BuildDefaultSlackHome("testId", updatedSkills);

            const string expectedUrl = "https://slack.com/api/views.publish";

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

            Utility utils = new Utility(mockedDB.Object, mockedDB.Object, httpClient);

            GlobalVars.SLACK_TOKEN = "testToken";

            JObject payload = JObject.Parse(payloadJson);
            await utils.PayloadRouter(payload);

            handlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        r => r.Method == HttpMethod.Post
                        && r.RequestUri == new Uri(expectedUrl)
                        && JToken.DeepEquals(JObject.Parse(r.Content.ReadAsStringAsync().Result), updatedMsg)
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );

            mockedDB.Verify(db => db.DeleteFromDB(It.IsAny<Document>()), Times.Once());
        }

        [Fact]
        public async void UtilityAddSkillViewTest()
        {
            var payloadJson = $@"{{
                                ""type"": ""view_submission"",
                                ""view"": {{
                                    ""callback_id"": ""addSkill_view"",
                                    ""state"": {{
                                        ""values"": {{
                                            ""addSkill_block"":{{
                                                ""skill_name"": {{
                                                    ""value"": ""testSkill""
                                                }}
                                            }}
                                        }}
                                    }}
                                }},
                                ""trigger_id"": ""testTriggerId"",
                                ""user"": {{
                                    ""id"": ""testId""
                                }}
                            }}";

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();
            List<Document> skillsFromDb = new List<Document> {
                new Document()
                {
                    { "skill_name", "c#"},
                    { "skill_display_name", "C#"}
                },
                new Document()
                {
                    { "skill_name", "java"},
                    { "skill_display_name", "Java"}
                }
            };
            mockedDB.Setup(db => db.GetAllFromDB(It.IsAny<string>())).ReturnsAsync(skillsFromDb);

            List<string> updatedSkills = new List<string> { "C#", "Java", "testSkill" };
            JObject updatedMsg = SlackHelper.BuildDefaultSlackHome("testId", updatedSkills);

            const string expectedUrl = "https://slack.com/api/views.publish";

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

            Utility utils = new Utility(mockedDB.Object, mockedDB.Object, httpClient);

            JObject payload = JObject.Parse(payloadJson);
            await utils.PayloadRouter(payload);

            handlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        r => r.Method == HttpMethod.Post
                        && r.RequestUri == new Uri(expectedUrl)
                        && JToken.DeepEquals(JObject.Parse(r.Content.ReadAsStringAsync().Result), updatedMsg)
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );

            mockedDB.Verify(db => db.AddToDB(It.IsAny<Dictionary<string, string>>()), Times.Once());
        }

        [Fact]
        public async void UtilityQualifyLeadViewTest()
        {
            var payloadJson = $@"{{
                                ""type"": ""view_submission"",
                                ""channel"": {{
                                    ""id"": ""testChannelId""
                                }},
                                ""user"": {{
                                    ""id"": ""testUserId""
                                }},
                                ""trigger_id"":""testTriggerId"",
                                ""view"": {{
                                    ""private_metadata"": ""http://testhookurl.com"",
                                    ""callback_id"": ""messageTs"",
                                    ""state"": {{
                                        ""values"": {{
                                            ""customer_block"":{{
                                                ""customer_name"": {{
                                                    ""value"": ""testName""
                                                }}
                                            }}
                                        }}
                                    }}
                                }},
                                ""user"": {{
                                    ""id"": ""testId""
                                }}
                            }}";

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();
            Document leadFromDB = new Document
            {
                { "message_text", "testMessage" }
            };
            mockedDB.Setup(db => db.GetFromDB(It.IsAny<string>())).Returns(Task.FromResult(leadFromDB));

            const string expectedUrl = "http://testhookurl.com";

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
                   Content = new StringContent($@"{{ ""id"": ""testId"", ""Qualified"": ""statusTest"" }}")
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            Utility utils = new Utility(mockedDB.Object, mockedDB.Object, httpClient);

            JObject payload = JObject.Parse(payloadJson);
            await utils.PayloadRouter(payload);

            handlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        r => r.Method == HttpMethod.Post
                        && r.RequestUri == new Uri("https://api.close.com/api/v1/opportunity/")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );

            handlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        r => r.Method == HttpMethod.Post
                        && r.RequestUri == new Uri(expectedUrl)
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );

            mockedDB.Verify(db => db.AddToDB(It.IsAny<Dictionary<string, string>>()), Times.Once());
        }

        [Fact]
        public async void UtilityQualifyLeadBtnTest()
        {
            var payloadJson = $@"{{
                                ""type"": ""block_actions"",
                                ""container"": {{
                                    ""type"": ""message"",
                                    ""message_ts"": ""testTs""
                                }},
                                ""channel"": {{
                                    ""id"": ""testChannelId""
                                }},
                                ""user"": {{
                                    ""id"": ""testUserId""
                                }},
                                ""blocks"": [
                                    {{
                                        ""block_id"": ""msg_header"",
                                        ""text"": {{
                                            ""text"": ""testText""
                                        }}
                                    }}
                                ],
                                ""response_url"": ""http://testresponseurl.com"",
                                ""actions"":[
                                    {{
                                        ""action_id"":""qualifyLead_btn""
                                    }}
                                ],
                                ""trigger_id"":""testTrigger""
                            }}";

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();

            GlobalVars.SLACK_TOKEN = "testToken";
            string expectedView = SlackHelper.GetQualificationModal("testTs", "http://testresponseurl.com");
            string expectedUrl = "https://slack.com/api/views.open?token=" + GlobalVars.SLACK_TOKEN + "&trigger_id=testTrigger&view=" + HttpUtility.UrlEncode(expectedView);

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

            Utility utils = new Utility(mockedDB.Object, mockedDB.Object, httpClient);

            JObject payload = JObject.Parse(payloadJson);
            await utils.PayloadRouter(payload);

            handlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        r => r.Method == HttpMethod.Get
                        && r.RequestUri == new Uri(expectedUrl)
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );

            mockedDB.Verify(db => db.AddToDB(It.IsAny<Dictionary<string, string>>()), Times.Once());
        }

        [Fact]
        public async void UtilityAddToCloseBtnTest()
        {
            var payloadJson = $@"{{
                                ""type"": ""block_actions"",
                                ""channel"": {{
                                    ""id"": ""testChannelId""
                                }},
                                ""user"": {{
                                    ""id"": ""testUserId""
                                }},
                                ""container"": {{
                                    ""type"": ""message"",
                                    ""message_ts"": ""testTs""
                                }},
                                ""blocks"": [
                                    {{
                                        ""block_id"": ""msg_header"",
                                        ""text"": {{
                                            ""text"": ""testText""
                                        }}
                                    }}
                                ],
                                ""elements"": [
                                    {{
                                        ""action_id"": ""customer_select"",
                                        ""initial_option"": {{
                                            ""value"": ""testOption""
                                        }}
                                    }}
                                ],
                                ""response_url"": ""http://testresponseurl.com"",
                                ""actions"":[
                                    {{
                                        ""action_id"":""addToClose_btn""
                                    }}
                                ],
                                ""trigger_id"":""testTrigger""
                            }}";

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();
            List<Document> leadsFromDb = new List<Document> {
                new Document()
                {
                    { "lead_id", "testLeadId"}
                }
            };
            mockedDB.Setup(db => db.GetFromDB(It.IsAny<string>())).ReturnsAsync(leadsFromDb[0]);

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
                   Content = new StringContent($@"{{ ""id"": ""testId"", ""Qualified"": ""statusTest"" }}")
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            Utility utils = new Utility(mockedDB.Object, mockedDB.Object, httpClient);

            JObject payload = JObject.Parse(payloadJson);
            await utils.PayloadRouter(payload);

            handlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        r => r.Method == HttpMethod.Post
                        && r.RequestUri == new Uri("https://api.close.com/api/v1/opportunity/")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );

            handlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        r => r.Method == HttpMethod.Post
                        && r.RequestUri == new Uri("http://testresponseurl.com")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [Fact]
        public async void UtilityCustomerSelectBtnTest()
        {
            var payloadJson = $@"{{
                                ""type"": ""block_actions"",
                                ""channel"": {{
                                    ""id"": ""testChannelId""
                                }},
                                ""user"": {{
                                    ""id"": ""testUserId""
                                }},
                                ""channel"": {{
                                    ""id"": ""testId""
                                }},
                                ""container"": {{
                                    ""type"": ""message"",
                                    ""message_ts"": ""testTs""
                                }},
                                ""blocks"": [
                                    {{
                                        ""block_id"": ""msg_header"",
                                        ""text"": {{
                                            ""text"": ""testText""
                                        }}
                                    }}
                                ],
                                ""response_url"": ""http://testresponseurl.com"",
                                ""actions"":[
                                    {{
                                        ""action_id"": ""customer_select"",
                                        ""selected_option"":{{
                                            ""text"": {{
                                                ""text"": ""optionText""
                                            }},
                                            ""value"": ""optionValue""
                                        }}
                                    }}
                                ],
                                ""trigger_id"":""testTrigger""
                            }}";

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();

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
                   Content = new StringContent($@"{{""data"":[
                       {{""display_name"":""displayTest"",""id"":""testId""}}
                       ]}}")
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            Utility utils = new Utility(mockedDB.Object, mockedDB.Object, httpClient);

            JObject payload = JObject.Parse(payloadJson);
            await utils.PayloadRouter(payload);

            handlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Exactly(2),
                    ItExpr.Is<HttpRequestMessage>(
                        r => r.Method == HttpMethod.Post
                        && r.RequestUri == new Uri("http://testresponseurl.com")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );

            mockedDB.Verify(db => db.AddToDB(It.IsAny<Dictionary<string, string>>()), Times.Once());
        }

        [Fact]
        public async void UtilityEventRouterHomeTest()
        {
            var payloadJson = $@"{{
                                ""type"": ""app_home_opened"",
                                ""user"": ""testId""
                            }}";

            Mock<IDBFacade> mockedDB = new Mock<IDBFacade>();
            List<Document> skillsFromDb = new List<Document> {
                new Document()
                {
                    { "skill_name", "c#"},
                    { "skill_display_name", "C#"}
                },
                new Document()
                {
                    { "skill_name", "java"},
                    { "skill_display_name", "Java"}
                }
            };
            mockedDB.Setup(db => db.GetAllFromDB(It.IsAny<string>())).ReturnsAsync(skillsFromDb);

            List<string> updatedSkills = new List<string> { "C#", "Java" };
            JObject updatedMsg = SlackHelper.BuildDefaultSlackHome("testId", updatedSkills);

            const string expectedUrl = "https://slack.com/api/views.publish";

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

            Utility utils = new Utility(mockedDB.Object, mockedDB.Object, httpClient);

            GlobalVars.SLACK_TOKEN = "testToken";

            JObject payload = JObject.Parse(payloadJson);
            await utils.EventPayloadRouter(payload);

            handlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        r => r.Method == HttpMethod.Post
                        && r.RequestUri == new Uri(expectedUrl)
                        && JToken.DeepEquals(JObject.Parse(r.Content.ReadAsStringAsync().Result), updatedMsg)
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );
        }
    }
}
