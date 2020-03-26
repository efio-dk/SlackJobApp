using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;

namespace SlackJobPosterReceiver
{
    public static class Metrics
    {
        static private List<MetricDatum> _data;
        private static readonly IAmazonCloudWatch _amazonCloudWatch;

        static Metrics()
        {
            _data = new List<MetricDatum>();
            _amazonCloudWatch = new AmazonCloudWatchClient(RegionEndpoint.EUWest1);
        }

        public static void AddData(MetricDatum data)
        {
            GlobalVars.CONTEXT.Logger.LogLine("Adding data");
            GlobalVars.CONTEXT.Logger.LogLine(System.Environment.StackTrace);
            _data.Add(data);
        }

        public static async Task CommitDataAsync()
        {
            GlobalVars.CONTEXT.Logger.LogLine("Adding metric");
            await _amazonCloudWatch.PutMetricDataAsync(new PutMetricDataRequest
            {
                Namespace = "Slack App",
                MetricData = _data
            });
            GlobalVars.CONTEXT.Logger.LogLine("Finished adding metric");
            _data = new List<MetricDatum>();
        }
    }
}