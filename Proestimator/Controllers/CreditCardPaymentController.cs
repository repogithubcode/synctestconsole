using Kendo.Mvc.UI;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.Logic;
using ProEstimator.Business.Model.CreditCardPayment;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;

namespace Proestimator.Controllers
{
    public class CreditCardPaymentController : SiteController
    {
        private ICreditCardPaymentService _creditCardPaymentService;

        public CreditCardPaymentController(ICreditCardPaymentService creditCardPaymentService)
        {
            _creditCardPaymentService = creditCardPaymentService ?? throw new ArgumentNullException(nameof(creditCardPaymentService));
        }


        [HttpGet]
        [Route("{userID}/creditCardPayment")]
        public ActionResult CreditCardPayment(int userID)
        {
            if (!CreditCardPaymentService.IsAuthorized(ActiveLogin.LoginID) || userID != ActiveLogin.SiteUserID)
            {
                return new HttpStatusCodeResult(404);
            }

            ViewBag.NavID = "creditCardPayment";

            var vm = new CreditCardPaymentNotApprovedVM
            {
            };

            return View(vm);
        }

        [HttpGet]
        [Route("{userID}/creditCardPaymentApproved")]
        public ActionResult CreditCardPaymentApproved(int userID)
        {
            var ccInfo = _creditCardPaymentService.GetMerchantCreditCardPaymentInfo(ActiveLogin.LoginID);
            if (userID != ActiveLogin.SiteUserID
                || !CreditCardPaymentService.IsAuthorized(ActiveLogin.LoginID)
                || string.IsNullOrEmpty(ccInfo?.IntelliPayMerchantKey)
                || string.IsNullOrEmpty(ccInfo?.IntelliPayAPIKey))
            {
                return RedirectToAction("CreditCardPayment");
            }

            ViewBag.NavID = "creditCardPayment";

            var vm = new CreditCardPaymentApprovedVM
            {
                UserID = userID
            };

            return View(vm);
        }

        public ActionResult GetCreditCardPaymentInfoData([DataSourceRequest] DataSourceRequest request, int userID)
        {
            var ccPayments = new List<CreditCardPaymentInfoVM>();

            var loginID = _siteLoginManager.GetActiveLogin(userID, GetComputerKey())?.LoginID ?? 0;
            var ccInfo = _creditCardPaymentService.GetMerchantCreditCardPaymentInfo(loginID);
            if (!CreditCardPaymentService.IsAuthorized(loginID) || string.IsNullOrEmpty(ccInfo?.IntelliPayMerchantKey) || string.IsNullOrEmpty(ccInfo?.IntelliPayAPIKey))
            {
                return Json(ccPayments.ToDataSourceResult(request));

            }

            var db = new DBAccess();
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("LoginID", loginID)
            };

            var tableResult = db.ExecuteWithTable("CreditCardPayment_InfoData_Get", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                ccPayments.Add(new CreditCardPaymentInfoVM(row));
            }

            return Json(ccPayments.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ProcessPaymentCompleteResult(CreditCardPaymentSuccessVM model)
        {
            var estimate = new Estimate(model.EstimateID);
            if (estimate == null || model.LoginID <= 0 || !CreditCardPaymentService.IsAuthorized(model.LoginID))
            {
                return Json(new { Success = false, Message = "Invalid User ID and/or Estimate ID" }, JsonRequestBehavior.AllowGet);
            }

            var ccPaymentId = _creditCardPaymentService.InsertCreditCardPaymentSuccessInfo(model);

            // insert a record into the payment info table
            var paymentInfo = new PaymentInfoData
            {
                AdminInfoID = model.EstimateID,
                PaymentId = 0,
                Amount = Convert.ToDecimal(model.Amount),
                CheckNumber = "",
                Memo = "Credit card payment made using IntelliPay",
                PayeeName = model.NameOnCard,
                PaymentDate = DateTime.UtcNow.ToString(),
                PaymentType = "CC",
                WhoPays = "cp",
                CreditCardPaymentID = ccPaymentId
            };

            var saveResult = paymentInfo.Save(GetActiveLoginID(model.LoginID));

            if (saveResult.Success)
            {
                return Json(new { Success = true, Message = "" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Success = false, Message = "The payment information was not saved successfully." }, JsonRequestBehavior.AllowGet);
        }
    }
}