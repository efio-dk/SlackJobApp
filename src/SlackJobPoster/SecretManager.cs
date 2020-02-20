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

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.EUWest1);

            GetSecretValueRequest request = new GetSecretValueRequest();
            request.SecretId = secretName;

            GetSecretValueResponse response = null;

            // In this sample we only handle the specific exceptions for the 'GetSecretValue' API.
            // See https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
            // We rethrow the exception by default.

            try
            {
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