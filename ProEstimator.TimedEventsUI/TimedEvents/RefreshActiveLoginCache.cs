using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.TimedEvents
{
    public class RefreshActiveLoginCache : TimedEvent
    {

        public override TimeSpan TimeSpan { get { return new TimeSpan(0, 15, 0); } }

        protected override void LoadData()
        {
            base.LoadData();
        }

        public override void DoWork(System.Text.StringBuilder messageBuilder)
        {
            base.DoWork(messageBuilder);

            try
            {
                System.Net.WebClient client = new System.Net.WebClient();
                client.DownloadData("https://proestimator.web-est.com/Home/CleanActiveLogins");
            }
            catch (Exception ex)
            {
                messageBuilder.AppendLine("Refresh Active Login Cache Error: " + ex.Message + Environment.NewLine + ex.StackTrace);
                ProEstimatorData.ErrorLogger.LogError(ex, 0, "RefreshActiveLoginCacheEvent DoWork");
            }

        }

    }
}