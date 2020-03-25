using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
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
        private readonly IDBFacade _dbLeads;
        private readonly IDBFacade _dbSkills;
        private readonly SlackPoster _slackApi;
        private readonly ClosePoster _closeApi;

        public Utility(IDBFacade dbLeads, IDBFacade dbSkills, HttpClient client = null)
        {
            _dbLeads = dbLeads;
            _dbSkills = dbSkills;
            _client = client ?? new HttpClient();
            _slackApi = new SlackPoster(_client);
            _closeApi = new ClosePoster(_client);
        }

        public static JObject GetBodyJObject(string body)
        {
            NameValueCollection qscoll = GetParameterCollection(body);

            //Convert the JSON string from the key payload into a JObject
            return JObject.Parse(qscoll["payload"]);
        }

        public async Task<JObject> PayloadRouter(JObject payload)
        {
            JObject returnObj = new JObject();
            if (payload.GetValue("type").Value<string>() == "block_actions" && !(payload.SelectToken("container.type") is null))
            {
                if (payload.SelectToken("container.type").Value<string>() == "message")
                {
                    string msgTs = payload.SelectToken("container.message_ts").Value<string>();
                    // quering with JsonPath queries for easier identification of the elements when in array
                    string msgHeader = payload.SelectToken("$..blocks[?(@.block_id=='msg_header')].text.text").Value<string>();
                    string hookUrl = payload.SelectToken("response_url").Value<string>();
                    string triggerId = payload.GetValue("trigger_id").Value<string>();

                    switch (payload.SelectToken("actions[0].action_id").Value<string>())
                    {
                        case "qualifyLead_btn":
                            await QualifyLead(msgTs, msgHeader, hookUrl, triggerId);
                            break;
                        case "addToClose_btn":
                            string optionValue = payload.SelectToken("$...elements[?(@.action_id=='customer_select')].initial_option.value").Value<string>();
                            await AddToClose(msgTs, msgHeader, hookUrl, optionValue, triggerId);
                            break;
                        case "customer_select":
                            string customerName = payload.SelectToken("actions[0].selected_option.text.text").Value<string>();
                            string leadId = payload.SelectToken("actions[0].selected_option.value").Value<string>();
                            await CustomerSelected(msgTs, msgHeader, hookUrl, customerName, leadId, triggerId);
                            break;
                    }
                }
                else if (payload.SelectToken("container.type").Value<string>() == "view")
                {
                    switch (payload.SelectToken("actions[0].action_id").Value<string>())
                    {
                        case "addSkills_btn":
                            string triggerId = payload.GetValue("trigger_id").Value<string>();
                            await AddSkill(triggerId);
                            break;
                        case "deleteSkills_select":
                            JArray selectedOptions = payload.SelectToken("actions[0].selected_options").Value<JArray>();
                            string userId = payload.SelectToken("user.id").Value<string>();
                            await DeleteSkill(selectedOptions, userId);
                            break;
                    }
                }
            }
            else if (payload.GetValue("type").Value<string>() == "view_submission")
            {
                string callbackId = payload.SelectToken("view.callback_id").Value<string>();
                string userId = payload.SelectToken("user.id").Value<string>();
                string triggerId = payload.GetValue("trigger_id").Value<string>();

                switch (callbackId)
                {
                    case "addSkill_view":
                        string skillName = payload.SelectToken("view.state.values.addSkill_block.skill_name.value").Value<string>();
                        await AddSkillViewSubmitted(skillName, userId);
                        break;
                    default:
                        string hookUrl = payload.SelectToken("view.private_metadata").Value<string>();
                        string leadName = payload.SelectToken("view.state.values.customer_block.customer_name.value").Value<string>();

                        returnObj = await QualifyLeadViewSubmitted(callbackId, hookUrl, leadName, triggerId);
                        break;
                }
            }

            return returnObj;
        }

        public async Task EventPayloadRouter(JObject eventPayload)
        {
            switch (eventPayload.SelectToken("type").Value<string>())
            {
                case "app_home_opened":

                    List<string> skilloptions = new List<string>();
                    List<Document> skillDocuments = await _dbSkills.GetAllFromDB("skill_name");

                    foreach (Document doc in skillDocuments)
                        skilloptions.Add(doc["skill_display_name"]);

                    //post updated view to Slack Home page
                    JObject updatedMsg = SlackHelper.BuildDefaultSlackHome(eventPayload.SelectToken("user").Value<string>(), skilloptions);
                    await _slackApi.UpdateHomePage(updatedMsg);
                    break;
            }
        }

        private async Task QualifyLead(string msgTs, string msgHeader, string hookUrl, string triggerId)
        {
            string view = SlackHelper.GetQualificationModal(msgTs, hookUrl);

            await _slackApi.TriggerModalOpen(GlobalVars.SLACK_TOKEN, triggerId, view);

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "message_ts", msgTs },
                { "message_text", msgHeader }
            };

            await _dbLeads.AddToDB(parameters);
        }

        private async Task AddToClose(string msgTs, string msgHeader, string hookUrl, string optionValue, string triggerId)
        {
            Document leadDoc = await _dbLeads.GetFromDB(msgTs);
            string lead_id;

            if (!(leadDoc is null))
                lead_id = leadDoc["lead_id"].ToString();
            else
                lead_id = optionValue;

            try
            {
                //post opportunity to Close 
                await _closeApi.PostOpportunity(msgHeader, lead_id, "Qualified");

                //post updated message to Slack
                JObject finalCloseMsg = SlackHelper.BuildDefaultSlackPayload(msgHeader, null, SlackPostState.FINAL, lead_id);
                await _slackApi.UpdateMessage(finalCloseMsg, hookUrl);
            }
            catch (CloseConnectionException e)
            {
                string view = SlackHelper.GetErrorModal(":see_no_evil: :heavy_multiplication_x: There was a problem connecting to Close. Try again later.");

                await _slackApi.TriggerModalOpen(GlobalVars.SLACK_TOKEN, triggerId, view);
            }
        }

        private async Task CustomerSelected(string msgTs, string msgHeader, string hookUrl, string customerName, string leadId, string triggerId)
        {
            //build document to be persisted in DB
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "message_ts", msgTs },
                { "message_text", msgHeader },
                { "lead_id", leadId }
            };

            // persist select choice on DB for each message
            await _dbLeads.AddToDB(parameters);
            Option selected = new Option(customerName, leadId);

            try
            {
                //post updated message to Slack which will remove the buttons
                JObject updatedMsg = SlackHelper.BuildDefaultSlackPayload(msgHeader, selected, SlackPostState.INITIAL, leadId, await GetListOfCustomers());
                await _slackApi.UpdateMessage(updatedMsg, hookUrl);

                //post updated message to Slack which will add the right selection and buttons
                updatedMsg = SlackHelper.BuildDefaultSlackPayload(msgHeader, selected, SlackPostState.ACTIONS, leadId, await GetListOfCustomers());
                await _slackApi.UpdateMessage(updatedMsg, hookUrl);
            }
            catch (CloseConnectionException e)
            {
                string view = SlackHelper.GetErrorModal(":see_no_evil: :heavy_multiplication_x: There was a problem connecting to Close. Try again later.");

                await _slackApi.TriggerModalOpen(GlobalVars.SLACK_TOKEN, triggerId, view);
            }
        }

        private async Task<JObject> QualifyLeadViewSubmitted(string msgTs, string hookUrl, string leadName, string triggerId)
        {
            JObject returnObj = new JObject();
            Document document = await _dbLeads.GetFromDB(msgTs);
            string msgHeader = document["message_text"].ToString();

            try
            {
                //post lead to Close 
                JObject leadObj = await _closeApi.PostLead(leadName);
                string leadId = leadObj.SelectToken("id").Value<string>();

                //post opportunity to Close 
                await _closeApi.PostOpportunity(msgHeader, leadId, "Qualified");

                //post updated message to Slack which will change to the final message
                JObject updatedMsg = SlackHelper.BuildDefaultSlackPayload(msgHeader, null, SlackPostState.FINAL, leadId);
                await _slackApi.UpdateMessage(updatedMsg, hookUrl);

                //build document to be persisted in DB
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "message_ts", msgTs },
                    { "message_text", msgHeader },
                    { "lead_id", leadId }
                };

                await _dbLeads.AddToDB(parameters);
            }
            catch (CloseConnectionException e)
            {
                string view = SlackHelper.GetErrorModal(":see_no_evil: :heavy_multiplication_x: There was a problem connecting to Close. Try again later.");

                returnObj = await _slackApi.TriggerModalOpen(GlobalVars.SLACK_TOKEN, triggerId, view, true);
            }

            return returnObj;
        }

        private async Task AddSkill(string triggerId)
        {
            string view = SlackHelper.GetAddSkillModal();

            await _slackApi.TriggerModalOpen(GlobalVars.SLACK_TOKEN, triggerId, view);
        }

        private async Task AddSkillViewSubmitted(string skillName, string userId)
        {
            string lowerSkillName = skillName.ToLower();

            List<string> skilloptions = new List<string>();
            List<Document> skillDocuments = await _dbSkills.GetAllFromDB("skill_name");

            foreach (Document doc in skillDocuments)
                skilloptions.Add(doc["skill_display_name"]);

            //adding the new skill to Home page
            skilloptions.Add(skillName);

            //post updated view to Slack Home page
            JObject updatedMsg = SlackHelper.BuildDefaultSlackHome(userId, skilloptions);
            await _slackApi.UpdateHomePage(updatedMsg);

            //build document to be persisted in DB
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "skill_name", lowerSkillName },
                { "skill_display_name", skillName }
            };

            await _dbSkills.AddToDB(parameters);
        }

        private async Task DeleteSkill(JArray selectedSkills, string userId)
        {
            List<Document> skillDocuments = await _dbSkills.GetAllFromDB("skill_name");
            List<string> skilloptions = new List<string>();
            List<string> newOptions = new List<string>();

            foreach (JObject skill in selectedSkills)
                skilloptions.Add(skill.SelectToken("value").Value<string>().ToLower());

            //remove skills that were specified
            foreach (Document skillDoc in skillDocuments)
            {
                if (!skilloptions.Contains(skillDoc["skill_name"]))
                {
                    newOptions.Add(skillDoc["skill_display_name"]);
                }
            }

            //post updated view to Slack Home page
            JObject updatedMsg = SlackHelper.BuildDefaultSlackHome(userId, newOptions);
            await _slackApi.UpdateHomePage(updatedMsg);

            //delete from DB
            foreach (Document skillDoc in skillDocuments)
            {
                if (skilloptions.Contains(skillDoc["skill_name"]))
                {
                    await _dbSkills.DeleteFromDB(skillDoc);
                }
            }
        }

        private static NameValueCollection GetParameterCollection(string queryString)
        {
            string base64Decoded;

            try
            {
                //Convert the base64 encoded queryString into a string
                string base64Encoded = queryString;
                byte[] data = System.Convert.FromBase64String(base64Encoded);
                base64Decoded = System.Text.Encoding.ASCII.GetString(data);
            }
            catch
            {
                base64Decoded = queryString;
            }

            //Convert the resulting query string into a collection separated by keys
            return HttpUtility.ParseQueryString(base64Decoded);
        }

        public static string ComputeHmacSha256Hash(string rawData, string key)
        {
            HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(key));
            StringBuilder sb = new StringBuilder();
            byte[] calc_sig = hmac.ComputeHash(Encoding.ASCII.GetBytes(rawData));
            for (int i = 0; i < calc_sig.Length; i++)
            {
                sb.Append(calc_sig[i].ToString("x2"));
            }

            return sb.ToString();
        }

        //get list of cutomers from Close
        public async Task<Dictionary<string, Option>> GetListOfCustomers()
        {
            Dictionary<string, Option> customers = new Dictionary<string, Option>();
            JObject leads = await _closeApi.GetLeads();

            foreach (JObject lead in leads.SelectToken("data").Value<JArray>())
            {
                customers.Add(lead["display_name"].Value<string>(), new Option(lead["display_name"].Value<string>(), lead["id"].Value<string>()));
            }

            return customers;
        }
    }
}