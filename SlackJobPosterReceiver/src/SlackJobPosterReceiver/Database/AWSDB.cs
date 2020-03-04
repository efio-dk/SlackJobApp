using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace SlackJobPosterReceiver.Database
{
    public class AWSDB : IDBFacade
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly Table _leadsDB;

        public AWSDB(string tableName)
        {
            _client = new AmazonDynamoDBClient();
            _leadsDB = Table.LoadTable(_client, tableName);
        }

        public async Task AddLeadToDB(string message_ts, string header, string leadId = null)
        {
            Document document = new Document
            {
                ["message_ts"] = message_ts,
                ["message_text"] = header
            };

            if (!(leadId is null))
                document["lead_id"] = leadId;

            await _leadsDB.PutItemAsync(document);
        }

        public async Task<Document> GetLeadFromDB(string message_ts)
        {
            Document leadDocument = await _leadsDB.GetItemAsync(message_ts);

            return leadDocument;
        }
    }
}