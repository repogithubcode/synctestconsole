using ProEstimator.Business.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;

using ProEstimatorData;

namespace ProEstimator.TimedEvents
{
    public class EmailEvent : TimedEvent
    {

        public override TimeSpan TimeSpan { get { return new TimeSpan(0, 0, 5); } }

        public override void DoWork(System.Text.StringBuilder messageBuilder)
        {
            base.DoWork(messageBuilder);

            ProcessQueuedEmails();
        }

        private async void ProcessQueuedEmails()
        {

//#if DEBUG
//            await Task.Delay(1);
//#else
            Task task = EmailSender.SendQueuedEmails();
            await task;
//#endif

            ExecutionFinished();
        } 

    }
}