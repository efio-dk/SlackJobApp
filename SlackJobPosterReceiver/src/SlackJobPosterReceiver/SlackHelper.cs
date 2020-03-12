using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SlackMessageBuilder;
using static SlackMessageBuilder.Button;

namespace SlackJobPosterReceiver
{
    public static class SlackHelper
    {
        public static string GetQualificationModal(string message_ts, string hookUrl)
        {
            //create Slack modal
            ModalBuilder builder = new ModalBuilder("Qualify Lead", message_ts).AddPrivateMetadata(hookUrl);
            builder.AddBlock(
                new Input(
                    new PlainTextInput("customer_name", "Customer name goes here")
                    , "Customer name"
                    , "Customer name as it will appear in Close", "customer_block"));

            return builder.GetJObject().ToString();
        }

        public static string GetAddSkillModal()
        {
            //create Slack modal
            ModalBuilder builder = new ModalBuilder("Add Skill", "addSkill_view");
            builder.AddBlock(
                new Input(
                    new PlainTextInput("skill_name", "Skill name goes here")
                    , "Skill name"
                    , "Skill name to be filtered", "addSkill_block"));

            return builder.GetJObject().ToString();
        }

        public static JObject BuildDefaultSlackPayload(string header, Option selectedOption, SlackPostState postState, string leadId = "", Dictionary<string, Option> customers = null)
        {
            BlocksBuilder builder = new BlocksBuilder();
            SlackAction actions = new SlackAction("actions");

            if (!(customers is null))
            {
                StaticSelect customerSelect = new StaticSelect("customer_select", customers.Values.ToList(), "Customer");
                actions.AddElement(customerSelect);

                if (!(selectedOption is null))
                    customerSelect.AddInitialOption(selectedOption);
            }

            actions.AddElement(new Button("addToClose_btn", "Add to Close", ButtonStyle.PRIMARY));

            // adding button in the proper place
            actions.AddElement(new Button("qualifyLead_btn", "Qualify Lead"));

            builder.AddBlock(new Section(new Text(" ")));
            builder.AddBlock(new Section(new Text(" ")));
            builder.AddBlock(new Section(new Text(header, "mrkdwn"), "msg_header"));

            if (postState == SlackPostState.ACTIONS)
                builder.AddBlock(actions);
            else if (postState == SlackPostState.FINAL)
                builder.AddBlock(new Section(new Text($":white_check_mark: *Opportunity added to <https://app.close.com/lead/{leadId}|Close.com>*", "mrkdwn")));

            builder.AddBlock(new Divider());

            return builder.GetJObject();
        }

        public static JObject BuildDefaultSlackHome(string userId, List<string> skilloptions)
        {
            Home homeView = new Home();
            homeView.AddBlock(new Section(new Text(" ")));
            homeView.AddBlock(new Section(new Text(" ")));

            if (skilloptions.Count > 0)
            {
                Section fields = new Section();
                List<Option> options = new List<Option>();

                foreach (string option in skilloptions)
                {
                    fields.AddField(option);
                    options.Add(new Option(option, option));
                }

                //add fields array
                homeView.AddBlock(fields);

                homeView.AddBlock(new Divider());

                Section multiStaticSelect = new Section(new Text("Pick one or more skills to be removed"));
                multiStaticSelect.AddAccessory(new MultiLineSelect(options, "deleteSkills_select", "Select items"));
                homeView.AddBlock(multiStaticSelect);
            }

            homeView.AddBlock(new SlackAction("home_actions").AddElement(new Button("addSkills_btn", "Add skills", ButtonStyle.PRIMARY)));

            ViewBuilder builder = new ViewBuilder(userId, homeView);

            return builder.GetJObject();
        }
    }
}