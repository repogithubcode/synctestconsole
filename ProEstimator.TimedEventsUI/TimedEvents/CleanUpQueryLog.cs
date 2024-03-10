using ProEstimator.Business.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;

namespace ProEstimator.TimedEvents
{
    public class CleanUpQueryLog : TimedEvent
    {

        public override TimeSpan TimeSpan { get { return new TimeSpan(24, 0, 0); } }

        public override void DoWork(System.Text.StringBuilder messageBuilder)
        {
            base.DoWork(messageBuilder);

            string connString = System.Configuration.ConfigurationManager.AppSettings["QueryLogConnectionString"];

            DBAccess db = new DBAccess(connString);
            db.ExecuteNonQuery("QueryLog_CleanUp");

            ExecutionFinished();
        }

    }
}