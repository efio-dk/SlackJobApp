using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace SlackJobPoster
{
    public static class SecretManager
    {
        public static async Task<string> GetSecret(string secretName)
        {
            string region = "eu-west-1";
            string secret = "";

            MemoryStream memoryStream = new MemoryStream();

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            GetSecretValueRequest request = new GetSecretValueRequest();
            request.SecretId = secretName;

            GetSecretValueResponse response = null;

            // In this sample we only handle the specific exceptions for the 'GetSecretValue' API.
            // See https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
            // We rethrow the exception by default.

            try
            {
                response = await client.GetSecretValueAsync(request);
            }
            catch (Exception e)
            {
                throw;
            }

            if (response.SecretString != null)
            {
                secret = response.SecretString;
            }
            else
            {
                memoryStream = response.SecretBinary;
                StreamReader reader = new StreamReader(memoryStream);
                secret = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
            }

            return secret;
        }
    }
}