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

using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SlackJobPoster
{
    public class Function
    {
        private HttpClient client;
        private string webhook_url;
        public Function()
        {
            client = new HttpClient();
        }

        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine("BEFORE SECRET");
            webhook_url = await SecretManager.GetSecret("SLACK_WEBHOOK");
            context.Logger.LogLine("SECRET IS: " + webhook_url);

            JObject jobPost = JObject.Parse(message.Body);
            string jobPostHeader = jobPost.Value<string>("header");
            string jobPostUrl = jobPost.Value<string>("sourceId");

            JObject jsonObject = BuildSlackPayload(jobPostHeader, jobPostUrl);

            await client.PostAsJsonAsync(webhook_url, jsonObject);

            await Task.CompletedTask;
        }

        public JObject BuildSlackPayload(string header, string sourceId)
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
