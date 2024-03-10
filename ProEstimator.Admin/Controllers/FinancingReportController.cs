using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ProEstimator.Admin.ViewModel.Financing;
using ProEstimator.Admin.ViewModel.ManageDocument;
using ProEstimator.Business.Logic;
using ProEstimator.Business.Logic.Admin;
using System.Web.Mvc;

namespace ProEstimator.Admin.Controllers
{
    public class FinancingReportController : AdminController
    {
        [Route("Reports/FinancingReport")]
        public ActionResult Index()
        {
            if (!AdminIsValidated() || !SalesRepPermissionManager.HasPermission(ViewBag.SessionSalesRepID, "FinancingReport"))
            {
                Session["AfterLogin"] = "/Reports/FinancingReport";
                return Redirect("/LogOut");
            }

            var financingPageVM = new FinancingPageVM();

            financingPageVM.MerchantSignupStatusList = new SelectList(FinancingService.Instance.GetFinancingReportSignupStatusList(), "Value", "Text");
            financingPageVM.SelectedMerchantSignupStatus = "";

            return View(financingPageVM);
        }

        public ActionResult GetMerchantSignupsList([DataSourceRequest] DataSourceRequest request, string rangeStart, string rangeEnd,
                                                string loginID, string merchantName, string signupStatus, string minLoanApps)
        {
            if (!AdminIsValidated() || !SalesRepPermissionManager.HasPermission(ViewBag.SessionSalesRepID, "FinancingReport"))
            {
                Session["AfterLogin"] = "/Reports/FinancingReport";
                return Redirect("/LogOut");
            }

            return Json(FinancingService.Instance.GetFinancingReportMerchantSignupData(rangeStart, rangeEnd,
                merchantName, loginID, signupStatus, minLoanApps)?.ToDataSourceResult(request));
        }

        public ActionResult GetMerchantLoanApplicationsList([DataSourceRequest] DataSourceRequest request, string merchantID, bool includeAllLoanApps)
        {
            if (!AdminIsValidated() || !SalesRepPermissionManager.HasPermission(ViewBag.SessionSalesRepID, "FinancingReport"))
            {
                Session["AfterLogin"] = "/Reports/FinancingReport";
                return Redirect("/LogOut");
            }

            return Json(FinancingService.Instance.GetFinancingReportMerchantLoanApplicationsData(merchantID, includeAllLoanApps)?.ToDataSourceResult(request));
        }

        [Route("Reports/FinancingReport/LoanApps")]
        public ActionResult LoanApps()
        {
            if (!AdminIsValidated() || !SalesRepPermissionManager.HasPermission(ViewBag.SessionSalesRepID, "FinancingReport"))
            {
                Session["AfterLogin"] = "/Reports/FinancingReport/LoanApps";
                return Redirect("/LogOut");
            }

            var financingPageVM = new FinancingLoanAppsPageVM();

            financingPageVM.LoanAppStatusList = new SelectList(FinancingService.Instance.GetFinancingReportLoanAppStatusList(), "Value", "Text");
            financingPageVM.SelectedLoanAppStatus = "";

            return View(financingPageVM);
        }

        public ActionResult GetLoanApplicationsList([DataSourceRequest] DataSourceRequest request, string rangeStart, string rangeEnd,
                                                string loginID, string merchantName, string loanAppStatus, string estimateID, string customerName)
        {
            if (!AdminIsValidated() || !SalesRepPermissionManager.HasPermission(ViewBag.SessionSalesRepID, "FinancingReport"))
            {
                Session["AfterLogin"] = "/Reports/FinancingReport/LoanApps";
                return Redirect("/LogOut");
            }

            return Json(FinancingService.Instance.GetFinancingReportLoanApplicationsData(rangeStart, rangeEnd,
                                                loginID, merchantName, loanAppStatus, estimateID, customerName)?.ToDataSourceResult(request));
        }

        public ActionResult GetMerchantSignupLogList([DataSourceRequest] DataSourceRequest request, string merchantID)
        {
            if (!AdminIsValidated() || !SalesRepPermissionManager.HasPermission(ViewBag.SessionSalesRepID, "FinancingReport"))
            {
                Session["AfterLogin"] = "/Reports/FinancingReport";
                return Redirect("/LogOut");
            }

            return Json(FinancingService.Instance.GetFinancingReportMerchantSignupLogData(merchantID)?.ToDataSourceResult(request));
        }

        public ActionResult GetLoanApplicationLogList([DataSourceRequest] DataSourceRequest request, string transactionID)
        {
            if (!AdminIsValidated() || !SalesRepPermissionManager.HasPermission(ViewBag.SessionSalesRepID, "FinancingReport"))
            {
                Session["AfterLogin"] = "/Reports/FinancingReport";
                return Redirect("/LogOut");
            }

            return Json(FinancingService.Instance.GetFinancingReportLoanApplicationLogData(transactionID)?.ToDataSourceResult(request));
        }
    }
}
