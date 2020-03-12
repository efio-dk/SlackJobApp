using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace SlackJobPosterReceiver.Database
{
    public class AWSDB : IDBFacade
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly Table _db;

        public AWSDB(string tableName)
        {
            _client = new AmazonDynamoDBClient();
            _db = Table.LoadTable(_client, tableName);
        }

        public async Task AddToDB(Dictionary<string, string> parameters)
        {
            Document document = new Document();

            foreach (KeyValuePair<string, string> param in parameters)
            {
                document.Add(param.Key, param.Value);
            }

            await _db.PutItemAsync(document);
        }

        public async Task<Document> GetFromDB(string key)
        {
            Document document = await _db.GetItemAsync(key);

            return document;
        }

        public async Task<List<Document>> GetAllFromDB(string key)
        {
            ScanFilter filter = new ScanFilter();
            filter.AddCondition(key, ScanOperator.IsNotNull);

            Search search = _db.Scan(filter);
            List<Document> result = new List<Document>();

            do
            {
                result.AddRange(await search.GetNextSetAsync());
            }
            while (!search.IsDone);

            return result;
        }

        public async Task DeleteFromDB(Document document)
        {
            await _db.DeleteItemAsync(document);
        }
    }
}