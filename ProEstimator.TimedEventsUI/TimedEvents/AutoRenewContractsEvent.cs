using ProEstimator.Business.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;

using ProEstimatorData;
using ProEstimator.Business.Logic;

namespace ProEstimator.TimedEvents
{
    public class AutoRenewContractsEvent : TimedEvent
    {

        public override TimeSpan TimeSpan { get { return new TimeSpan(1, 0, 0); } }

        public override void DoWork(System.Text.StringBuilder messageBuilder)
        {
            base.DoWork(messageBuilder);

#if !DEBUG
            FunctionResult result = ContractManager.DoAutoRenewContracts();
            if (!result.Success)
            {
                ErrorLogger.LogError(result.ErrorMessage, "AutoRenewContractsEvent Renew");
                messageBuilder.AppendLine(result.ErrorMessage);
            }

            result = ContractManager.DoAutoRenewWarnings();
            if (!result.Success)
            {
                ErrorLogger.LogError(result.ErrorMessage, "AutoRenewContractsEvent Warnings");
                messageBuilder.AppendLine(result.ErrorMessage);
            }
#endif


            ExecutionFinished();
        }

    }
}