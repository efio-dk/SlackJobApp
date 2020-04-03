using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using SlackJobPosterReceiver.Database;
using System.Collections.Generic;
using Amazon.CloudWatch.Model;
using Amazon.CloudWatch;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SlackJobPosterReceiver
{
    public class Function
    {
        private readonly Utility _utils;
        private readonly HealthChecks _healthChecker;

        public Function()
        {
            HttpClient client = new HttpClient();
            IDBFacade leadsDB = new AWSDB(GlobalVars.SLACKLEADS_TABLE);
            IDBFacade skillsDB = new AWSDB(GlobalVars.SLACKSKILLS_TABLE);

            _utils = new Utility(leadsDB, skillsDB, client);
            _healthChecker = new HealthChecks(leadsDB, client);
        }

        public Function(IDBFacade leadsDB, IDBFacade skillsDB)
        {
            HttpClient client = new HttpClient();

            _utils = new Utility(leadsDB, skillsDB, client);
            _healthChecker = new HealthChecks(leadsDB, client);
        }

        public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                //We know this request must be true to do an healthcheck on the system
                if (!(JObject.Parse(request.Body).SelectToken("healthcheck") is null) && (bool)JObject.Parse(request.Body).SelectToken("healthcheck"))
                {
                    context.Logger.LogLine("Doing healthcheck");
                    //Do healthcheck
                    JObject health = await _healthChecker.CheckHealth();

                    //Create response with the result from the healthcheck
                    APIGatewayProxyResponse healthResponse = new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK
                    };

                    Metrics.AddData(new MetricDatum
                    {
                        MetricName = "HealthChecks",
                        Value = 1,
                        Unit = StandardUnit.Count,
                        TimestampUtc = DateTime.UtcNow,
                        Dimensions = new List<Dimension>
                            {
                                new Dimension
                                {
                                    Name = "Error",
                                    Value = health.SelectToken("error").ToString()
                                },
                            }
                    });

                    context.Logger.LogLine(health.ToString());
                    context.Logger.LogLine("Healthcheck completed");

                    await Metrics.CommitDataAsync();

                    return healthResponse;
                }
            }
            catch{}
            GlobalVars.CONTEXT = context;

            try
            {
                string timestamp = request.Headers["X-Slack-Request-Timestamp"];
                string sigHeader = request.Headers["X-Slack-Signature"];

                string sig_baseString = $"v0:{timestamp}:{request.Body}";
                string hmacSig = $"v0={Utility.ComputeHmacSha256Hash(sig_baseString, GlobalVars.SLACK_VERIFICATION_TOKEN)}";

                if (hmacSig != sigHeader)
                {
                    var invalidResponse = new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized
                    };

                    return invalidResponse;
                }
            }
            catch
            {
                var invalidResponse = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };

                return invalidResponse;
            }

            APIGatewayProxyResponse response;
            // event subscriptions for Slack app
            JObject eventPayload;
            try
            {
                eventPayload = JObject.Parse(request.Body);
            }
            catch
            {
                eventPayload = new JObject();
            }

            if (string.IsNullOrEmpty((string)eventPayload.SelectToken("challenge")) && eventPayload.SelectToken("event") is null)
            {
                //consume request from Slack actions
                JObject payload = Utility.GetBodyJObject(request.Body);

                //depending on the payload, perform needed action
                JObject responseObj = await _utils.PayloadRouter(payload);

                // TODO: refactor for more reliability
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                };
                if (responseObj != null)
                {
                    response.Body = responseObj.ToString();
                    response.Headers = new Dictionary<string, string>()
                    {
                        {"Content-type", "application/json"}
                    };
                }
            }
            else if (!(eventPayload.SelectToken("event") is null))
            {
                await _utils.EventPayloadRouter(eventPayload.SelectToken("event").Value<JObject>());
                string body = "";

                if (!string.IsNullOrEmpty((string)eventPayload.SelectToken("challenge")))
                    body = eventPayload.SelectToken("challenge").Value<string>();

                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = body,
                    Headers = new Dictionary<string, string>()
                    {
                        {"Content-type", "text/plain"}
                    }
                };
            }
            else
            {
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = eventPayload.SelectToken("challenge").Value<string>(),
                    Headers = new Dictionary<string, string>()
                    {
                        {"Content-type", "text/plain"}
                    }
                };
            }

            await Metrics.CommitDataAsync();

            return response;
        }
    }
}
