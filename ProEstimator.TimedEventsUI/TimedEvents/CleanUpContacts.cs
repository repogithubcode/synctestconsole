using ProEstimator.Business.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;

namespace ProEstimator.TimedEvents
{
    public class CleanUpContacts : TimedEvent
    {

        public override TimeSpan TimeSpan { get { return new TimeSpan(1, 0, 0); } }

        public override void DoWork(System.Text.StringBuilder messageBuilder)
        {
            base.DoWork(messageBuilder);

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("Maintenance_CleanContacts");

            ExecutionFinished();
        }

    }
}