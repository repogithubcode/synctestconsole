using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.IO;
using System.Configuration;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Admin;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.Business.Logic;

using ProEstimator.Admin.ViewModel.RenewalReport;
using ProEstimator.Admin.ViewModel;
using ProEstimator.Admin.ViewModel.MonthlySalesDashboard;

namespace ProEstimator.Admin.Controllers
{
    public class MonthlySalesDashboardReportController : AdminController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Reports/MonthlySalesDashboardReport")]
        public ActionResult Index()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/Reports/MonthlySalesDashboardReport";
                return Redirect("/LogOut");
            }
            else
            {
                MonthlySalesDashboardPageVM vm = new MonthlySalesDashboardPageVM();

                vm.GoodData = true;

                SalesRep salesRep = SalesRep.Get(GetSalesRepID());
                if (salesRep != null)
                {
                    vm.SessionSalesRepID = salesRep.SalesRepID;
                    vm.IsAdmin = ProEstimator.Business.Logic.Admin.SalesRepPermissionManager.HasPermission(salesRep.SalesRepID, "EditBonusGoals");
                }

                return View(vm);
            }
        }

        public ActionResult GetNewSalesReport([DataSourceRequest] DataSourceRequest request, string monthFilter, string yearFilter)
        {
            List<MonthlySalesVM> MonthlySalesVMs = new List<MonthlySalesVM>();

            int month = InputHelper.GetInteger(monthFilter);
            int year = InputHelper.GetInteger(yearFilter);

            List<MonthlyNewSales> monthlyNewSalesColl = MonthlyNewSales.GetForFilter(month, year);

            foreach (MonthlyNewSales monthlyNewSales in monthlyNewSalesColl)
            {
                MonthlySalesVMs.Add(new MonthlySalesVM(monthlyNewSales));
            }

            return Json(MonthlySalesVMs.ToDataSourceResult(request));
        }

        public JsonResult GetPayCodeCommission(string salesType)
        {
            List<PayCodeCommissionVM> PayCodeCommissionVMs = new List<PayCodeCommissionVM>();

            List<PayCodeCommission> PayCodeCommissions = PayCodeCommission.Get();
            PayCodeCommissions = PayCodeCommissions.Where(eachPayCodeCommission => eachPayCodeCommission.SalesType == salesType).
                                                        ToList<PayCodeCommission>();

            foreach (PayCodeCommission PayCodeCommission in PayCodeCommissions)
            {
                PayCodeCommissionVMs.Add(new PayCodeCommissionVM(PayCodeCommission));
            }

            return Json(PayCodeCommissionVMs, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveMonthlySales(int contractID, int previousCycleContractID, string walkThrough, string payCode, string commission, string muCommission,
                                string qbCommission, string paymentExtension)
        {
            try
            {
                ContractPayCodeCommissionMapping contractPayCodeCommMapping = new ContractPayCodeCommissionMapping();
                contractPayCodeCommMapping.ContractID = contractID;
                contractPayCodeCommMapping.PreviousCycleContractID = previousCycleContractID;

                if(string.IsNullOrEmpty(walkThrough))
                    contractPayCodeCommMapping.WalkThrough = null;
                else
                    contractPayCodeCommMapping.WalkThrough = InputHelper.GetDateTime(walkThrough);

                contractPayCodeCommMapping.PayCode = InputHelper.GetInteger(payCode);
                contractPayCodeCommMapping.Commission = InputHelper.GetDouble(commission);
                contractPayCodeCommMapping.MUCommission = InputHelper.GetDouble(muCommission);
                contractPayCodeCommMapping.QBCommission = InputHelper.GetDouble(qbCommission);
                contractPayCodeCommMapping.PaymentExtension = InputHelper.GetBoolean(paymentExtension);

                SaveResult saveResult = contractPayCodeCommMapping.Save();
                return Json(saveResult.ErrorMessage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "MonthlySalesDashboardReport SaveMonthlySales");
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetRenewSalesReport([DataSourceRequest] DataSourceRequest request, string monthFilter, string yearFilter)
        {
            List<MonthlySalesVM> monthlySalesVMs = new List<MonthlySalesVM>();

            int month = InputHelper.GetInteger(monthFilter);
            int year = InputHelper.GetInteger(yearFilter);

            List<MonthlyRenewSales> monthlyRenewSalesColl = MonthlyRenewSales.GetForFilter(month, year);

            foreach (MonthlyRenewSales monthlyRenewSales in monthlyRenewSalesColl)
            {
                monthlySalesVMs.Add(new MonthlySalesVM(monthlyRenewSales));
            }

            return Json(monthlySalesVMs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DownloadData(int month, int year)
        {
            if (AdminIsValidated())
            {
                try
                {

                    SpreadsheetWriter spreadsheetWriter = new SpreadsheetWriter();

                    DataTable newSalesTable = MonthlyNewSales.GetForFiltertData(month, year);

                    DataTable newSalesClonedTable = newSalesTable.Clone();
                    newSalesClonedTable.Columns["HasFrame"].DataType = typeof(string);
                    newSalesClonedTable.Columns["HasEMS"].DataType = typeof(string);
                    newSalesClonedTable.Columns["HasPDR"].DataType = typeof(string);
                    newSalesClonedTable.Columns["HasMultiUser"].DataType = typeof(string);
                    newSalesClonedTable.Columns["HasQBExport"].DataType = typeof(string);
                    newSalesClonedTable.Columns["HasProAdvisor"].DataType = typeof(string);
                    newSalesClonedTable.Columns["HasImageEditor"].DataType = typeof(string);
                    newSalesClonedTable.Columns["HasBundle"].DataType = typeof(string);
                    newSalesClonedTable.Columns["HasReporting"].DataType = typeof(string);
                    newSalesClonedTable.Columns["PaymentExtension"].DataType = typeof(string);

                    foreach (DataRow eachDataRow in newSalesTable.Rows)
                    {
                        DataRow cloneDataRow = newSalesClonedTable.LoadDataRow(eachDataRow.ItemArray, true);

                        cloneDataRow.SetField("HasFrame", (InputHelper.GetBoolean(cloneDataRow["HasFrame"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasEMS", (InputHelper.GetBoolean(cloneDataRow["HasEMS"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasPDR", (InputHelper.GetBoolean(cloneDataRow["HasPDR"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasMultiUser", (InputHelper.GetBoolean(cloneDataRow["HasMultiUser"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasQBExport", (InputHelper.GetBoolean(cloneDataRow["HasQBExport"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasProAdvisor", (InputHelper.GetBoolean(cloneDataRow["HasProAdvisor"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasImageEditor", (InputHelper.GetBoolean(cloneDataRow["HasImageEditor"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasBundle", (InputHelper.GetBoolean(cloneDataRow["HasBundle"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasReporting", (InputHelper.GetBoolean(cloneDataRow["HasReporting"].ToString()) == true) ? "Y" : "N");

                        // submitted for payroll
                        if (Convert.ToString(cloneDataRow["PaymentExtension"]) == "True")
                            cloneDataRow.SetField("PaymentExtension", "Y");
                        else if ((Convert.ToString(cloneDataRow["PaymentExtension"]) == "False") ||
                                                             (cloneDataRow["PaymentExtension"] == DBNull.Value))
                            cloneDataRow.SetField("PaymentExtension", "N");
                    }

                    // remove unnecessary columns
                    newSalesClonedTable.Columns.Remove("ContractID");
                    newSalesClonedTable.Columns.Remove("State");
                    newSalesClonedTable.Columns.Remove("SalesRepID");


                    // renew
                    DataTable renewSalesTable = null;
                    DataTable renewSalesClonedTable = null;

                    renewSalesTable = MonthlyRenewSales.GetForFiltertData(month, year);

                    renewSalesClonedTable = renewSalesTable.Clone();
                    renewSalesClonedTable.Columns["HasFrame"].DataType = typeof(string);
                    renewSalesClonedTable.Columns["HasEMS"].DataType = typeof(string);
                    renewSalesClonedTable.Columns["HasPDR"].DataType = typeof(string);
                    renewSalesClonedTable.Columns["HasMultiUser"].DataType = typeof(string);
                    renewSalesClonedTable.Columns["HasQBExport"].DataType = typeof(string);
                    renewSalesClonedTable.Columns["HasProAdvisor"].DataType = typeof(string);
                    renewSalesClonedTable.Columns["HasImageEditor"].DataType = typeof(string);
                    renewSalesClonedTable.Columns["HasBundle"].DataType = typeof(string);
                    renewSalesClonedTable.Columns["HasReporting"].DataType = typeof(string);
                    renewSalesClonedTable.Columns["PaymentExtension"].DataType = typeof(string);

                    foreach (DataRow eachDataRow in renewSalesTable.Rows)
                    {
                        DataRow cloneDataRow = renewSalesClonedTable.LoadDataRow(eachDataRow.ItemArray, true);

                        cloneDataRow.SetField("HasFrame", (InputHelper.GetBoolean(cloneDataRow["HasFrame"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasEMS", (InputHelper.GetBoolean(cloneDataRow["HasEMS"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasPDR", (InputHelper.GetBoolean(cloneDataRow["HasPDR"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasMultiUser", (InputHelper.GetBoolean(cloneDataRow["HasMultiUser"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasQBExport", (InputHelper.GetBoolean(cloneDataRow["HasQBExport"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasProAdvisor", (InputHelper.GetBoolean(cloneDataRow["HasProAdvisor"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasImageEditor", (InputHelper.GetBoolean(cloneDataRow["HasImageEditor"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasBundle", (InputHelper.GetBoolean(cloneDataRow["HasBundle"].ToString()) == true) ? "Y" : "N");
                        cloneDataRow.SetField("HasReporting", (InputHelper.GetBoolean(cloneDataRow["HasReporting"].ToString()) == true) ? "Y" : "N");

                        // submitted for payroll
                        if (Convert.ToString(cloneDataRow["PaymentExtension"]) == "True")
                            cloneDataRow.SetField("PaymentExtension", "Y");
                        else if ((Convert.ToString(cloneDataRow["PaymentExtension"]) == "False") ||
                                                                (cloneDataRow["PaymentExtension"] == DBNull.Value))
                            cloneDataRow.SetField("PaymentExtension", "N");
                    }

                    // remove unnecessary columns
                    renewSalesClonedTable.Columns.Remove("PreviousCycleContractID");
                    renewSalesClonedTable.Columns.Remove("ContractID");
                    renewSalesClonedTable.Columns.Remove("State");
                    renewSalesClonedTable.Columns.Remove("SalesRepID");


                    string adminFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "Admin");
                    string diskPath = Path.Combine(adminFolder, "MonthlySalesDashboardReport" + "_" + month + "_" + year + ".xlsx");

                    if (!Directory.Exists(adminFolder))
                    {
                        Directory.CreateDirectory(adminFolder);
                    }

                    if (System.IO.File.Exists(diskPath))
                    {
                        System.IO.File.Delete(diskPath);
                    }

                    DataTable[] salesTableArr = new DataTable[2];

                    // newsales
                    salesTableArr[0] = newSalesClonedTable;
                    salesTableArr[0].TableName = "New Sales";

                    // renew sales
                    salesTableArr[1] = renewSalesClonedTable;
                    salesTableArr[1].TableName = "Renew Sales";

                    spreadsheetWriter.WriteSpreadshet(salesTableArr, diskPath);

                    byte[] fileBytes = System.IO.File.ReadAllBytes(diskPath);
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "MonthlySalesDashboardReport" + "_" + month + "_" + year + ".xlsx");
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, 0, "MonthlySalesDashboardReport DownloadData");
                    return Content(ex.Message);
                }
            }

            return Content("Error generating data.");
        }

    }
}