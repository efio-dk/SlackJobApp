using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using SlackJobPosterReceiver.Database;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SlackJobPosterReceiver
{
    public class Function
    {
        private readonly Utility _utils;

        public Function()
        {
            _utils = new Utility(new AWSDB(GlobalVars.SLACKLEADS_TABLE), new HttpClient());
        }

        public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine(request.Body);

            string timestamp = request.Headers["X-Slack-Request-Timestamp"];
            string sigHeader = request.Headers["X-Slack-Signature"];

            context.Logger.LogLine("\n\n" + timestamp);
            context.Logger.LogLine(sigHeader);

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

            //consume request from Slack actions
            JObject payload = Utility.GetBodyJObject(request.Body);

            //depending on the payload, perform needed action
            await _utils.PayloadRouter(payload);

            // TODO: refactor for more reliability
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK
            };

            context.Logger.LogLine("Finished processing");

            return response;
        }
    }
}
