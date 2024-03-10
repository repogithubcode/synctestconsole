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

namespace ProEstimator.Admin.Controllers
{
    public class RenewalReportController : AdminController
    {
        [HttpGet]
        [Route("Reports/Renewal")]
        public ActionResult Index()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/Reports/Renewal";
                return Redirect("/LogOut");
            }
            else
            {
                RenewalReportPageVM vm = new RenewalReportPageVM();

                vm.GoodData = true;               

                SalesRep salesRep = SalesRep.Get(GetSalesRepID());
                if (salesRep != null)
                {
                    vm.SessionSalesRepID = salesRep.SalesRepID;
                    vm.CanSelectSalesRep = salesRep.SalesRepID == 0;
                    vm.SalesRepName = salesRep.FirstName + " " + salesRep.LastName;
                    vm.IsAdmin = ProEstimator.Business.Logic.Admin.SalesRepPermissionManager.HasPermission(salesRep.SalesRepID, "EditBonusGoals");
                }

                return View(vm);
            }
        }

        public JsonResult GetRenewalGoal(int salesRepID, int month, int year)
        {
            RenewalGoal renewalGoal = null;

            for (int loopYear = year; loopYear >= 2017; loopYear--)
            {
                for (int loopMonth = month; loopMonth >= 1; loopMonth--)
                {
                    renewalGoal = RenewalGoal.Get(salesRepID, loopMonth, loopYear);

                    if (renewalGoal != null)
                    {
                        break;
                    }
                }

                if (renewalGoal != null)
                {
                    break;
                }
            }

            if (renewalGoal == null)
            {
                RenewalGoal newCopy = new RenewalGoal();
                newCopy.BonusMonth = month;
                newCopy.BonusYear = year;
                newCopy.Forecast = 1;
                newCopy.RenewalBonus120 = 1;
                newCopy.RenewalBonus130 = 1;
                newCopy.RenewalBonus1Yr100 = 1;
                newCopy.RenewalBonus1Yr110 = 1;
                newCopy.RenewalGoal1Yr = 1;
                newCopy.RenewalGoal2Yr = 1;
                newCopy.SalesBonus100 = 1;
                newCopy.SalesBonus110 = 1;
                newCopy.SalesBonus120 = 1;
                newCopy.SalesBonus130 = 1;
                newCopy.SalesGoal = 1;
                newCopy.SalesRepId = salesRepID;
                newCopy.CreateDate = DateTime.Now;
                newCopy.ModifiedDate = DateTime.Now;

                newCopy.Save(ActiveLogin.ID);
                renewalGoal = newCopy;
            }
            else if (renewalGoal.BonusYear != year || renewalGoal.BonusMonth != month)
            {
                RenewalGoal newCopy = new RenewalGoal();
                newCopy.ActualSales = renewalGoal.ActualSales;
                newCopy.BonusMonth = month;
                newCopy.BonusYear = year;
                newCopy.Forecast = renewalGoal.Forecast;
                newCopy.RenewalBonus120 = renewalGoal.RenewalBonus120;
                newCopy.RenewalBonus130 = renewalGoal.RenewalBonus130;
                newCopy.RenewalBonus1Yr100 = renewalGoal.RenewalBonus1Yr100;
                newCopy.RenewalBonus1Yr110 = renewalGoal.RenewalBonus1Yr110;
                newCopy.RenewalGoal1Yr = renewalGoal.RenewalGoal1Yr;
                newCopy.RenewalGoal2Yr = renewalGoal.RenewalGoal2Yr;
                newCopy.SalesBonus100 = renewalGoal.SalesBonus100;
                newCopy.SalesBonus110 = renewalGoal.SalesBonus110;
                newCopy.SalesBonus120 = renewalGoal.SalesBonus120;
                newCopy.SalesBonus130 = renewalGoal.SalesBonus130;
                newCopy.SalesGoal = renewalGoal.SalesGoal;
                newCopy.SalesRepId = renewalGoal.SalesRepId;
                newCopy.CreateDate = DateTime.Now;
                newCopy.ModifiedDate = DateTime.Now;

                newCopy.Save(ActiveLogin.ID);
                renewalGoal = newCopy;
            }

            List<RenewalDetails> details = RenewalDetails.GetRenewalReport(year, month, salesRepID);

            RenewalGoalVM renewalGoalVM = new RenewalGoalVM(renewalGoal, details);

            return Json(renewalGoalVM, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveSaleBonuses(
              int salesRepID
            , int month
            , int year
            , string renewalGoal1Yr
            , string renewalGoal2Yr
            , string salesGoal
            , string salesBonus100
            , string salesBonus110
            , string salesBonus120
            , string salesBonus130
            , string renewalBonus1Yr100
            , string renewalBonus1Yr110
            , string renewalBonus120
            , string renewalBonus130
        )
        {
            try
            {
                RenewalGoal renewalGoal = RenewalGoal.Get(salesRepID, month, year);

                if (renewalGoal == null)
                {
                    renewalGoal = new RenewalGoal();
                    renewalGoal.CreateDate = DateTime.Now;
                }

                renewalGoal.SalesRepId = salesRepID;
                renewalGoal.BonusMonth = month;
                renewalGoal.BonusYear = year;
                renewalGoal.RenewalGoal1Yr = InputHelper.GetInteger(renewalGoal1Yr);
                renewalGoal.RenewalGoal2Yr = InputHelper.GetInteger(renewalGoal2Yr);
                renewalGoal.SalesGoal = InputHelper.GetInteger(salesGoal);
                renewalGoal.SalesBonus100 = InputHelper.GetInteger(salesBonus100);
                renewalGoal.SalesBonus110 = InputHelper.GetInteger(salesBonus110);
                renewalGoal.SalesBonus120 = InputHelper.GetInteger(salesBonus120);
                renewalGoal.SalesBonus130 = InputHelper.GetInteger(salesBonus130);
                renewalGoal.RenewalBonus1Yr100 = InputHelper.GetInteger(renewalBonus1Yr100);
                renewalGoal.RenewalBonus1Yr110 = InputHelper.GetInteger(renewalBonus1Yr110);
                renewalGoal.RenewalBonus120 = InputHelper.GetInteger(renewalBonus120);
                renewalGoal.RenewalBonus130 = InputHelper.GetInteger(renewalBonus130);
                renewalGoal.ModifiedDate = DateTime.Now;

                SaveResult saveResult = renewalGoal.Save(ActiveLogin.ID);
                return Json(saveResult.ErrorMessage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "RenewalReport SaveSaleBonuses");
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SaveForcast(
              int salesRepID
            , int month
            , int year
            , string forecast
        )
        {
            try
            {
                RenewalGoal renewalGoal = RenewalGoal.Get(salesRepID, month, year);

                if (renewalGoal == null)
                {
                    renewalGoal = new RenewalGoal();
                    renewalGoal.CreateDate = DateTime.Now;
                }

                renewalGoal.SalesRepId = salesRepID;
                renewalGoal.ModifiedDate = DateTime.Now;
                renewalGoal.Forecast = InputHelper.GetInteger(forecast);

                SaveResult saveResult = renewalGoal.Save(ActiveLogin.ID);
                return Json(saveResult.ErrorMessage, JsonRequestBehavior.AllowGet); 
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "RenewalReport SaveForcast");
                return Json(ex.Message, JsonRequestBehavior.AllowGet); 
            }
        }

        public JsonResult UpdateWillRenew(int loginID, int contractID, bool willRenew, string source)
        {
            try
            {
                if (source == "WE")
                {
                    Contract.SaveWEContractWillRenew(contractID, willRenew);
                }
                else
                {
                    Contract contract = Contract.Get(contractID);
                    if (contract != null && contract.LoginID == loginID)
                    {
                        contract.WillRenew = willRenew;
                        contract.Save(ActiveLogin.ID);
                    }
                    else
                    {
                        ErrorLogger.LogError("Invalid contract (" + contractID + ") or Login(" + loginID + ")", loginID, 0, "RenewalReport UpdateWillRenew");
                        return Json("Invalid contract or contract does not belong to login.", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "RenewalReport UpdateWillRenew");
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateWillNotRenew(int loginID, int contractID, bool willNotRenew)
        {
            try
            {
                Contract contract = Contract.Get(contractID);
                if (contract != null && contract.LoginID == loginID)
                {
                    contract.WillNotRenew = willNotRenew;
                    contract.Save(ActiveLogin.ID);
                }
                else
                {
                    ErrorLogger.LogError("Invalid contract (" + contractID + ") or Login(" + loginID + ")", loginID, 0, "RenewalReport UpdateWillNotRenew");
                    return Json("Invalid contract or contract does not belong to login.", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "RenewalReport UpdateWillNotRenew");
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveNotes(int loginID, int contractID, string notes)
        {
            try
            {
                Contract contract = Contract.Get(contractID);
                if (contract != null && contract.LoginID == loginID)
                {
                    contract.Notes = notes;
                    contract.Save(ActiveLogin.ID);
                }
                else
                {
                    List<System.Data.SqlClient.SqlParameter> parameters = new List<System.Data.SqlClient.SqlParameter>();
                    parameters.Add(new System.Data.SqlClient.SqlParameter("ContractID", contractID));
                    parameters.Add(new System.Data.SqlClient.SqlParameter("Note", notes));

                    DBAccess db = new DBAccess();
                    db.ExecuteNonQuery("SaveWEContractNote", parameters);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "RenewalReport SaveNotes");
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DownloadData(int salesRepID, int month, int year, string search)
        {
            if (AdminIsValidated())
            {
                try
                {
                    search = search.ToLower();

                    SpreadsheetWriter spreadsheetWriter = new SpreadsheetWriter();
                    DataTable table = RenewalDetails.GetRenewalReportData(year, month, salesRepID);

                    DataTable filteredTable = table.Clone();

                    foreach (DataRow row in table.Rows)
                    {
                        string salesRep = InputHelper.GetString(row["SalesRep"].ToString()).ToLower();
                        string companyName = InputHelper.GetString(row["CompanyName"].ToString()).ToLower();
                        string contact = InputHelper.GetString(row["Contact"].ToString()).ToLower();
                        string phoneNumber = InputHelper.GetString(row["PhoneNumber"].ToString()).ToLower();
                        string notes = InputHelper.GetString(row["Notes"].ToString()).ToLower();

                        if (
                               salesRep.Contains(search)
                            || companyName.Contains(search)
                            || contact.Contains(search)
                            || phoneNumber.Contains(search)
                            || notes.Contains(search)
                        )
                        {
                            filteredTable.Rows.Add(row.ItemArray);
                        }
                    }

                    string adminFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "Admin");
                    string diskPath = Path.Combine(adminFolder, "RenewalReport_" + salesRepID + "_" + month + "_" + year + ".xlsx");

                    if (!Directory.Exists(adminFolder))
                    {
                        Directory.CreateDirectory(adminFolder);
                    }

                    if (System.IO.File.Exists(diskPath))
                    {
                        System.IO.File.Delete(diskPath);
                    }

                    spreadsheetWriter.WriteSpreadshet(filteredTable, diskPath);

                    byte[] fileBytes = System.IO.File.ReadAllBytes(diskPath);
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "RenewalReport_" + salesRepID + "_" + month + "_" + year + ".xlsx");
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, 0, "RenewalReport DownloadData");
                    return Content(ex.Message);
                }
            }

            return Content("Error generating data.");
        }

        public JsonResult GetGridShowHideColumnInfo(int loginID, string gridControlID)
        {
            List<GridMappingVM> gridMappingViewModelColl = GetGridShowHideColumnInfoList(loginID, gridControlID);
            gridMappingViewModelColl = gridMappingViewModelColl.OrderBy(o => o.SortOrderIndex).ToList();

            return Json(new { Success = true, ResultObject = gridMappingViewModelColl }, JsonRequestBehavior.AllowGet);
        }

        private List<GridMappingVM> GetGridShowHideColumnInfoList(int loginID, string gridControlID)
        {
            List<GridMappingVM> results = new List<GridMappingVM>();

            List<GridColumnInfoUserMapping> mappings = GridColumnInfoUserMapping.GetForUserControlID(loginID, gridControlID);

            foreach (GridColumnInfoUserMapping mapping in mappings)
            {
                results.Add(new GridMappingVM(mapping));
            }

            return results;
        }

        public JsonResult SaveGridShowHideColumnInfo(int loginID, string gridControlID, List<GridMappingVM> gridMappingVMList)
        {
            List<GridColumnInfoUserMapping> mappings = GridColumnInfoUserMapping.GetForUserControlID(loginID, gridControlID);

            foreach (GridColumnInfoUserMapping mapping in mappings)
            {
                GridMappingVM vm = gridMappingVMList.FirstOrDefault(o => o.GridColumnID == mapping.GridColumnInfo.ID);
                if (vm != null)
                {
                    mapping.Visible = vm.Visible;
                    mapping.Save(ActiveLogin.ID);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveGridSortingColumn(int loginID, string gridControlID, string columnTitle, int newIndex)
        {
            List<GridColumnInfoUserMapping> mappings = GridColumnInfoUserMapping.GetForUserControlID(loginID, gridControlID).OrderBy(o => o.SortOrderIndex).ToList();
            GridColumnInfoUserMapping mappingToMove = mappings.FirstOrDefault(o => o.GridColumnInfo.Name == columnTitle);

            if (mappingToMove != null)
            {
                // Move the mapping in the list
                if (newIndex >= mappings.Count)
                {
                    newIndex = mappings.Count - 1;
                }

                mappings.Remove(mappingToMove);
                mappings.Insert(newIndex, mappingToMove);

                // Save the whole mapping list with updated sort indexes.
                int index = 0;
                foreach (GridColumnInfoUserMapping mapping in mappings)
                {
                    mapping.SortOrderIndex = index++;
                    mapping.Save(ActiveLogin.ID);
                }
            }

            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}