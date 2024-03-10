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

using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.Controllers
{
    public class SalesBoardReportController : AdminController
    {
        private readonly SalesBoardReportService _salesService = new SalesBoardReportService();

        [Route("SalesBoardReport/List")]
        public ActionResult Index()
        {
            SalesBoardVM salesBoardVM = new SalesBoardVM();

            salesBoardVM.SalesRepDDL = new SelectList(GetSalesRepList(), "Value", "Text");
            salesBoardVM.SelectedSalesRep = "-1";

            salesBoardVM.YearSoldDDL = new SelectList(GetYearSoldList(), "Value", "Text");
            salesBoardVM.SelectedMonthSold = DateTime.Now.Month.ToString();

            salesBoardVM.MonthSoldDDL = new SelectList(GetMonthSoldList(), "Value", "Text");
            salesBoardVM.SelectedYearSold = DateTime.Now.Year.ToString();

            salesBoardVM.HasCommitPermission = ProEstimator.Business.Logic.Admin.SalesRepPermissionManager.HasPermission(GetSalesRepID(), "SalesBoardAdd");

            return View(salesBoardVM);
        }

        public ActionResult GetYearToDateSalesBoardReport([DataSourceRequest] DataSourceRequest request)
        {
            List<SalesBoardReportVM> salesBoardReportVMs = new List<SalesBoardReportVM>();

            List<SalesBoardReport> SalesBoardReports = SalesBoardReport.GetForFilter();

            foreach (SalesBoardReport salesBoardReport in SalesBoardReports)
            {
                salesBoardReportVMs.Add(new SalesBoardReportVM(salesBoardReport));
            }

            return Json(salesBoardReportVMs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSalesBoardDetailsReport([DataSourceRequest] DataSourceRequest request, string ytdMonthFilter, string ytdYearFilter)
        {
            List<SalesBoardDetailReportVM> salesBoardDetailReportVMs = new List<SalesBoardDetailReportVM>();

            if (string.IsNullOrEmpty(ytdMonthFilter))
            {
                ytdMonthFilter = DateTime.Now.Month.ToString();
            }

            if (string.IsNullOrEmpty(ytdYearFilter))
            {
                ytdYearFilter = DateTime.Now.Year.ToString();
            }

            string date = PrepareDateWithMonthYear(ytdMonthFilter, ytdYearFilter);
            List<SalesBoardDetailReport> salesBoardDetailReports = SalesBoardDetailReport.GetForFilter(date);

            foreach (SalesBoardDetailReport salesBoardDetailReport in salesBoardDetailReports)
            {
                salesBoardDetailReportVMs.Add(new SalesBoardDetailReportVM(salesBoardDetailReport, ytdMonthFilter, ytdYearFilter));
            }

            ViewBag.Month = ytdMonthFilter;
            ViewBag.Year = ytdYearFilter;

            return Json(salesBoardDetailReportVMs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private string PrepareDateWithMonthYear(string month, string year)
        {
            return !string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(year) ? month + "/1/" + year : null;
        }

        private IEnumerable<SelectListItem> GetSalesRepList()
        {
            List<SalesRepItem> salesRepItems = SalesRepItem.GetForFilter();
            List<SelectListItem> salesRepSelectList = new List<SelectListItem>();
            List<ProEstimatorData.DataModel.SalesRep> salesReps = ProEstimatorData.DataModel.SalesRep.GetAll().Where(o => o.IsSalesRep && o.IsActive).ToList();
            foreach (ProEstimatorData.DataModel.SalesRep salesRep in salesReps)
            {
                salesRepSelectList.Add(new SelectListItem() { Value = salesRep.SalesRepID.ToString(), Text = salesRep.SalesNumber + " - " + salesRep.FirstName + " " + salesRep.LastName });
            }
            // Add an empty for the New drop down
            List<SelectListItem> allSalesRepSelectList = salesRepSelectList.ToList();
            allSalesRepSelectList.Insert(0, new SelectListItem() { Value = "-1", Text = "--Select--" });

            return allSalesRepSelectList;
        }

        public JsonResult InsertSalesBoardReport(SalesBoardInsertVM model)
        {
            model.DateSold = model.SoldMonth + "/1/" + model.SoldYear;

            FunctionResult functionResult = SalesBoardDetailReport.InsertSalesBoard(model.LoginID, model.Est, model.Frame, model.EMS, model.SalesRep, model.DateSold, model.AddUser, model.HasQBExporter, model.ProAdvisor, model.HasBundle, model.HasImageEditor, model.HasEnterpriseReporting);

            return Json(new { Success = functionResult.Success }, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<SelectListItem> GetMonthSoldList()
        {
            string[] monthNames = DateTimeFormatInfo.InvariantInfo.MonthNames;
            List<SelectListItem> monthNameDDL = new List<SelectListItem>();
            int indexOf = 0;

            foreach (string monthName in monthNames)
            {
                indexOf = indexOf + 1;

                if(string.IsNullOrEmpty(monthName))
                {
                    monthNameDDL.Add(new SelectListItem()
                    {
                        Text = "--Month Sold--",
                        Value = "-1"
                    });
                }
                else
                {
                    monthNameDDL.Add(new SelectListItem()
                    {
                        Text = monthName,
                        Value = Convert.ToString(indexOf)
                    });
                }
            }

            return monthNameDDL;
        }

        private IEnumerable<SelectListItem> GetYearSoldList()
        {
            List<SelectListItem> yearDDL = new List<SelectListItem>();
            List<string> years = new List<string>();
            int year = 0;

            yearDDL.Add(new SelectListItem()
            {
                Text = "--Year sold--",
                Value = "-1"
            });

            for (year = DateTime.Now.Year; year >= 2009; year += -1)
            {
                yearDDL.Add(new SelectListItem()
                {
                    Text = Convert.ToString(year),
                    Value = Convert.ToString(year)
                });
            }

            return yearDDL;
        }
    }
}
