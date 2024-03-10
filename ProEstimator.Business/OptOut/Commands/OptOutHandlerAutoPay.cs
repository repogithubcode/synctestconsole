using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimator.Business.Resources;
using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Business.OptOut.Commands
{

    /// <summary>
    /// Record that the LoginID does not want to recieve AutoPay
    /// </summary>
    internal class OptOutHandlerAutoPay : IOptOutHandler
    {
        public OptOutType OptOutType { get { return OptOutType.AutoPayFail; } }

        public FunctionResult Process(IOptOutService optOutService, int loginID, int detailID)
        {
            if (optOutService.HasOptedOut(OptOutType.AutoPayFail, loginID, detailID) == false)
            {
                OptOutLog.Insert(loginID, OptOutType.AutoPayFail, detailID);
            }

            return new FunctionResult(true, ProEstBusiness.OptOutAutoPaySuccess);
        }
    }
}
