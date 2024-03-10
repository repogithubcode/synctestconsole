using ProEstimator.Business.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProEstimator.TimedEvents
{
    public class FetchSentSmsStatusTimedEvents : TimedEvent
    {

        public override TimeSpan TimeSpan { get { return new TimeSpan(0, 0, 30); } }

        public override void DoWork(System.Text.StringBuilder messageBuilder)
        {
            base.DoWork(messageBuilder);
            TextMessageService.UpdateAllQueuedSMSStatus(messageBuilder);
            ExecutionFinished();
        }

    }
}