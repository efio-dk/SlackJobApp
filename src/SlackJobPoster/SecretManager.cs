using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace SlackJobPoster
{
    public static class SecretManager
    {
        public static async Task<string> GetSecret(string secretName, ILambdaContext context)
        {
            string secret = "";

                context.Logger.LogLine("Setup client");
            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.EUWest1);

                context.Logger.LogLine("Setup request");
            GetSecretValueRequest request = new GetSecretValueRequest();
            request.SecretId = secretName;

                context.Logger.LogLine("Setup response");
            GetSecretValueResponse response = null;

            // In this sample we only handle the specific exceptions for the 'GetSecretValue' API.
            // See https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
            // We rethrow the exception by default.

            try
            {
                context.Logger.LogLine("Will start waiting for a secret");
                response = await client.GetSecretValueAsync(request);
                context.Logger.LogLine("Did not get an error");
            }
            catch (Exception e)
            {
                context.Logger.LogLine("Got an error:");
                context.Logger.LogLine(e.Message);
            }

            if (response.SecretString != null)
            {
                secret = response.SecretString;
            }

            return secret;
        }
    }
}