using System;
using Amazon.Lambda.Core;

namespace SlackJobPoster
{
    public static class GlobalVars
    {
        public static readonly string SLACKSKILLS_TABLE = Environment.GetEnvironmentVariable("AWS_TABLE_SLACK_SKILLS");
        public static ILambdaContext CONTEXT;
    }
}