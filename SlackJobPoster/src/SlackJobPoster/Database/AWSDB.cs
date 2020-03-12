using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;

namespace SlackJobPoster.Database
{
    public class AWSDB : IDBFacade
    {
        public async Task<bool> ItemExists(string key, Table table)
        {
            bool result = false;
            Document doc = await table.GetItemAsync(key);

            if (doc != null)
                result = true;

            return result;
        }
    }
}