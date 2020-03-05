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
