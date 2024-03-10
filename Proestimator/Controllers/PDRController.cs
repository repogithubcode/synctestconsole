using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

using Proestimator.Helpers;
using Proestimator.Resources;
using ProEstimator.Business.Logic;

using Proestimator.ViewModel.PDR;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.PDR;
using Kendo.Mvc.UI;
using Proestimator.ViewModel;
using ProEstimatorData.DataModel.ProAdvisor;
using Kendo.Mvc.Extensions;
using ProEstimatorData.DataModel.Profiles;
using System.Web.Profile;
using Proestimator.ViewModelMappers.PDR;

namespace Proestimator.Controllers
{
    public class PDRController : SiteController
    {

        public ActionResult UndoPDR(PDRMatrix model)
        {
            return RedirectToAction("AddPartsGraphically", "Estimate");
        }

        private int GetLastLineNumber(int estimateID)
        {
            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Estimate_GetLastLineNumber", new SqlParameter("AdminInfoID", estimateID));
            return result.Value;
        }

        private int SavePDRLine(int userID, PDRLine pdrLine, Estimate estimate, List<PDR_EstimateDataPanel> panels, List<PDR_EstimateDataPanelSupplementChange> supplementChanges, int lineNumber, StringBuilder errorBuilder)
        {
            try
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                PDR_EstimateDataPanel panel = panels.FirstOrDefault(o => o.ID == pdrLine.ID);
                if (panel != null)
                {
                    // Get the selected quantity and size vaulues
                    PDR_Quantity quantity = null;
                    PDR_Size size = PDR_Size.None;

                    if (pdrLine.QuantityID > 0)
                    {
                        quantity = PDR_Quantity.GetByID(pdrLine.QuantityID);
                    }
                    if (pdrLine.SizeID > 0)
                    {
                        size = (PDR_Size)pdrLine.SizeID;
                    }

                    if (!string.IsNullOrEmpty(pdrLine.SelectedID))
                    {
                        string[] pieces = pdrLine.SelectedID.Split('-');
                        int quantityID = InputHelper.GetInteger(pieces[1]);

                        quantity = PDR_Quantity.GetByID(quantityID);
                        size = (PDR_Size)Enum.Parse(typeof(PDR_Size), pieces[2]);
                    }

                    // If the supplement change is in the current supplement we might just want to delete it
                    PDR_EstimateDataPanelSupplementChange supplementChange = supplementChanges.FirstOrDefault(o => o.EstimateDataPanelID == panel.ID && o.SupplementVersion == estimate.LockLevel);
                    if (supplementChange != null && supplementChange.SupplementVersion == estimate.LockLevel)
                    {
                        List<PDR_EstimateDataPanelOversize> oversizedDents = PDR_EstimateDataPanelOversize.GetForDataPanelID(panel.ID).Where(o => o.SupplementAdded == estimate.LockLevel || o.SupplementDeleted == estimate.LockLevel).ToList();

                        if (InputHelper.GetDouble(pdrLine.CustomCharge) == 0 && pdrLine.SelectedOversizedDentQuantity == 0 && quantity == null && size == PDR_Size.None && oversizedDents.Count == 0)
                        {
                            if (supplementChange.CustomCharge != 0 || supplementChange.OversizedDents != 0 || supplementChange.Quantity != null || supplementChange.Size != PDR_Size.None)
                            {
                                supplementChange.Delete();
                                return lineNumber;
                            }
                        }                  
                    }

                    // If the estimate is not in a supplement, copy the data into the base panel record
                    if (estimate.LockLevel == 0)
                    {
                        panel.Quantity = quantity;
                        panel.Size = size;

                        // Oversized and multipliers are saved by the popup on mobile, don't overwrite when saving here
                        if (pdrLine.SaveDetails)
                        {
                            panel.OversizedDents = pdrLine.SelectedOversizedDentQuantity;
                            panel.Multiplier = pdrLine.SelectedMultiplier;
                            panel.CustomCharge = InputHelper.GetDouble(pdrLine.CustomCharge);
                        }

                        List<PDR_EstimateDataPanelOversize> oversizedDents = PDR_EstimateDataPanelOversize.GetForDataPanelID(panel.ID);

                        if ((quantity != null && quantity.ID > 0) || size != PDR_Size.None || panel.OversizedDents > 0 || panel.Multiplier > 0 || panel.CustomCharge != 0 || oversizedDents.Count > 0)
                        {
                            if (panel.LineNumber == 0)
                            {
                                panel.LineNumber = lineNumber;
                                lineNumber++;
                            }
                        }
                        else
                        {
                            panel.LineNumber = 0;
                        }
                    }

                    // If the estimate is in a supplement, see if the current data is diffent than the panel.  If it is either update or add a supplement records for the panel
                    else
                    {
                        // Get the previous data, this is either the base PDR_EstimateDataPanel data or if the estimate is > supplement 1 this is from the last supplement
                        PDR_Quantity lastQuantity = panel.Quantity;
                        PDR_Size lastSize = panel.Size;
                        int lastDentQuantity = panel.OversizedDents;
                        int lastMultiplier = panel.Multiplier;
                        int lastLineNumber = panel.LineNumber;
                        double lastCustomCharge = panel.CustomCharge;

                        if (estimate.LockLevel > 1)
                        {
                            // Look backwards through supplements until we find data for this panel
                            for (int supplementVersion = estimate.LockLevel - 1; supplementVersion > 0; supplementVersion--)
                            {
                                PDR_EstimateDataPanelSupplementChange lastSuppData = supplementChanges.FirstOrDefault(o => o.EstimateDataPanelID == panel.ID && o.SupplementVersion == supplementVersion);
                                if (lastSuppData != null)
                                {
                                    lastQuantity = lastSuppData.Quantity;
                                    lastSize = lastSuppData.Size;
                                    lastDentQuantity = lastSuppData.OversizedDents;
                                    lastMultiplier = lastSuppData.Multiplier;
                                    lastLineNumber = lastSuppData.LineNumber;

                                    break;
                                }
                            }
                        }

                        List<PDR_EstimateDataPanelOversize> oversizedDents = PDR_EstimateDataPanelOversize.GetForDataPanelID(panel.ID).Where(o => o.SupplementAdded == estimate.LockLevel || o.SupplementDeleted == estimate.LockLevel).ToList();

                        if (lastQuantity != quantity || lastSize != size || lastDentQuantity != pdrLine.SelectedOversizedDentQuantity || lastMultiplier != pdrLine.SelectedMultiplier || lastCustomCharge != InputHelper.GetDouble(pdrLine.CustomCharge) || oversizedDents.Count > 0)
                        {
                            if (supplementChange == null)
                            {
                                supplementChange = new PDR_EstimateDataPanelSupplementChange();
                                supplementChange.EstimateDataPanelID = panel.ID;
                                supplementChange.SupplementVersion = estimate.LockLevel;

                                //bool lineIsDeleted = quantity == null && size == PDR_Size.None && pdrLine.SelectedOversizedDentQuantity == 0 && pdrLine.SelectedMultiplier == 0 && InputHelper.GetDouble(pdrLine.CustomCharge) == 0 && oversizedDents.Count == 0;
                                //if (panel.LineNumber > 0 && !lineIsDeleted)
                                //{
                                //    supplementChange.LineNumber = panel.LineNumber;
                                //}
                                //else
                                //{
                                supplementChange.LineNumber = lineNumber;
                                lineNumber++;
                                //}
                            }

                            supplementChange.Quantity = quantity;
                            supplementChange.Size = size;

                            // Oversized and multipliers are saved by the popup on mobile, don't overwrite when saving here
                            if (pdrLine.SaveDetails)
                            {
                                supplementChange.OversizedDents = pdrLine.SelectedOversizedDentQuantity;
                                supplementChange.Multiplier = pdrLine.SelectedMultiplier;
                                supplementChange.CustomCharge = InputHelper.GetDouble(pdrLine.CustomCharge);
                            }

                            supplementChange.Save(activeLogin.ID, activeLogin.LoginID);
                        }
                    }

                    if (pdrLine.SaveDetails)
                    {
                        panel.Description = pdrLine.Description;
                        panel.Expanded = pdrLine.IsExpanded;
                    }

                    
                    panel.Save(activeLogin.ID);
                }
            }
            catch (Exception ex)
            {
                errorBuilder.AppendLine(ex.Message + Environment.NewLine + ex.StackTrace);
            }

