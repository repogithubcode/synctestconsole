using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ProEstimator.Admin.ViewModel.InvoiceFailure;
using ProEstimatorData;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.DataRepositories.Contracts;

namespace ProEstimator.Admin.Controllers
{
    public class InvoiceFailureController : AdminController
    {
        private IInvoiceFailureSummaryRepository _failureSummaryService;

        public InvoiceFailureController(IInvoiceFailureSummaryRepository failureSummaryService)
        {
            _failureSummaryService = failureSummaryService;
        }

        [Route("Reports/InvoiceFailures")]
        public ActionResult Index()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/Reports/InvoiceFailures";
                return Redirect("/LogOut");
            }

            if (!ProEstimator.Business.Logic.Admin.SalesRepPermissionManager.HasPermission(GetSalesRepID(), "InvoiceFailureLog"))
            {
                return Redirect("/");
            }

            InvoiceFailurePageVM vm = new InvoiceFailurePageVM();
            return View(vm);
        }


        public ActionResult GetList([DataSourceRequest] DataSourceRequest request, string dayRange)
        {
            int dayRangeValue = InputHelper.GetInteger(dayRange);
            List<InvoiceFailureSummary> invoiceFailureSummaries = _failureSummaryService.Get(dayRangeValue);

            List<InvoiceFailureSummary> returnList = new List<InvoiceFailureSummary>();
            foreach (InvoiceFailureSummary failure in invoiceFailureSummaries)
            {
                if (returnList.FirstOrDefault(o => o.InvoiceID == failure.InvoiceID) == null)
                {
                    returnList.Add(failure);
                }
            }

            return Json(returnList.ToDataSourceResult(request));
        }

        public ActionResult GetDetails([DataSourceRequest] DataSourceRequest request, int invoiceID)
        {
            List<InvoiceFailureLog> failures = InvoiceFailureLog.GetForInvoiceAdmin(invoiceID);
            return Json(failures.ToDataSourceResult(request));
        }
    }
}