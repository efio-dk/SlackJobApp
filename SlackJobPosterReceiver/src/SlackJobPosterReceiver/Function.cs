using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using SlackJobPosterReceiver.Database;
using System.Collections.Generic;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SlackJobPosterReceiver
{
    public class Function
    {
        private readonly Utility _utils;

        public Function()
        {
            IDBFacade leadsDB = new AWSDB(GlobalVars.SLACKLEADS_TABLE);
            IDBFacade skillsDB = new AWSDB(GlobalVars.SLACKSKILLS_TABLE);
            _utils = new Utility(leadsDB, skillsDB, new HttpClient());
        }

        public Function(IDBFacade leadsDB, IDBFacade skillsDB)
        {
            _utils = new Utility(leadsDB, skillsDB, new HttpClient());
        }

        public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
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
                await _utils.PayloadRouter(payload);

                // TODO: refactor for more reliability
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK
                };

                context.Logger.LogLine("Finished processing");
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

            return response;
        }
    }
}