            return lineNumber;
        }

        public JsonResult SavePDRList(int userID, int loginID, int estimateID, List<PDRLine> data)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                StringBuilder errorBuilder = new StringBuilder();

                if (data == null)
                {
                    errorBuilder.AppendLine("No data passed to Save PDR List");
                }
                else
                {
                    try
                    {
                        Estimate estimate = new Estimate(estimateID);
                        List<PDR_EstimateDataPanel> panels = PDR_EstimateDataPanel.GetForEstimate(estimateID);
                        List<PDR_EstimateDataPanelSupplementChange> supplementChanges = PDR_EstimateDataPanelSupplementChange.GetForEstimate(estimateID);

                        int lineNumber = GetLastLineNumber(estimateID) + 1;

                        foreach (PDRLine pdrLine in data)
                        {
                            lineNumber = SavePDRLine(userID, pdrLine, estimate, panels, supplementChanges, lineNumber, errorBuilder);
                        }

                        DBAccess db = new DBAccess();
                        DBAccessTableResult table = db.ExecuteWithTable("RedoLineNumbers", new SqlParameter("AdminInfoID", estimate.EstimateID));

                        Estimate.RefreshProcessedLines(estimateID);
                    }
                    catch (Exception ex)
                    {
                        errorBuilder.AppendLine(ex.Message + Environment.NewLine + ex.StackTrace);
                    }
                }

