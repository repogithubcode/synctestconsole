using ProEstimator.Business.Logic;
using ProEstimatorData.DataModel.Financing;
using System.Web.Mvc;

namespace Proestimator.Controllers
{
    public class FinancingCallbackController : Controller
    {
        [HttpPost]
        [Route("FinancingCallback/Callback/MerchantSignup")]
        public ActionResult LogMerchantSignupCallbackInfo(WisetackMerchantSignupCallbackInfo callbackData)
        {
            if (string.IsNullOrEmpty(callbackData?.MerchantId) || string.IsNullOrEmpty(callbackData?.SignupLink)
                || string.IsNullOrEmpty(callbackData?.Status) || string.IsNullOrEmpty(callbackData?.EventType))
            {
                return new HttpStatusCodeResult(400); // Bad Request
            }

            if (FinancingService.Instance.InsertWisetackMerchantSignupCallbackInfo(callbackData))
            {
                return new HttpStatusCodeResult(200);
            }

            return new HttpStatusCodeResult(500);
        }

        [HttpPost]
        [Route("FinancingCallback/Callback/LoanApplication")]
        public ActionResult LogLoanApplicationCallbackInfo(WisetackLoanApplicationCallbackInfo callbackData)
        {
            if (string.IsNullOrEmpty(callbackData?.TransactionId) || string.IsNullOrEmpty(callbackData?.ChangedStatus)
                 || string.IsNullOrEmpty(callbackData?.RequestedLoanAmount))
            {
                return new HttpStatusCodeResult(400); // Bad Request
            }

            if (FinancingService.Instance.InsertWisetackLoanApplicationCallbackInfo(callbackData))
            {
                return new HttpStatusCodeResult(200);
            }

            return new HttpStatusCodeResult(500);
        }
    }
}