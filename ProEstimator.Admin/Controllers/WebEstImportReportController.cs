using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ProEstimator.Admin.ViewModel.Errors;
using ProEstimator.Admin.ViewModel.Payment;
using ProEstimator.Admin.ViewModel.WebEstImportReport;
using ProEstimatorData.DataModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace ProEstimator.Admin.Controllers
{
    public class WebEstImportReportController : AdminController
    {
        [Route("WebEstImportReport/List")]
        public ActionResult Index()
        {
            ImportLineItemVM importLineItemVM = new ImportLineItemVM();
            
            return View(importLineItemVM);
        }

        public ActionResult GetWebEstImportReport([DataSourceRequest] DataSourceRequest request, string startDateFilter, string endDateFilter)
        {
            List<ImportLineItemVM> importLineItemVMs = new List<ImportLineItemVM>();
            
            List<ImportLineItem> importLineItems = ImportLineItem.GetForFilter(startDateFilter ?? "", endDateFilter ?? "");

            foreach (ImportLineItem importLineItem in importLineItems)
            {
                importLineItemVMs.Add(new ImportLineItemVM(importLineItem));
            }

            return Json(importLineItemVMs.ToDataSourceResult(request));
        }
    }
}
