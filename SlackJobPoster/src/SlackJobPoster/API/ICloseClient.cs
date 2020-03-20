using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SlackMessageBuilder;

namespace SlackJobPoster.API
{
    public interface ICloseClient
    {
        Task<Dictionary<string, Option>> GetListOfCustomers();
    }
}