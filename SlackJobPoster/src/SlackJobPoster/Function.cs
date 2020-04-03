using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json.Linq;

using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using SlackMessageBuilder;
using static SlackMessageBuilder.Button;
using Newtonsoft.Json;
using SlackJobPoster.Database;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using SlackJobPoster.API;
using Amazon.CloudWatch.Model;
using Amazon.CloudWatch;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SlackJobPoster
{
    public class Function
    {
        private readonly HttpClient _client;
        private string _webhook_url;
        private readonly ICloseClient _closeApi;

        public Function()
        {
            _client = new HttpClient();
            _closeApi = new CloseClient(_client);
        }

        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            AmazonDynamoDBClient dbClient = new AmazonDynamoDBClient();
            Table skillsTable = Table.LoadTable(dbClient, GlobalVars.SLACKSKILLS_TABLE);

            IDBFacade db = new AWSDB();

            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, skillsTable, db, context);
            }
            await Metrics.CommitDataAsync();
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, Table skillsTable, IDBFacade db, ILambdaContext context)
        {
            Metrics.AddData(new MetricDatum
            {
                MetricName = "IncomingJobPosts",
                Value = 1,
                Unit = StandardUnit.Count,
                TimestampUtc = DateTime.UtcNow,
                Dimensions = new List<Dimension>
                            {
                                new Dimension
                                {
                                    Name = "IncomingJobPosts",
                                    Value = "1"
                                }
                            }
            });

            SecretManager sm = new SecretManager();
            _webhook_url = sm.Get("SLACK_WEBHOOK");

            JObject jobPost = JObject.Parse(message.Body);
            string jobPostHeader = jobPost.Value<string>("header");
            string jobPostUrl = jobPost.Value<string>("sourceId");
            string jobPostCustomer = jobPost.Value<string>("customer");
            List<string> jobPostKeywords = jobPost.Value<JArray>("keywords").ToObject<List<string>>();

            if (await SkillFilterExists(jobPostKeywords, skillsTable, db))
            {
                JObject jsonObject = await BuildSlackPayload(jobPostHeader, jobPostUrl, _closeApi, jobPostCustomer);
                await _client.PostAsJsonAsync(_webhook_url, jsonObject);
                Metrics.AddData(new MetricDatum
                {
                    MetricName = "PostedJobPostsToSlack",
                    Value = 1,
                    Unit = StandardUnit.Count,
                    TimestampUtc = DateTime.UtcNow,
                    Dimensions = new List<Dimension>
                            {
                                new Dimension
                                {
                                    Name = "PostedJobPostsToSlack",
                                    Value = "1"
                                }
                            }
                });
            }

            await Task.CompletedTask;
        }

        public async Task<JObject> BuildSlackPayload(string header, string sourceId, ICloseClient closeApi, string jobPostCustomer = null)
        {
            Dictionary<string, Option> customers = await closeApi.GetListOfCustomers();

            BlocksBuilder builder = new BlocksBuilder();
            StaticSelect customerSelect = new StaticSelect("customer_select", customers.Values.ToList(), "Customer");
            SlackAction actions = new SlackAction("actions")
                            .AddElement(customerSelect);

            // check if we have detected a customer and if so set it as initial option
            if (!string.IsNullOrEmpty(jobPostCustomer))
            {
                actions.AddElement(new Button("addToClose_btn", "Add to Close", ButtonStyle.PRIMARY));
                if (customers.ContainsKey(jobPostCustomer))
                    customerSelect.AddInitialOption(customers[jobPostCustomer]);
            }

            // adding button in the proper place
            actions.AddElement(new Button("qualifyLead_btn", "Qualify Lead", string.IsNullOrEmpty(jobPostCustomer) ? ButtonStyle.PRIMARY : ButtonStyle.DEFAULT));

            builder.AddBlock(new Section(new Text(" ")));
            builder.AddBlock(new Section(new Text(" ")));
            builder.AddBlock(new Section(new Text("*" + header + "*" + Environment.NewLine + sourceId, "mrkdwn"), "msg_header"));
            builder.AddBlock(actions);
            builder.AddBlock(new Divider());

            return builder.GetJObject();
        }

        public async Task<bool> SkillFilterExists(List<string> skills, Table skillsTable, IDBFacade db)
        {
            foreach (string skill in skills)
            {
                if (await db.ItemExists(skill.ToLower(), skillsTable))
                    return true;
            }

            return false;
        }
    }
}
