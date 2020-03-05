using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json.Linq;
using SlackJobPosterReceiver.API;
using SlackJobPosterReceiver.Database;
using SlackMessageBuilder;

namespace SlackJobPosterReceiver
{
    public class Utility
    {
        private readonly HttpClient _client;
        private readonly IDBFacade _db;
        private readonly SlackPoster _slackApi;
        private readonly ClosePoster _closeApi;

        public Utility(IDBFacade db, HttpClient client = null)
        {
            _db = db;
            _client = client ?? new HttpClient();
            _slackApi = new SlackPoster(_client);
            _closeApi = new ClosePoster(_client);
        }

        public static JObject GetBodyJObject(string body)
        {
            string base64Decoded;

            try
            {
                //Convert the base64 encoded body into a string
                string base64Encoded = body;
                byte[] data = System.Convert.FromBase64String(base64Encoded);
                base64Decoded = System.Text.Encoding.ASCII.GetString(data);
            }
            catch
            {
                base64Decoded = body;
            }

            //Convert the resulting query string into a collection separated by keys
            NameValueCollection qscoll = HttpUtility.ParseQueryString(base64Decoded);

            //Convert the JSON string from the key payload into a JObject
            return JObject.Parse(qscoll["payload"]);
        }

        public async Task PayloadRouter(JObject payload)
        {
            if (payload.GetValue("type").Value<string>() == "block_actions")
            {
                string msgTs = payload.SelectToken("container.message_ts").Value<string>();
                // quering with JsonPath queries for easier identification of the elements when in array
                string msgHeader = payload.SelectToken("$..blocks[?(@.block_id=='msg_header')].text.text").Value<string>();
                string hookUrl = payload.SelectToken("response_url").Value<string>();

                switch (payload.SelectToken("actions[0].action_id").Value<string>())
                {
                    case "qualifyLead_btn":
                        string triggerId = payload.GetValue("trigger_id").Value<string>();
                        await QualifyLead(msgTs, msgHeader, hookUrl, triggerId);
                        break;
                    case "addToClose_btn":
                        string optionValue = payload.SelectToken("$...elements[?(@.action_id=='customer_select')].initial_option.value").Value<string>();
                        await AddToClose(msgTs, msgHeader, hookUrl, optionValue);
                        break;
                    case "customer_select":
                        string customerName = payload.SelectToken("actions[0].selected_option.text.text").Value<string>();
                        string leadId = payload.SelectToken("actions[0].selected_option.value").Value<string>();
                        await CustomerSelected(msgTs, msgHeader, hookUrl, customerName, leadId);
                        break;
                }
            }
            else if (payload.GetValue("type").Value<string>() == "view_submission")
            {
                string msgTs = payload.SelectToken("view.callback_id").Value<string>();
                string hookUrl = payload.SelectToken("view.private_metadata").Value<string>();
                string leadName = payload.SelectToken("view.state.values.customer_block.customer_name.value").Value<string>();

                await ViewSubmitted(msgTs, hookUrl, leadName);
            }
        }

        private async Task QualifyLead(string msgTs, string msgHeader, string hookUrl, string triggerId)
        {
            string view = SlackHelper.GetModal(msgTs, hookUrl);

            await _slackApi.TriggerModalOpen(GlobalVars.SLACK_TOKEN, triggerId, view);

            await _db.AddLeadToDB(msgTs, msgHeader);
        }

        private async Task AddToClose(string msgTs, string msgHeader, string hookUrl, string optionValue)
        {
            Document leadDoc = await _db.GetLeadFromDB(msgTs);
            string lead_id;

            if (!(leadDoc is null))
                lead_id = leadDoc["lead_id"].ToString();
            else
                lead_id = optionValue;

            //post opportunity to Close 
            await _closeApi.PostOpportunity(msgHeader, lead_id);

            //post updated message to Slack
            JObject finalCloseMsg = SlackHelper.BuildDefaultSlackPayload(msgHeader, null, SlackPostState.FINAL, lead_id);
            await _slackApi.UpdateMessage(finalCloseMsg, hookUrl);
        }

        private async Task CustomerSelected(string msgTs, string msgHeader, string hookUrl, string customerName, string leadId)
        {
            // persist select choice on DB for each message
            await _db.AddLeadToDB(msgTs, msgHeader, leadId);
            Option selected = new Option(customerName, leadId);

            //post updated message to Slack which will remove the buttons
            JObject updatedMsg = SlackHelper.BuildDefaultSlackPayload(msgHeader, selected, SlackPostState.INITIAL, leadId);
            await _slackApi.UpdateMessage(updatedMsg, hookUrl);

            //post updated message to Slack which will add the right selection and buttons
            updatedMsg = SlackHelper.BuildDefaultSlackPayload(msgHeader, selected, SlackPostState.ACTIONS, leadId);
            await _slackApi.UpdateMessage(updatedMsg, hookUrl);
        }

        private async Task ViewSubmitted(string msgTs, string hookUrl, string leadName)
        {
            Document document = await _db.GetLeadFromDB(msgTs);
            string msgHeader = document["message_text"].ToString();

            //post lead to Close 
            JObject leadObj = await _closeApi.PostLead(leadName);
            string leadId = leadObj.SelectToken("id").Value<string>();

            //post opportunity to Close 
            await _closeApi.PostOpportunity(msgHeader, leadId);

            //post updated message to Slack which will change to the final message
            JObject updatedMsg = SlackHelper.BuildDefaultSlackPayload(msgHeader, null, SlackPostState.FINAL, leadId);
            await _slackApi.UpdateMessage(updatedMsg, hookUrl);

            await _db.AddLeadToDB(msgTs, msgHeader, leadId);
        }
    }
}