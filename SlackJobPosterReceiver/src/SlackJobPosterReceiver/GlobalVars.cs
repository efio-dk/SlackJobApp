using System;

namespace SlackJobPosterReceiver
{
    public static class GlobalVars
    {
        public static readonly string SLACKLEADS_TABLE = Environment.GetEnvironmentVariable("AWS_TABLE_SLACK_LEADS");
        public static readonly string SLACK_TOKEN = Environment.GetEnvironmentVariable("SLACK_TOKEN");
        public static readonly string CLOSE_TOKEN = Environment.GetEnvironmentVariable("CLOSE_TOKEN");
    }
}