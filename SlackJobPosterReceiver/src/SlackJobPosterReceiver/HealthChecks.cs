using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlackJobPosterReceiver.API;
using SlackJobPosterReceiver.Database;

namespace SlackJobPosterReceiver
{
    public class HealthChecks
    {
        private readonly HttpClient _client;
        private readonly IDBFacade _dbLeads;
        private readonly SlackPoster _slackApi;
        private readonly ClosePoster _closeApi;

        public HealthChecks(IDBFacade dbLeads, HttpClient client = null)
        {
            _dbLeads = dbLeads;
            _client = client ?? new HttpClient();
            _slackApi = new SlackPoster(_client);
            _closeApi = new ClosePoster(_client);
        }

        public async Task<JObject> CheckHealth()
        {
            JObject returnObj = new JObject();
            List<string> messages = new List<string>();
            List<string> errorMessages = new List<string>();
            messages.Add(await CheckClose());
            messages.Add(await CheckSlack());
            messages.Add(await CheckDB());

            bool hasError = false;
            foreach(string msg in messages)
            {
                if(!String.IsNullOrEmpty(msg))
                {
                    hasError = true;
                    errorMessages.Add(msg);
                }
            }

            returnObj.Add("messages", JToken.FromObject(errorMessages));
            returnObj.Add("error", hasError);

            return returnObj;
        }

        private async Task<string> CheckDB()
        {
            string message = "";

            try
            {
                await _dbLeads.GetAllFromDB("skill_name");
            }
            catch (Exception)
            {
                message += "There was a problem with connectivity to DynamoDB" + System.Environment.NewLine;
            }

            return message;
        }

        private async Task<string> CheckSlack()
        {
            string message = "";

            try
            {
                HttpResponseMessage response = await _slackApi.TestAPI();
                if (response.StatusCode != HttpStatusCode.OK)
                    message = "Slack api responded with " + response.StatusCode;
            }
            catch (Exception)
            {
                message += "There was a problem with connectivity to Slack API" + System.Environment.NewLine;
            }

            return message;
        }

        private async Task<string> CheckClose()
        {
            string message = "";

            try
            {
                await _closeApi.GetLeads();
            }
            catch (CloseConnectionException)
            {
                message += typeof(CloseConnectionException) + " There was a problem with connectivity to close API";
            }

            return message;
        }
    }
}