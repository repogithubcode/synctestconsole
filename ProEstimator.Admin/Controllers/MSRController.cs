using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Linq;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimator.Business.Model.Admin;

using ProEstimator.Admin.ViewModel;
using ProEstimator.Admin.ViewModel.Contracts;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.Business.Logic;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.Controllers
{
    public class MSRController : AdminController
    {
        public MSRController()
        {

        }

        public ActionResult Index()
        {
            WebEstMonthlySalesVM webEstMonthlySalesVM = null;
            webEstMonthlySalesVM = new WebEstMonthlySalesVM();

            return View(webEstMonthlySalesVM);
        }

        private List<MonthlySalesVM> BuildMonthlySalesData(List<WebEstMonthlySales> WebEstMonthlySalesColl)
        {
            // New Sales
            MonthlySalesVM msrvmObj = null;
            List<MonthlySalesVM> MSRVMColl = new List<MonthlySalesVM>();

            foreach (WebEstMonthlySales eachWebEstMonthlySales in WebEstMonthlySalesColl)
            {
                // create object 1
                msrvmObj = new MonthlySalesVM(eachWebEstMonthlySales);

                MSRVMColl.Add(msrvmObj);
            }

            return MSRVMColl;
        }

        private List<MonthlySalesVM> BuildMonthlyRenewalSalesData()
        {
            // New Sales
            List<MonthlySalesVM> MSRVMColl = new List<MonthlySalesVM>();

            return MSRVMColl;
        }

        public ActionResult SaveNewMonthlySales(List<MonthlySalesVM> monthlySalesNew, FormCollection argFormColl)  
        {
            WebEstMonthlySalesVM webEstMonthlySalesVM = null;

            try  
            {
                // Convert a List<> Into XML using LINQ
                // https://www.dotnetcurry.com/ShowArticle.aspx?ID=428 
                try
                {
                    SaveMonthlySales(monthlySalesNew);

                    int month = Convert.ToInt32(HttpContext.Session["month"]);
                    int year =  Convert.ToInt32(HttpContext.Session["year"]);

                    webEstMonthlySalesVM = FindMonthlySales(month, year);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return View("Index", webEstMonthlySalesVM);
            }
            catch (Exception ex)  
            {  

            }
            return RedirectToAction("Index");  
        }

        public ActionResult SaveRenewalMonthlySales(List<MonthlySalesVM> MonthlySalesRenewal, FormCollection argFormColl)
        {
            WebEstMonthlySalesVM webEstMonthlySalesVM = null;

            try
            {
                // Convert a List<> Into XML using LINQ
                // https://www.dotnetcurry.com/ShowArticle.aspx?ID=428 
                try
                {
                    int month = Convert.ToInt32(HttpContext.Session["month"]);
                    int year = Convert.ToInt32(HttpContext.Session["year"]);

                    SaveMonthlySales(MonthlySalesRenewal);

                    webEstMonthlySalesVM = FindMonthlySales(month, year);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return View("Index", webEstMonthlySalesVM);
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("Index");
        }

        private void SaveMonthlySales(List<MonthlySalesVM> monthlySalesColl)
        {
            try
            {
                // Convert a List<> Into XML using LINQ
                // https://www.dotnetcurry.com/ShowArticle.aspx?ID=428 
                try
                {
                    XElement xElementWebEstMonthlySale = new XElement("WebEstMonthlySales",
                                                    from WebEstMonthlySaleObj in monthlySalesColl
                                                    select new XElement("WebEstMonthlySales",
                                                                   new XElement("WebEstMonthlySalesID", WebEstMonthlySaleObj.WebEstMonthlySalesID),
                                                                   new XElement("walkthough", WebEstMonthlySaleObj.walkthough)
                                                    ));

                    WebEstMonthlySalesService.UpdateWebEstMonthlySales(xElementWebEstMonthlySale.ToString());
                }
                catch (Exception ex)
                {
                    
                }
            }
            catch (Exception ex)
            {

            }
        }

        public ActionResult SearchMonthlySales(FormCollection argFormColl)
        {
            try
            {
                int month = Convert.ToInt32(argFormColl["SelectedMonth"]);
                int year = Convert.ToInt32(argFormColl["SelectedYear"]);

                HttpContext.Session["month"] = month;
                HttpContext.Session["year"] = year;

                WebEstMonthlySalesVM webEstMonthlySalesVM = null;
                webEstMonthlySalesVM = FindMonthlySales(month, year);

                // MonthSoldList
                webEstMonthlySalesVM.MonthSoldList = SetListItem(webEstMonthlySalesVM.MonthSoldList,month);

                // YearSoldList
                webEstMonthlySalesVM.YearSoldList = SetListItem(webEstMonthlySalesVM.YearSoldList, year);

                return View("Index",webEstMonthlySalesVM);
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("Index");
        }

        private List<SelectListItem> SetListItem(List<SelectListItem> argSoldList, int valueToCompare)
        {
            SelectListItem selectListItem = argSoldList.
                        Where(eachSold => eachSold.Value == Convert.ToString(valueToCompare)).FirstOrDefault();

            selectListItem.Selected = true;

            return argSoldList;
        }

        public WebEstMonthlySalesVM FindMonthlySales(int month, int year)
        {
            WebEstSales webEstSales = new WebEstSales();
            WebEstMonthlySalesVM webEstMonthlySalesVM = null;
            webEstMonthlySalesVM = new WebEstMonthlySalesVM();

            try
            {
                List<WebEstMonthlySales> WebEstMonthlySalesColl_New = new List<WebEstMonthlySales>();
                List<WebEstMonthlySales> WebEstMonthlySalesColl_Renewal = new List<WebEstMonthlySales>();
                List<WebEstMonthlySalesSummary> WebEstMonthlySales_Summary = new List<WebEstMonthlySalesSummary>();

                webEstSales = webEstSales.GetForFilter(month, year);
                
                // BuildMonthlyNewSalesData
                List<MonthlySalesVM> monthlySalesNewColl = BuildMonthlySalesData(webEstSales.WebEstMonthlySalesNew);
                webEstMonthlySalesVM.MonthlySalesNew = monthlySalesNewColl;

                // BuildMonthlyRenewalSalesData
                List<MonthlySalesVM> monthlySalesRenewColl = BuildMonthlySalesData(webEstSales.WebEstMonthlySalesRenew);
                webEstMonthlySalesVM.MonthlySalesRenewal = monthlySalesRenewColl;

                // BuildMonthlyNonRenewalSalesData
                webEstMonthlySalesVM.CustCount = webEstSales.WebEstMonthlySalesSummary[0].CustCount;
                webEstMonthlySalesVM.RegContracts = webEstSales.WebEstMonthlySalesSummary[0].RegContracts;
                webEstMonthlySalesVM.CommPkg = webEstSales.WebEstMonthlySalesSummary[0].CommPkg;
                webEstMonthlySalesVM.Frame = webEstSales.WebEstMonthlySalesSummary[0].Frame;
                webEstMonthlySalesVM.MTHCustCount = webEstSales.WebEstMonthlySalesSummary[0].MTHCustCount;
                webEstMonthlySalesVM.YTDCustCount = webEstSales.WebEstMonthlySalesSummary[0].YTDCustCount;
                webEstMonthlySalesVM.MTHTotalPrice = webEstSales.WebEstMonthlySalesSummary[0].MTHTotalPrice;
                webEstMonthlySalesVM.YTDTotalPrice = webEstSales.WebEstMonthlySalesSummary[0].YTDTotalPrice;
                webEstMonthlySalesVM.AVG1YTDTotalPrice = webEstSales.WebEstMonthlySalesSummary[0].AVG1YTDTotalPrice;
                webEstMonthlySalesVM.AVG2YTDTotalPrice = webEstSales.WebEstMonthlySalesSummary[0].AVG2YTDTotalPrice;

                // MonthSoldList
                webEstMonthlySalesVM.MonthSoldList = SetListItem(webEstMonthlySalesVM.MonthSoldList, month);

                // YearSoldList
                webEstMonthlySalesVM.YearSoldList = SetListItem(webEstMonthlySalesVM.YearSoldList, year);
            }
            catch (Exception ex)
            {

            }

            return webEstMonthlySalesVM;
        }
    }
}