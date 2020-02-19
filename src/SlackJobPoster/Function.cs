using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json.Linq;
using SlackJobPoster.SlackMessageBuilder;
using static SlackJobPoster.SlackMessageBuilder.Button;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SlackJobPoster
{
    public class Function
    {
        private HttpClient client;
        private static string url = "https://hooks.slack.com/services/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";
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            client = new HttpClient();
        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            JObject jobPost = JObject.Parse(message.Body);
            string jobPostHeader = jobPost.Value<string>("header");
            string jobPostUrl = jobPost.Value<string>("sourceId");

            JObject jsonObject = BuildSlackPayload(jobPostHeader, jobPostUrl);

            HttpResponseMessage response = await client.PostAsJsonAsync(url, jsonObject);

            context.Logger.LogLine($"Processed message {message.Body}");
            context.Logger.LogLine(jsonObject.ToString());

            // TODO: Do interesting work based on the new message
            await Task.CompletedTask;
        }

        private JObject BuildSlackPayload(string header, string sourceId)
        {
            SlackMsgBuilder builder = new SlackMsgBuilder();

            builder.AddBlock(new Section(new Text(" ")));
            builder.AddBlock(new Section(new Text(" ")));
            builder.AddBlock(new Section(new Text("*" + header + "*" + Environment.NewLine + sourceId, "mrkdwn")));
            builder.AddBlock(new Divider());
            builder.AddBlock(new SlackAction("actions")
                            .AddElement(new StaticSelect("customer_select", GetListOfCustomers(), "Customer"))
                            .AddElement(new Button("addToClose_btn", "Add to Close", ButtonStyle.PRIMARY))
                            .AddElement(new Button("qualifyLead_btn", "Qualify Lead")));

            return builder.GetJObject();
        }

        private List<Option> GetListOfCustomers()
        {
            List<Option> customers = new List<Option>();
            customers.Add(new Option("DSB", "DSB"));
            customers.Add(new Option("Efio", "Efio"));

            return customers;
        }
    }
}
