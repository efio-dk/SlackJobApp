using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;

namespace SlackJobPosterReceiver.Database
{
    public interface IDBFacade
    {
         Task AddLeadToDB(string message_ts, string header, string leadId = null);
         Task<Document> GetLeadFromDB(string message_ts);
    }
}