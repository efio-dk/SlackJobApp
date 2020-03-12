using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;

namespace SlackJobPoster.Database
{
    public interface IDBFacade
    {
        Task<bool> ItemExists(string hash, Table table);
    }
}
