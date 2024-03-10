using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ProEstimator.Admin.ViewModel.Errors;
using ProEstimator.Admin.ViewModel.SalesBoard;
using ProEstimator.Business.Logic.Admin;
using ProEstimator.Business.Model.Admin;
using ProEstimator.Business.Model.Admin.Reports;
using ProEstimatorData;
using ProEstimatorData.DataModel.Admin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace ProEstimator.Admin.Controllers
{
    public class SalesBoardUserDetailsReportController : AdminController
    {
        private readonly SalesBoardReportService _salesService = new SalesBoardReportService();

        public ActionResult Index()
        {
            SalesBoardVM salesBoardVM = new SalesBoardVM();

            salesBoardVM.SelectedSalesRep = Convert.ToString(Request.QueryString["salesRepID"]);
            salesBoardVM.SelectedMonthSold = Convert.ToString(Request.QueryString["month"]);
            salesBoardVM.SelectedYearSold = Convert.ToString(Request.QueryString["year"]);

            return View(salesBoardVM);
        }

        public ActionResult GetSalesBoardUserDetailsReport([DataSourceRequest] DataSourceRequest request, string salesRepID, string month, string year)
        {
            List<SalesBoardUserDetailsReportVM> salesBoardUserDetailsReportVMs = new List<SalesBoardUserDetailsReportVM>();

            string date = PrepareDateWithMonthYear(month, year);

            List<SalesBoardUserDetailsReport> salesBoardUserDetailsReports = null;

            if(salesRepID != "-1")
            {
                salesBoardUserDetailsReports = SalesBoardUserDetailsReport.GetForFilter(salesRepID, date);
            }
            else
            {
                salesBoardUserDetailsReports = SalesBoardUserDetailsReport.GetForDateFilter(date);
            }

            foreach (SalesBoardUserDetailsReport salesBoardUserDetailsReport in salesBoardUserDetailsReports)
            {
                salesBoardUserDetailsReportVMs.Add(new SalesBoardUserDetailsReportVM(salesBoardUserDetailsReport));
            }

            return Json(salesBoardUserDetailsReportVMs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private string PrepareDateWithMonthYear(string month, string year)
        {
            return !string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(year) ? month + "/1/" + year : null;
        }

        [HttpPost]
        public ActionResult DeleteSalesBoard(string salesBoardID)
        {
            FunctionResult functionResult = SalesBoardUserDetailsReport.DeleteSalesBoardUser(Convert.ToInt32(salesBoardID));

            return Json(new { Success = functionResult.Success }, JsonRequestBehavior.AllowGet);
        }
    }
}