                string error = errorBuilder.ToString();
                if (string.IsNullOrEmpty(error))
                {
                    return Json(Proestimator.Resources.PDRStrings.PDR_DataSaved, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ErrorLogger.LogError(error, loginID, estimateID, "PDR Save");
                    return Json(error, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult SavePDRSingleLine(int userID, int loginID, int estimateID, PDRLine data)
        {
            FunctionResult functionResult = new FunctionResult();

            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                StringBuilder errorBuilder = new StringBuilder();

                try
                {
                    Estimate estimate = new Estimate(estimateID);
                    List<PDR_EstimateDataPanel> panels = PDR_EstimateDataPanel.GetForEstimate(estimateID);
                    List<PDR_EstimateDataPanelSupplementChange> supplementChanges = PDR_EstimateDataPanelSupplementChange.GetForEstimate(estimateID);

                    bool hasVehicle = false;
                    Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimateID);
                    if (vehicle?.VehicleID > 0)
                    {
                        hasVehicle = true;
                    }

                    int lineNumber = GetLastLineNumber(estimateID) + 1;

                    SavePDRLine(userID, data, estimate, panels, supplementChanges, lineNumber, errorBuilder);

                    DBAccess db = new DBAccess();
                    DBAccessTableResult table = db.ExecuteWithTable("RedoLineNumbers", new SqlParameter("AdminInfoID", estimate.EstimateID));

                    // Reload data and put together line summary to return
                    panels = PDR_EstimateDataPanel.GetForEstimate(estimateID);
                    supplementChanges = PDR_EstimateDataPanelSupplementChange.GetForEstimate(estimateID);

                    PDR_EstimateData pdrEstimateData = PDR_EstimateData.GetForEstimate(estimateID);
                    List<PDR_Rate> allRates = PDR_Rate.GetByProfile(pdrEstimateData.RateProfileID);
                    PDR_EstimateDataPanel dataPanel = panels.FirstOrDefault(o => o.ID == data.ID);
                    PDR_EstimateDataPanelSupplementChange suppChange = supplementChanges.FirstOrDefault(o => o.EstimateDataPanelID == data.ID);

                    PanelVMMapper mapper = new PanelVMMapper();
                    PanelVM panelVM = mapper.Map(new PanelVMMapperConfiguration() { Panel = dataPanel, SupplementChange = suppChange, AllRates = allRates, HasVehicle = hasVehicle });

                    functionResult.Success = true;
                    functionResult.ErrorMessage = panelVM.LineSummary;

                    Estimate.RefreshProcessedLines(estimateID);
                }
                catch (Exception ex)
                {
                    errorBuilder.AppendLine(ex.Message + Environment.NewLine + ex.StackTrace);
                }

                string error = errorBuilder.ToString();
                if (!string.IsNullOrEmpty(error))
                {
                    functionResult.Success = false;
                    functionResult.ErrorMessage = error;
                    ErrorLogger.LogError(error, loginID, estimateID, "PDR Save");
                }
            }

            return Json(functionResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPDRDetails(int userID, int estimateDataPanelID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                PDR_EstimateDataPanelSupplementChange supplementData = null;
                PDR_EstimateDataPanel dataPanel = PDR_EstimateDataPanel.GetByID(estimateDataPanelID);

                List<PDR_EstimateDataPanelSupplementChange> allSupData = PDR_EstimateDataPanelSupplementChange.GetForEstimateDataPanel(dataPanel.ID);
                if (allSupData != null && allSupData.Count > 0)
                {
                    supplementData = allSupData.OrderByDescending(o => o.SupplementVersion).Last();
                }

                PDRLineDetails details = new PDRLineDetails();
                details.Panel = dataPanel.Panel.PanelName;
                details.PanelID = dataPanel.Panel.ID;
                details.OversizedDentCount = dataPanel.OversizedDents;
                details.MultiplierID = dataPanel.Multiplier;
                details.Description = dataPanel.Description;
                details.CustomCharge = dataPanel.CustomCharge;
                details.QuantityID = dataPanel.Quantity == null ? 0 : dataPanel.Quantity.ID;
                details.SizeID = (int)dataPanel.Size;

                if (supplementData != null)
                {
                    if (supplementData.Quantity != null)
                    {
                        details.QuantityID = supplementData.Quantity.ID;
                    }
               
                    details.SizeID = (int)supplementData.Size;
                    details.OversizedDentCount = supplementData.OversizedDents;
                    details.MultiplierID = supplementData.Multiplier;
                    details.CustomCharge = supplementData.CustomCharge;
                }

                List<PDR_EstimateDataPanelOversize> oversized = PDR_EstimateDataPanelOversize.GetForDataPanelID(estimateDataPanelID);
                foreach (PDR_EstimateDataPanelOversize oversize in oversized)
                {
                    OversizedDentVM dentVM = new OversizedDentVM(oversize);

                    AddOversizedResult oversizeDetails = new AddOversizedResult();
                    oversizeDetails.Success = true;
                    oversizeDetails.ID = oversize.ID;
                    oversizeDetails.Description = dentVM.Display;
                    oversizeDetails.Amount = dentVM.Amount;
                    oversizeDetails.PanelID = dataPanel.Panel.ID;

                    details.OversizedDents.Add(oversizeDetails);
                }

                // Set up the size rates for every quantity
                details.PanelRates = new List<PDRPanelRates>();

                PDR_EstimateData pdrData = PDR_EstimateData.GetForEstimate(dataPanel.AdminInfoID);
                List<PDR_Rate> allRates = PDR_Rate.GetByProfile(pdrData.RateProfileID).Where(o => o.Panel.ID == dataPanel.Panel.ID).ToList();
                List<PDR_Quantity> allQuantities = PDR_Quantity.GetAll();

                foreach (PDR_Quantity quantity in allQuantities)
                {
                    details.PanelRates.Add(new PDRPanelRates(quantity.ID, allRates.Where(o => o.Quantity != null && o.Quantity.ID == quantity.ID).ToList()));
                }

                return Json(details, JsonRequestBehavior.AllowGet);
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        public class PDRLineDetails
        {
            public string Panel { get; set; }
            public int PanelID { get; set; }

            public int OversizedDentCount { get; set; }
            public int MultiplierID { get; set; }
            public string Description { get; set; }
            public double CustomCharge { get; set; }
            public int QuantityID { get; set; }
            public int SizeID { get; set; }
            public bool ShowRAndID { get; set; }

            public List<AddOversizedResult> OversizedDents { get; set; }

            /// <summary>
            /// Only used for the mobile PRD panel popup.
            /// </summary>
            public List<PDRPanelRates> PanelRates { get; set; }

            public PDRLineDetails()
            {
                Panel = "";

                OversizedDentCount = 0;
                MultiplierID = 0;
                CustomCharge = 0;

                OversizedDents = new List<AddOversizedResult>();
            }
        }

        public class PDRPanelRates
        {
            public int QuantityID { get; set; }
            public double DimeRate { get; set; }
            public double NickelRate { get; set; }
            public double QuarterRate { get; set; }
            public double HalfDollarRate { get; set; }

            public PDRPanelRates()
            {

            }

            public PDRPanelRates(int quantityID, List<PDR_Rate> rates)
            {
                QuantityID = quantityID;

                if (rates != null)
                {
                    DimeRate = GetRate(rates.Where(o => o.Size == PDR_Size.Dime).ToList(), PDR_Size.Dime);
                    NickelRate = GetRate(rates.Where(o => o.Size == PDR_Size.Nickel).ToList(), PDR_Size.Nickel);
                    QuarterRate = GetRate(rates.Where(o => o.Size == PDR_Size.Quarter).ToList(), PDR_Size.Quarter);
                    HalfDollarRate = GetRate(rates.Where(o => o.Size == PDR_Size.Half).ToList(), PDR_Size.Half);
                }
                else
                {
                    DimeRate = 0;
                    NickelRate = 0;
                    QuarterRate = 0;
                    HalfDollarRate = 0;
                }
            }

            private double GetRate(List<PDR_Rate> rates, PDR_Size size)
            {
                PDR_Rate rate = rates.FirstOrDefault(o => o.Size == size);
                if (rate != null)
                {
                    return (double)rate.Amount;
                }

                return 0;
            }
        }

        public JsonResult AddOversized(int userID, int estimateID, int panelID, string size, string depth)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                AddOversizedResult result = new AddOversizedResult();

                try
                {
                    PDR_EstimateDataPanel dataPanel = PDR_EstimateDataPanel.GetByID(panelID);
                    if (dataPanel.AdminInfoID == estimateID)
                    {
                        ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                        PDR_Size pdrSize = (PDR_Size)Enum.Parse(typeof(PDR_Size), size);
                        PDR_Depth pdrDepth = (PDR_Depth)Enum.Parse(typeof(PDR_Depth), depth);

                        Estimate estimate = new Estimate(estimateID);
                        PDR_EstimateData estimateData = PDR_EstimateData.GetForEstimate(estimateID);
                        PDR_RateProfile rateProfile = PDR_RateProfile.GetByID(estimateData.RateProfileID);
                        PDR_Rate rate = rateProfile.GetAllRates().FirstOrDefault(o => o.Panel.ID == 1 && o.Size == pdrSize && o.Depth == pdrDepth);

                        PDR_EstimateDataPanelOversize oversize = new PDR_EstimateDataPanelOversize();
                        oversize.EstimateDataPanel = panelID;
                        oversize.Size = pdrSize;
                        oversize.Depth = pdrDepth;
                        oversize.Amount = rate.Amount;
                        oversize.SupplementAdded = estimate.LockLevel;
                        SaveResult saveResult = oversize.Save(activeLogin.ID, activeLogin.LoginID);

                        if (saveResult.Success)
                        {
                            OversizedDentVM dentVM = new OversizedDentVM(oversize);

                            result.Success = true;
                            result.ID = oversize.ID;
                            result.Description = dentVM.Display;
                            result.Amount = dentVM.Amount;
                            result.PanelID = dataPanel.Panel.ID;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteOversized(int userID, int loginID, int id)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                AjaxResult result = new AjaxResult();
                result.Success = false;
                result.ErrorMessage = @Proestimator.Resources.ProStrings.ErrorDeletingOverziedDent;

                try
                {
                    PDR_EstimateDataPanelOversize oversized = PDR_EstimateDataPanelOversize.GetByID(id);
                    PDR_EstimateDataPanel dataPanel = PDR_EstimateDataPanel.GetByID(oversized.EstimateDataPanel);
                    Estimate estimate = new Estimate(dataPanel.AdminInfoID);

                    if (estimate.CreatedByLoginID == loginID)
                    {
                        oversized.Delete();
                        result.Success = true;
                        result.ErrorMessage = "";

                        DBAccess db = new DBAccess();
                        DBAccessTableResult table = db.ExecuteWithTable("RedoLineNumbers", new SqlParameter("AdminInfoID", estimate.EstimateID));
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        public class AddOversizedResult : AjaxResult
        {
            public string Description { get; set; }
            public string Amount { get; set; }
            public int ID { get; set; }
            public int PanelID { get; set; }
        }

        public JsonResult DeletePDR(int userID, int estimateID, int ID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                Estimate estimate = new Estimate(estimateID);
                if (estimate != null && !estimate.IsLocked())
                {
                    PDR_EstimateDataPanel dataPanel = PDR_EstimateDataPanel.GetByID(ID);
                    if (dataPanel == null || dataPanel.AdminInfoID != estimateID)
                    {
                        return Json(@Proestimator.Resources.ProStrings.PanelNotFound, JsonRequestBehavior.AllowGet);
                    }

                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                    SaveResult saveResult = dataPanel.ClearData(estimate.LockLevel, GetLastLineNumber(estimateID) + 1, activeLogin.ID, activeLogin.LoginID);

                    DBAccess db = new DBAccess();
                    DBAccessTableResult table = db.ExecuteWithTable("RedoLineNumbers", new SqlParameter("AdminInfoID", estimate.EstimateID));

                    Estimate.RefreshProcessedLines(estimateID);

                    return Json(saveResult.ErrorMessage, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(@Proestimator.Resources.ProStrings.CannotEditLockedEstimate, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateSizeSelection(int userID, int estimateID, string value)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                string message = "";

                try
                {
                    string[] pieces = value.Split(':');
                    int panelID = InputHelper.GetInteger(pieces[0]);
                    int quantityID = InputHelper.GetInteger(pieces[1]);
                    string sizeCode = pieces[2];

                    PDR_EstimateDataPanel panelData = PDR_EstimateDataPanel.GetForEstimate(estimateID).FirstOrDefault(o => o.Panel.ID == panelID);
                    if (panelData != null)
                    {
                        if (panelData.Quantity != null && panelData.Quantity.ID == quantityID && panelData.Size.ToString() == sizeCode)
                        {
                            panelData.Quantity = null;
                        }
                        else
                        {
                            panelData.Quantity = PDR_Quantity.GetByID(quantityID);
                            panelData.Size = (PDR_Size)Enum.Parse(typeof(PDR_Size), sizeCode);
                        }

                        ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                        SaveResult saveResult = panelData.Save(activeLogin.ID);
                        if (!saveResult.Success)
                        {
                            message = saveResult.ErrorMessage;
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }

                return Json(message, JsonRequestBehavior.AllowGet);
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/pdr-profile-select")]
        public ActionResult ProfileSelect(int userID, int estimateID, string errorMessage = "")
        {
            PDR_Manager manager = new PDR_Manager();

            ProfileSelectVM vm = new ProfileSelectVM();
            vm.LoginID = ActiveLogin.LoginID;
            vm.EstimateID = estimateID;

            if (errorMessage != "PDR")  // "PDR" is a special case meaning to open the PDR popup after selecting the profile
            {
                vm.ErrorMessage = errorMessage;
            }

            PDR_EstimateData estimateData = PDR_EstimateData.GetForEstimate(estimateID);
            LoginInfo loginInfo = null;
            if (estimateData == null)
            {
                // This page is called when there is no PDR Estimate Data for an estimate.  This is responsible for creating the rate profile copy and PDR Estimate record

                manager.MakeSureDefaultExists(ActiveLogin.LoginID, ActiveLogin.ID);

                List<PDR_RateProfile> profiles = PDR_RateProfile.GetByLogin(ActiveLogin.LoginID).Where(o => o.AdminInfoID == 0).ToList();

                // Check if pdr rate profiles has only one record then set it as default
                loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
                if (loginInfo.UseDefaultPDRRateProfile == false)
                {
                    if (profiles != null && profiles.Count == 1)
                    {
                        if (profiles[0].IsDefault == false)
                        {
                            profiles[0].IsDefault = true; // set IsDefault flag set to 1
                            profiles[0].Save();
                        }

                        // following updates flag in logininfo table for for UseDefaultPDRRateProfile
                        loginInfo.UseDefaultPDRRateProfile = !loginInfo.UseDefaultPDRRateProfile;
                        loginInfo.Save();
                    }
                }

                // If the organization is set to use a default rate profile, make a copy for the new estimate data
                loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
                if (loginInfo.UseDefaultPDRRateProfile)
                {
                    // If there is no default PDR rate profile, redirect to the page to create one
                    if (profiles.FirstOrDefault(o => o.IsDefault) == null)
                    {
                        ErrorLogger.LogError("No default profile", ActiveLogin.LoginID, estimateID, "PDRController ProfileSelect");
                        return RedirectToAction("RateProfileList", "Home");
                    }

                    // Copy the default profile and make a new estimate.
                    PDR_RateProfileFunctionResult profileCopyResult = manager.CopyFromDefault(ActiveLogin.LoginID, ActiveLogin.ID);

                    if (profileCopyResult.Success)
                    {
                        profileCopyResult.RateProfile.AdminInfoID = estimateID;
                        profileCopyResult.RateProfile.Save();

                        return CreatePDREstimateAndRedirect(ActiveLogin.LoginID, estimateID, profileCopyResult.RateProfile.ID, ActiveLogin.ID, errorMessage == "PDR");
                    }
                    else
                    {
                        vm.ErrorMessage = profileCopyResult.ErrorMessage;
                    }
                }

                // Use default rate profile is not set.  Let the user pick one of the rate profiles.
                foreach (PDR_RateProfile profile in profiles)
                {
                    vm.Profiles.Add(new ProfileVM(profile.ProfileName, profile.ID));
                }
            }

            loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
            vm.UseDefaultPDRRateProfile = loginInfo.UseDefaultPDRRateProfile;

            ViewBag.EstimateID = estimateID;
            
            return View("ProfileSelect", vm);
        }

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/select-pdr-profile/{profileID}")]
        public ActionResult SelectRateProfile(int userID, int estimateID, int profileID)
        {
            PDR_Manager manager = new PDR_Manager();
            PDR_RateProfileFunctionResult result = manager.DuplicateRateProfile(profileID, ActiveLogin.LoginID, ActiveLogin.ID);

            if (result.Success)
            {
                result.RateProfile.AdminInfoID = estimateID;
                result.RateProfile.Save();

                return CreatePDREstimateAndRedirect(ActiveLogin.LoginID, estimateID, result.RateProfile.ID, ActiveLogin.ID, true);
            }
            else
            {
                return ProfileSelect(ActiveLogin.LoginID, estimateID, result.ErrorMessage);
            }
        }

        private ActionResult CreatePDREstimateAndRedirect(int loginID, int estimateID, int rateProfileID, int activeLoginID, bool openPDR = false)
        {
            PDR_EstimateData estimateData = new PDR_EstimateData();
            estimateData.AdminInfoID = estimateID;
            estimateData.RateProfileID = rateProfileID;
            SaveResult result = estimateData.Save(activeLoginID);

            if (result.Success)
            {
                result = PDR_EstimateDataPanel.CreateEmptyListForEstimate(estimateID);
                if (!result.Success)
                {
                    return ProfileSelect(loginID, estimateID, result.ErrorMessage);
                }

                if (openPDR)
                {
                    return RedirectToAction("AddPartsGraphically", "Estimate", new { PDR = "True" });
                }
                else
                {
                    return RedirectToAction("AddPartsGraphically", "Estimate");
                }
            }
            else
            {
                return ProfileSelect(loginID, estimateID, result.ErrorMessage);
            }
        }

        public JsonResult GetPDRSummaryList(int estimateID)
        {
            List<PanelVM> panelVMs = new List<PanelVM>();

            List<PDR_EstimateDataPanel> panels = PDR_EstimateDataPanel.GetForEstimate(estimateID);
            List<PDR_EstimateDataPanelSupplementChange> supplementChanges = PDR_EstimateDataPanelSupplementChange.GetForEstimate(estimateID);
            PDR_EstimateData pdrEstimateData = PDR_EstimateData.GetForEstimate(estimateID);
            List<PDR_Rate> allRates = PDR_Rate.GetByProfile(pdrEstimateData.RateProfileID);

            bool hasVehicle = false;
            Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimateID);
            if (vehicle?.VehicleID > 0)
            {
                hasVehicle = true;
            }

            PanelVMMapper mapper = new PanelVMMapper();
            
            foreach (PDR_EstimateDataPanel panel in panels.Where(o => o.Panel != null))
            {
                PDR_EstimateDataPanelSupplementChange supplementChange = supplementChanges.Where(o => o.EstimateDataPanelID == panel.ID).OrderByDescending(o => o.SupplementVersion).FirstOrDefault();
                PanelVM panelVM = mapper.Map(new PanelVMMapperConfiguration() { Panel = panel, SupplementChange = supplementChange, AllRates = allRates.Where(o => o.Panel.ID == panel.Panel.ID).ToList(), HasVehicle = hasVehicle });
                panelVMs.Add(panelVM);
            }

            return Json(panelVMs, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSummary(int estimateID, int panelID)
        {
            List<PDR_EstimateDataPanel> panels = PDR_EstimateDataPanel.GetForEstimate(estimateID);
            PDR_EstimateDataPanel panel = panels.FirstOrDefault(o => o.Panel.ID == panelID);

            List<PDR_EstimateDataPanelSupplementChange> supplementChanges = PDR_EstimateDataPanelSupplementChange.GetForEstimate(estimateID);
            PDR_EstimateDataPanelSupplementChange supplementChange = supplementChanges.Where(o => o.EstimateDataPanelID == panel.ID).OrderByDescending(o => o.SupplementVersion).FirstOrDefault();

            StringBuilder lineSummaryBuilder = new StringBuilder();

            PDR_Quantity selectedQuantity = panel.Quantity;
            PDR_Size size = panel.Size;
            string selectedOversizedDentQuantity = "";

            if (supplementChange != null)
            {
                selectedOversizedDentQuantity = supplementChange.OversizedDents.ToString();

                selectedQuantity = supplementChange.Quantity;
                size = supplementChange.Size;
            }
            else
            {
                selectedOversizedDentQuantity = panel.OversizedDents.ToString();
            }

            if (selectedQuantity != null)
            {
                lineSummaryBuilder.Append(selectedQuantity.Min.ToString() + " - " + selectedQuantity.Max.ToString() + " " + size.ToString());
            }

            foreach (PDR_EstimateDataPanelOversize oversized in PDR_EstimateDataPanelOversize.GetForDataPanelID(panel.ID).Where(o => o.SupplementDeleted == 0))
            {
                OversizedDentVM vm = new OversizedDentVM(oversized);
                lineSummaryBuilder.Append(" " + vm.Display);
            }

            if (panel.CustomCharge != 0)
            {
                lineSummaryBuilder.Append(" $" + panel.CustomCharge.ToString());
            }

            if (!string.IsNullOrEmpty(selectedOversizedDentQuantity) && selectedOversizedDentQuantity != "0")
            {
                lineSummaryBuilder.Append(" " + selectedOversizedDentQuantity + " Oversized");
            }

            return Json(lineSummaryBuilder.ToString().Trim(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns the PDR profile list
        /// </summary>
        public ActionResult GetPDRProfileList([DataSourceRequest] DataSourceRequest request, int userID, int estimateID, string errorMessage = "")
        {
            PDR_Manager manager = new PDR_Manager();
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            ProfileSelectVM vm = new ProfileSelectVM();
            vm.LoginID = activeLogin.LoginID;
            vm.EstimateID = estimateID;

            if (errorMessage != "PDR")  // "PDR" is a special case meaning to open the PDR popup after selecting the profile
            {
                vm.ErrorMessage = errorMessage;
            }

            PDR_EstimateData estimateData = PDR_EstimateData.GetForEstimate(estimateID);
            if (estimateData == null)
            {
                // This page is called when there is no PDR Estimate Data for an estimate.  This is responsible for creating the rate profile copy and PDR Estimate record

                manager.MakeSureDefaultExists(activeLogin.LoginID, activeLogin.ID);

                List<PDR_RateProfile> profiles = PDR_RateProfile.GetByLogin(activeLogin.LoginID).Where(o => o.AdminInfoID == 0).ToList();

                // Check if pdr rate profiles has only one record then set it as default
                LoginInfo loginInfo = LoginInfo.GetByID(activeLogin.LoginID);
                if (loginInfo.UseDefaultPDRRateProfile == false)
                {
                    if (profiles != null && profiles.Count == 1)
                    {
                        if (profiles[0].IsDefault == false)
                        {
                            profiles[0].IsDefault = true; // set IsDefault flag set to 1
                            profiles[0].Save();
                        }

                        // following updates flag in logininfo table for for UseDefaultPDRRateProfile
                        loginInfo.UseDefaultPDRRateProfile = !loginInfo.UseDefaultPDRRateProfile;
                        loginInfo.Save();
                    }
                }

                // If the organization is set to use a default rate profile, make a copy for the new estimate data
                loginInfo = LoginInfo.GetByID(activeLogin.LoginID);
                if (loginInfo.UseDefaultPDRRateProfile)
                {
                    // If there is no default PDR rate profile, redirect to the page to create one
                    if (profiles.FirstOrDefault(o => o.IsDefault) == null)
                    {
                        ErrorLogger.LogError("No default profile", activeLogin.LoginID, estimateID, "PDRController ProfileSelect");
                        return RedirectToAction("RateProfileList", "Home");
                    }

                    // Copy the default profile and make a new estimate.
                    PDR_RateProfileFunctionResult profileCopyResult = manager.CopyFromDefault(activeLogin.LoginID, activeLogin.ID);

                    if (profileCopyResult.Success)
                    {
                        profileCopyResult.RateProfile.AdminInfoID = estimateID;
                        profileCopyResult.RateProfile.Save();

                        return CreatePDREstimateAndRedirect(activeLogin.LoginID, estimateID, profileCopyResult.RateProfile.ID, activeLogin.ID, errorMessage == "PDR");
                    }
                    else
                    {
                        vm.ErrorMessage = profileCopyResult.ErrorMessage;
                    }
                }

                // Use default rate profile is not set.  Let the user pick one of the rate profiles.
                foreach (PDR_RateProfile profile in profiles)
                {
                    vm.Profiles.Add(new ProfileVM(profile.ProfileName, profile.ID, profile.IsDefault));
                }
            }

            return Json(vm.Profiles.ToDataSourceResult(request));
        }
    }
}