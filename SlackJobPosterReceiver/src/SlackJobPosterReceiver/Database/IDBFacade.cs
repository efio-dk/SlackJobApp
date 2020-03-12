using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;

namespace SlackJobPosterReceiver.Database
{
    public interface IDBFacade
    {
        Task AddToDB(Dictionary<string, string> parameters);
        Task<Document> GetFromDB(string key);
        Task<List<Document>> GetAllFromDB(string key);
        Task DeleteFromDB(Document document);
    }
}