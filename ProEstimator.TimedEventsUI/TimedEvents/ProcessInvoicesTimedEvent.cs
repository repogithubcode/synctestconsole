using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimator.Business.Payments;
using ProEstimatorData;

namespace ProEstimator.TimedEvents
{
    public class ProcessInvoicesTimedEvent : TimedEvent
    {

        public override TimeSpan TimeSpan { get { return new TimeSpan(12, 0, 0); } }

        protected override void LoadData()
        {
            base.LoadData();

            // Leave a minute of buffer so it doesn't start right when the program starts, which is annoying when testing other things.
            LastExecution = DateTime.Now.AddHours(-11).AddMinutes(-59);
        }

        public override void DoWork(System.Text.StringBuilder messageBuilder)
        {
            base.DoWork(messageBuilder);

            try
            {
                IPaymentService paymentService = new PaymentService();
                IStripeService stripeService = new StripeService();

                FunctionResult result = paymentService.ProcessAutoPay(stripeService);
                messageBuilder.Append(result.ErrorMessage);

                ExecutionFinished();
            }
            catch (Exception ex)
            {
                messageBuilder.AppendLine("Process Invoices Error: " + ex.Message + Environment.NewLine + ex.StackTrace);
                ProEstimatorData.ErrorLogger.LogError(ex, 0, "ProcessInvoiceTimedEvent DoWork");
            }

        }

    }
}