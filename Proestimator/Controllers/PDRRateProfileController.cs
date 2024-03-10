using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data;
using System.Data.SqlClient;

using Proestimator.ViewModel;
using Proestimator.ViewModel.PDR;
using Proestimator;

using ProEstimator.Business.Logic;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.Models;
using ProEstimatorData.Models.EditorTemplateModel;
using ProEstimatorData.Models.SubModel;

namespace Proestimator.Controllers
{
    public class PDRRateProfileController : SiteController 
    {

        /// <summary>
        /// Create a copy of the passed rate profile for the current login.
        /// Redirects to the Rate profile screen with the new profile ID.
        /// </summary>
        public ActionResult CopyRateProfile(int userID, int profileID)
        {
            PDR_Manager manager = new PDR_Manager();
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            PDR_RateProfileFunctionResult result = manager.DuplicateRateProfile(profileID, activeLogin.LoginID, activeLogin.ID);

            if (result.Success)
            {
                return Redirect("/" + userID + "/pdr-rates/" + result.RateProfile.ID);
            }
            else
            {
                return Redirect("/" + userID + "/rates?error=" + result.ErrorMessage);
            }  
        }

        /// <summary>
        /// Create a copy of the system default empty profile
        /// Redirects to the Rate profile screen with the new profile ID.
        /// </summary>
        public ActionResult CreateRateProfile(int userID)
        {
            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                return CopyRateProfile(userID, 2);
            }
            return Redirect("/");
        }

        /// <summary>
        /// Make a copy of the system default rate profile
        /// Redirects to the Rate profile screen with the new profile ID.
        /// </summary>
        public ActionResult CopyFromDefault(int userID)
        {
            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                return CopyRateProfile(userID, 1);
            }
            return Redirect("/");
        }

        public JsonResult DeleteRateProfile(int userID, int profileID)
        {
            CacheActiveLoginID(userID);

            FunctionResult result = new FunctionResult();

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                // Get all default rate profiles for the logged in user
                PDR_RateProfile profile = PDR_RateProfile.GetByID(profileID);
                List<PDR_RateProfile> allProfiles = PDR_RateProfile.GetByLogin(activeLogin.LoginID);

                if (allProfiles.Count == 1)
                {
                    result.ErrorMessage = @Proestimator.Resources.ProStrings.CannotDeleteOnlyProfile;
                }
                else if (profile == null)
                {
                    result.ErrorMessage = @Proestimator.Resources.ProStrings.ProfileNotFound;
                }
                    else if (profile.LoginID != activeLogin.LoginID)
                {
                    result.ErrorMessage = @Proestimator.Resources.ProStrings.InvalidRateProfile;
                }
                else
                {
                    PDR_Manager manager = new PDR_Manager();
                    manager.DeleteRateProfile(profileID, activeLogin.LoginID, activeLogin.ID);
                }

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    result.Success = false;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RestoreRateProfile(int userID, int profileID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                PDR_Manager manager = new PDR_Manager();
                FunctionResult result = manager.RestoreRateProfile(profileID, activeLogin.LoginID, activeLogin.ID);

                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new FunctionResult("Unauthorized Login"), JsonRequestBehavior.AllowGet);
        }

        [Route("{userID}/pdr-rates/{profileID}/rate-matrix")]
        public ActionResult RateMatrix(int userID, int profileID)
        {
            RateMatrixVM vm = new RateMatrixVM();
            ViewBag.LoginID = ActiveLogin.LoginID;

            ViewBag.RateProfileNavID = 1;
            ViewBag.RateProfileSelectedTab = "pdr-rate-profiles";

            try
            {
                vm.LoginID = ActiveLogin.LoginID;
                vm.ProfileID = profileID;
                // FillRates(ActiveLogin.LoginID, vm);

                PDR_RateProfile profile = PDR_RateProfile.GetByID(profileID);

                if (profile != null && profile.LoginID == ActiveLogin.LoginID)
                {
                    List<PDR_Rate> rates = PDR_Rate.GetByProfile(profile.ID);
                    vm.LoadData(profile, rates);

                    vm.GoodData = true;
                    ViewBag.NavID = "rates";

                    // Hilight the left menu button if global, the top "edit rate profile" button if an estimate's profile
                    if (profile.AdminInfoID == 0)
                    {
                        ViewBag.NavID = "rates";
                    }
                    else
                    {
                        ViewBag.EstimateNavID = "profile";
                        ViewBag.EstimateID = profile.AdminInfoID;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, userID, 0, "RateProfileController Rates");
            }

            return View(vm);
        }

        [Route("{userID}/pdr-rates/{profileID}/rate-matrix")]
        [HttpPost]
        public ActionResult RateMatrix(RateMatrixVM vm)
        {
            try
            {
                ViewBag.RateProfileNavID = 1;
                ViewBag.RateProfileSelectedTab = "pdr-rate-profiles";

                PDR_RateProfile profile = PDR_RateProfile.GetByID(vm.ProfileID);

                if (profile != null && profile.LoginID == ActiveLogin.LoginID)
                {
                    List<PDR_Rate> rates = PDR_Rate.GetByProfile(profile.ID);

                    int saveCounter = 0;

                    // Check all rates, save changes
                    foreach(RateMatrixPanelVM panelVM in vm.Panels)
                    {
                        foreach(RateMatrixQuantityWrapperVM quantityWrapper in panelVM.QuantityWrappers)
                        {
                            foreach(RateMatrixRateVM rateVM in quantityWrapper.Rates)
                            {
                                PDR_Rate rate = rates.FirstOrDefault(o => o.ID == rateVM.ID);
                                decimal newValue = InputHelper.GetDecimal(rateVM.Amount);
                                if (rate != null && rate.Amount != newValue)
                                {
                                    rate.Amount = newValue;
                                    string name = profile.ProfileName + " Rate Matrix " + rate.Panel.PanelName + " " + rate.Quantity.Min + "-" + rate.Quantity.Max + " " + rate.Size;
                                    rate.Save(ActiveLogin.ID, ActiveLogin.LoginID, name);

                                    saveCounter++;
                                }
                            }
                        }
                    }

                    if (profile.AdminInfoID > 0)
                    {
                        Estimate.RefreshProcessedLines(profile.AdminInfoID);
                    }

                    ViewBag.NavID = "rates";
                    vm.GoodData = true;
                    vm.Message = "Saved " + saveCounter.ToString("N0") + " rate" + (saveCounter == 1 ? "" : "s");

                    // Hilight the left menu button if global, the top "edit rate profile" button if an estimate's profile
                    if (profile.AdminInfoID == 0)
                    {
                        ViewBag.LoginID = profile.LoginID;
                        ViewBag.NavID = "rates";
                    }
                    else
                    {
                        ViewBag.EstimateNavID = "profile";
                        ViewBag.LoginID = profile.LoginID;
                        ViewBag.EstimateID = profile.AdminInfoID;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, 0, 0, "RateProfileController Rates");
            }

            return DoRedirect("/" + ActiveLogin.SiteUserID + "/pdr-rates/" + vm.ProfileID + "/rate-matrix");
        }

        [Route("{userID}/pdr-rates/{profileID}")]
        public ActionResult Rates(int userID, int profileID, int panelID = 1, string message = "")
        {
            PDRRatesVM vm = new PDRRatesVM();

            ViewBag.RateProfileNavID = 0;
            ViewBag.LoginID = ActiveLogin.LoginID;

            ViewBag.RateProfileSelectedTab = "pdr-rate-profiles";
            try
            {
                vm.LoginID = ActiveLogin.LoginID;
                vm.ProfileID = profileID;
                vm.SelectedPanel = panelID;
                FillRates(ActiveLogin.LoginID, vm);

                if (!string.IsNullOrEmpty(message))
                {
                    vm.Message = message;
                }

                PDR_RateProfile profile = PDR_RateProfile.GetByID(profileID);

                if (profile != null && profile.LoginID == ActiveLogin.LoginID)
                {
                    ViewBag.NavID = "rates";

                    // Hilight the left menu button if global, the top "edit rate profile" button if an estimate's profile
                    if (profile.AdminInfoID == 0)
                    {
                        ViewBag.LoginID = userID;
                        ViewBag.NavID = "rates";

                        List<PDR_RateProfile> rateProfiles = PDR_RateProfile.GetByLogin(userID, false);

                        rateProfiles = rateProfiles.Where(eachRateProfile => string.Compare(eachRateProfile.ProfileName, profile.ProfileName, true) == 0).ToList<PDR_RateProfile>();

                        ViewBag.PDRErrorMessage = string.Empty;

                        if (rateProfiles.Count > 1)
                        {
                            ViewBag.PDRErrorMessage = "There are " + rateProfiles.Count() + " PDR rate profiles with this same name";
                        }

                        ViewBag.EstimateID = 0;
                    }
                    else
                    {
                        ViewBag.EstimateNavID = "profile";
                        
                        ViewBag.EstimateID = profile.AdminInfoID;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, userID, 0, "RateProfileController Rates");
            }

            return View(vm);
        }

        [HttpPost]
        [Route("{userID}/pdr-rates/")]
        public ActionResult Rates(PDRRatesVM vm)
        {
            PDR_RateProfile profile = PDR_RateProfile.GetByID(vm.ProfileID);
            if (profile != null && profile.LoginID == vm.LoginID)
            {
                // Save the rate profile info
                profile.ProfileName = vm.ProfileName;
                profile.HideDentCounts = vm.HideDentCounts;
                profile.Taxable = vm.Taxable;

                SaveResult saveResult = profile.Save();
                if (saveResult.Success)
                {
                    if (vm.IsDefault)
                    {
                        SetDefaultRateProfile(ActiveLogin.SiteUserID, profile.ID);
                    }

                    if (profile.AdminInfoID > 0)
                    {
                        Estimate.RefreshProcessedLines(profile.AdminInfoID);
                    }
                }
                else
                {
                    vm.Message = saveResult.ErrorMessage;
                }
            }

            return DoRedirect("/" + ActiveLogin.SiteUserID + "/pdr-rates/" + vm.ProfileID);
        }

        [HttpPost]
        [Route("{userID}/pdr-rates/panel-changed")]
        public ActionResult SelectedPanelChanged(PDRRatesVM vm)
        {
            return Redirect("/" + ActiveLogin.SiteUserID + "/pdr-rates/" + vm.ProfileID + "?panelID=" + vm.SelectedPanel);
        }

        [HttpPost]
        [Route("{userID}/pdr-rates/copy-rates")]
        public ActionResult CopyRates(PDRRatesVM vm)
        {
            PDR_Manager manager = new PDR_Manager();
            FunctionResult result = manager.CopyPanel(vm.CopyPanel, vm.SelectedPanel, vm.ProfileID, ActiveLogin.ID);

            if (result.Success)
            {
                return RedirectToAction("Rates", new { panelID = vm.SelectedPanel, ProfileID = vm.ProfileID });
            }
            else
            {
                vm.Message = result.ErrorMessage;
                return View(vm);
            }            
        }

        [HttpPost]
        [Route("{userID}/pdr-rates/save-rates")]
        public ActionResult SaveRates(PDRRatesVM vm)
        {
            PDR_RateProfile profile = PDR_RateProfile.GetByID(vm.ProfileID);
            if (profile != null && profile.LoginID == vm.LoginID)
            {
                // Save the rate profile info
                profile.ProfileName = vm.ProfileName;
                profile.HideDentCounts = vm.HideDentCounts;
                profile.Taxable = vm.Taxable;
                
                profile.Save();

                if (vm.IsDefault)
                {
                    SetDefaultRateProfile(vm.LoginID, profile.ID);
                }

                // Save the changed rate values
                int changeCount = 0;
                List<PDR_Rate> allRates = profile.GetAllRates();
                if (allRates == null || allRates.Count == 0)
                {
                    vm.Message = @Proestimator.Resources.ProStrings.NoRateFoundForProfile;
                }
                else
                {
                    List<PDR_Rate> rates = allRates.Where(o => o.Panel != null && o.Panel.ID == vm.SelectedPanel).ToList();
                    changeCount += SaveRateChanges(vm.DimeRates, rates);
                    changeCount += SaveRateChanges(vm.NickelRates, rates);
                    changeCount += SaveRateChanges(vm.QuarterRates, rates);
                    changeCount += SaveRateChanges(vm.HalfRates, rates);

                    // Save the changed oversized dents
                    foreach (OversizeddRateVM oversizedRateVM in vm.OversizedRates)
                    {
                        PDR_Rate rate = rates.FirstOrDefault(o => o.ID == oversizedRateVM.RateID);
                        if (rate != null && rate.Amount != oversizedRateVM.Amount)
                        {
                            rate.Amount = oversizedRateVM.Amount;
                            rate.Save(ActiveLogin.ID, ActiveLogin.LoginID);

                            changeCount++;
                        }
                    }

                    PDR_Rate oversizedRate = rates.FirstOrDefault(o => o.Size == PDR_Size.Oversized);
                    oversizedRate.Amount = InputHelper.GetDecimal(vm.OversizedPrice);
                    oversizedRate.Save(ActiveLogin.ID, ActiveLogin.LoginID);

                    //if (CurrentSession.EstimateID > 0)
                    //{
                    //    DBAccess db = new DBAccess();
                    //    db.ExecuteNonQuery("UpdateAdminTotals", new SqlParameter("AdminInfoID", CurrentSession.EstimateID));
                    //}

                    vm.Message = "Rate profile saved.  ";
                }

                if (changeCount > 0)
                {
                    vm.Message += changeCount.ToString() + " rates changed.";
                }
            }

            return Redirect("/" + ActiveLogin.SiteUserID + "/pdr-rates/" + vm.ProfileID + "?panelID=" + vm.SelectedPanel + "&message=" + vm.Message);
        }

        private int SaveRateChanges(RateSectionVM rateSectionVM, List<PDR_Rate> allRates)
        {
            int changeCount = 0;
            changeCount += SaveRateChange(rateSectionVM.Size, 1, rateSectionVM.Range_1_5, allRates, ActiveLogin.ID, ActiveLogin.LoginID);
            changeCount += SaveRateChange(rateSectionVM.Size, 6, rateSectionVM.Range_6_15, allRates, ActiveLogin.ID, ActiveLogin.LoginID);
            changeCount += SaveRateChange(rateSectionVM.Size, 16, rateSectionVM.Range_16_30, allRates, ActiveLogin.ID, ActiveLogin.LoginID);
            changeCount += SaveRateChange(rateSectionVM.Size, 31, rateSectionVM.Range_31_50, allRates, ActiveLogin.ID, ActiveLogin.LoginID);
            changeCount += SaveRateChange(rateSectionVM.Size, 51, rateSectionVM.Range_51_75, allRates, ActiveLogin.ID, ActiveLogin.LoginID);
            changeCount += SaveRateChange(rateSectionVM.Size, 76, rateSectionVM.Range_76_100, allRates, ActiveLogin.ID, ActiveLogin.LoginID);
            changeCount += SaveRateChange(rateSectionVM.Size, 101, rateSectionVM.Range_101_150, allRates, ActiveLogin.ID, ActiveLogin.LoginID);
            changeCount += SaveRateChange(rateSectionVM.Size, 151, rateSectionVM.Range_151_200, allRates, ActiveLogin.ID, ActiveLogin.LoginID);
            changeCount += SaveRateChange(rateSectionVM.Size, 201, rateSectionVM.Range_201_300, allRates, ActiveLogin.ID, ActiveLogin.LoginID);
            return changeCount;
        }

        private int SaveRateChange(PDR_Size size, int min, string newValueString, List<PDR_Rate> allRates, int activeLoginID, int loginID)
        {
            int changeCount = 0;
            decimal newValue = InputHelper.GetDecimal(newValueString);

            PDR_Rate rate = allRates.FirstOrDefault(o => o.Size == size && o.Quantity.Min == min);
            if (rate != null && rate.Amount != newValue)
            {
                changeCount++;

                rate.Amount = newValue;
                rate.Save(activeLoginID, loginID);
            }

            return changeCount;
        }

        private void FillRates(int loginID, PDRRatesVM vm)
        {
            PDR_RateProfile rateProfile = PDR_RateProfile.GetByID(vm.ProfileID);
            if (rateProfile != null && rateProfile.LoginID == loginID)
            {
                vm.GoodData = true;
                vm.LoadRateProfile(rateProfile);
            }
            else
            {
                vm.GoodData = false;
            }
        }

        public JsonResult SetDefaultRateProfile(int userID, int profileID)
        {
            CacheActiveLoginID(userID);

            string result = "";

            try
            {
                if (IsUserAuthorized(userID))
                {
                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey(), true);

                    List<PDR_RateProfile> allProfiles = PDR_RateProfile.GetByLogin(activeLogin.LoginID);
                    foreach (PDR_RateProfile profile in allProfiles)
                    {
                        if (profile.ID == profileID && !profile.IsDefault)
                        {
                            profile.IsDefault = true;
                            profile.Save();
                        }

                        if (profile.ID != profileID && profile.IsDefault)
                        {
                            profile.IsDefault = false;
                            profile.Save();
                        }
                    }

                    return Json(new FunctionResult(true, Proestimator.Resources.ProStrings.Message_DefaultRateProfileChanged), JsonRequestBehavior.AllowGet);
                }
            }
            catch (System.Exception ex)
            {
                result = Proestimator.Resources.ProStrings.Message_ErrorSettingDefaultProfileID + ": " + ex.Message;
                if (ex.InnerException != null)
                {
                    result += Environment.NewLine + ex.InnerException.Message;
                }
            }

            return Json(new FunctionResult(result), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ToggleUseDefaultProfile(int userID)
        {
            CacheActiveLoginID(userID);

            ToggleUseDefaultProfileResult result = new ToggleUseDefaultProfileResult();

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                LoginInfo loginInfo = LoginInfo.GetByID(activeLogin.LoginID);
                loginInfo.UseDefaultPDRRateProfile = !loginInfo.UseDefaultPDRRateProfile;
                SaveResult saveResult = loginInfo.Save();

                if (saveResult.Success)
                {
                    result.Success = true;
                    result.UseDefaultProfile = loginInfo.UseDefaultPDRRateProfile;
                }
                else
                {
                    result.Success = false;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public class ToggleUseDefaultProfileResult
        {
            public bool Success { get; set; }
            public bool UseDefaultProfile { get; set; }
        }

        #region Multipliers

        [HttpGet]
        [Route("{userID}/pdr-rates/{profileID}/multipliers")]
        public ActionResult Multipliers(int userID, int profileID)
        {
            ViewBag.RateProfileNavID = 3;
            ViewBag.LoginID = ActiveLogin.LoginID;

            MultipliersVM vm = GetMultipliersVM(ActiveLogin.LoginID, profileID);
            vm.LoginID = ActiveLogin.LoginID;

            PDR_RateProfile profile = PDR_RateProfile.GetByID(profileID);
            ViewBag.RateProfileSelectedTab = "pdr-rate-profiles";

            if (profile != null && profile.LoginID == ActiveLogin.LoginID)
            {
                ViewBag.NavID = "rates";

                // Hilight the left menu button if global, the top "edit rate profile" button if an estimate's profile
                if (profile.AdminInfoID == 0)
                {
                    ViewBag.NavID = "rates";
                }
                else
                {
                    ViewBag.EstimateNavID = "profile";
                    ViewBag.EstimateID = profile.AdminInfoID;
                }
            }

            return View(vm);
        }

        [HttpPost]
        [Route("{userID}/pdr-rates/{profileID}/multipliers")]
        public ActionResult Multipliers(MultipliersVM vm)
        {
            List<PDR_Multiplier> multipliers = PDR_Multiplier.GetByProfile(vm.ProfileID);

            System.Text.StringBuilder errorBuilder = new System.Text.StringBuilder();

            foreach(MultiplierVM multiplierVM in vm.Multipliers)
            {
                PDR_Multiplier multiplier = multipliers.FirstOrDefault(o => o.ID == multiplierVM.ID);
                if (multiplier != null)
                {
                    multiplier.Name = multiplierVM.Name;
                    multiplier.Value = InputHelper.GetDouble(multiplierVM.Value);
                    SaveResult saveResult = multiplier.Save(ActiveLogin.ID, ActiveLogin.LoginID);
                    if (!saveResult.Success)
                    {
                        errorBuilder.AppendLine(saveResult.ErrorMessage);
                    }
                }
            }

            vm = GetMultipliersVM(vm.LoginID, vm.ProfileID);
            vm.GoodData = true;

            vm.ErrorMessage = errorBuilder.ToString();
            if (string.IsNullOrEmpty(vm.ErrorMessage))
            {
                vm.ErrorMessage = @Proestimator.Resources.ProStrings.MultiplierDataSaved;
            }

            PDR_RateProfile profile = PDR_RateProfile.GetByID(vm.ProfileID);
            if (profile != null && profile.LoginID == ActiveLogin.LoginID)
            {
                ViewBag.NavID = "rates";

                // Hilight the left menu button if global, the top "edit rate profile" button if an estimate's profile
                if (profile.AdminInfoID == 0)
                {
                    ViewBag.NavID = "rates";
                }
                else
                {
                    ViewBag.EstimateNavID = "profile";
                    ViewBag.EstimateID = profile.AdminInfoID;
                }

                if (profile.AdminInfoID > 0)
                {
                    Estimate.RefreshProcessedLines(profile.AdminInfoID);
                }
            }
            ViewBag.RateProfileSelectedTab = "pdr-rate-profiles";
            return DoRedirect("/" + ActiveLogin.SiteUserID + "/pdr-rates/" + vm.ProfileID + "/multipliers");
        }

        public JsonResult AddMultiplier(int userID, int loginID, int id)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                PDR_RateProfile rateProfile = PDR_RateProfile.GetByID(id);
                if (rateProfile != null && rateProfile.LoginID == loginID)
                {
                    PDR_Multiplier multiplier = new PDR_Multiplier();
                    multiplier.RateProfileID = id;
                    multiplier.Name = "New Multiplier";
                    multiplier.Value = 0;

                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                    SaveResult saveResult = multiplier.Save(activeLogin.ID, activeLogin.LoginID);

                    MultiplierVM vm = new MultiplierVM(multiplier);
                    vm.Index = PDR_Multiplier.GetByProfile(id).Count - 1;

                    return Json(new JsonData(saveResult.Success, saveResult.ErrorMessage, vm), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new JsonData(false, @Proestimator.Resources.ProStrings.InvalidProfileCannotAddMultiplier, null), JsonRequestBehavior.AllowGet);
                }
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteMultiplier(int userID, int loginID, int id)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                PDR_Multiplier multiplier = PDR_Multiplier.GetByID(id);
                if (multiplier != null)
                {
                    PDR_RateProfile profile = PDR_RateProfile.GetByID(multiplier.RateProfileID);
                    if (profile != null && profile.LoginID == loginID)
                    {
                        multiplier.Delete();
                        return Json(new JsonData(true, "", null), JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new JsonData(false, @Proestimator.Resources.ProStrings.InvalidMultiplierCannotDelete, null), JsonRequestBehavior.AllowGet);
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        private MultipliersVM GetMultipliersVM(int loginID, int profileID)
        {
            MultipliersVM vm = new MultipliersVM();

            vm.LoginID = loginID;

            PDR_RateProfile rateProfile = PDR_RateProfile.GetByID(profileID);
            if (rateProfile != null && rateProfile.LoginID == loginID)
            {
                vm.GoodData = true;
                vm.ProfileID = profileID;

                List<PDR_Multiplier> multipliers = PDR_Multiplier.GetByProfile(profileID);
                vm.LoadMultipliers(multipliers);
            }
            else
            {
                vm.GoodData = false;
            }

            return vm;
        }

        #endregion

        #region Description Presets

        [HttpGet]
        [Route("{userID}/pdr-rates/presets")]
        public ActionResult DescriptionPresets(int userID)
        {
            DescriptionPresetsVM vm = GetDescriptionPresetsVM(ActiveLogin.LoginID);
            vm.LoginID = ActiveLogin.LoginID;

            ViewBag.NavID = "";
            ViewBag.RateProfileNavID = 1;

            return View(vm);
        }


        [HttpPost]
        [Route("{userID}/pdr-rates/presets")]
        public ActionResult DescriptionPresets(DescriptionPresetsVM vm)
        {
            List<PDR_DescriptionPreset> presets = PDR_DescriptionPreset.GetAllByLoginID(ActiveLogin.LoginID);

            System.Text.StringBuilder errorBuilder = new System.Text.StringBuilder();

            foreach (DescriptionPresetVM presetVM in vm.DescriptionPresets)
            {
                PDR_DescriptionPreset preset = presets.FirstOrDefault(o => o.ID == presetVM.ID);
                if (preset != null)
                {
                    if (preset.Text != presetVM.Text)
                    {
                        preset.Text = presetVM.Text;
                        SaveResult saveResult = preset.Save(ActiveLogin.ID);
                        if (!saveResult.Success)
                        {
                            errorBuilder.AppendLine(saveResult.ErrorMessage);
                        }
                    }
                }
            }

            vm = GetDescriptionPresetsVM(ActiveLogin.LoginID);
            vm.GoodData = true;

            vm.ErrorMessage = errorBuilder.ToString();
            if (string.IsNullOrEmpty(vm.ErrorMessage))
            {
                vm.ErrorMessage = @Proestimator.Resources.ProStrings.DescriptionPresetDataSaved;
            }

            return DoRedirect("/" + ActiveLogin.SiteUserID + "/pdr-rates/presets");
        }

        public JsonResult AddPreset(int userID, int loginID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                PDR_DescriptionPreset preset = new PDR_DescriptionPreset();
                preset.LoginID = loginID;
                preset.Text = "New Description";
                SaveResult saveResult = preset.Save(activeLogin.ID);

                DescriptionPresetVM vm = new DescriptionPresetVM(preset);
                vm.Index = PDR_DescriptionPreset.GetAllByLoginID(loginID).Count - 1;

                return Json(new JsonData(saveResult.Success, saveResult.ErrorMessage, vm), JsonRequestBehavior.AllowGet);
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeletePreset(int userID, int loginID, int id)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                PDR_DescriptionPreset preset = PDR_DescriptionPreset.GetByID(id);
                if (preset != null && preset.LoginID == loginID)
                {
                    preset.Delete();
                    return Json(new JsonData(true, "", null), JsonRequestBehavior.AllowGet);
                }

                return Json(new JsonData(false, @Proestimator.Resources.ProStrings.InvalidPresetCannotDelete, null), JsonRequestBehavior.AllowGet);
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DescriptionPreset(int userID, int id, string text)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                PDR_DescriptionPreset preset = PDR_DescriptionPreset.GetByID(id);
                if (preset != null)
                {
                    preset.Text = text;
                    SaveResult saveResult = preset.Save(ActiveLogin.ID);
                    return Json(new JsonData(saveResult.Success, saveResult.ErrorMessage, null), JsonRequestBehavior.AllowGet);
                }

                return Json(new JsonData(false, @Proestimator.Resources.ProStrings.InvalidPresetData, null), JsonRequestBehavior.AllowGet);
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        private DescriptionPresetsVM GetDescriptionPresetsVM(int loginID)
        {
            DescriptionPresetsVM vm = new DescriptionPresetsVM();

            List<PDR_DescriptionPreset> descriptionPresets = PDR_DescriptionPreset.GetAllByLoginID(loginID);
            if (descriptionPresets != null)
            {
                vm.GoodData = true;
                vm.LoadDescriptionPresets(descriptionPresets);
            }
            else
            {
                vm.GoodData = false;
            }

            return vm;
        }

        #endregion

        [Route("{userID}/pdr-rates/{profileID}/oversized-dents")]
        public ActionResult OversizedDents(int userID, int profileID)
        {
            OversizedDentsVM vm = new OversizedDentsVM();
            ViewBag.LoginID = ActiveLogin.LoginID;

            ViewBag.RateProfileNavID = 2;
            ViewBag.RateProfileSelectedTab = "pdr-rate-profiles";

            try
            {
                vm.LoginID = ActiveLogin.LoginID;
                vm.ProfileID = profileID;

                PDR_RateProfile profile = PDR_RateProfile.GetByID(profileID);

                if (profile != null && profile.LoginID == ActiveLogin.LoginID)
                {
                    List<PDR_Rate> rates = PDR_Rate.GetByProfile(profile.ID);
                    vm.LoadData(profile, rates);

                    vm.GoodData = true;
                    ViewBag.NavID = "rates";

                    // Hilight the left menu button if global, the top "edit rate profile" button if an estimate's profile
                    if (profile.AdminInfoID == 0)
                    {
                        ViewBag.NavID = "rates";
                    }
                    else
                    {
                        ViewBag.EstimateNavID = "profile";
                        ViewBag.EstimateID = profile.AdminInfoID;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, userID, 0, "RateProfileController Rates");
            }

            return View(vm);
        }

        [Route("{userID}/pdr-rates/{profileID}/oversized-dents")]
        [HttpPost]
        public ActionResult OversizedDents(OversizedDentsVM vm)
        {
            try
            {
                ViewBag.RateProfileNavID = 2;
                ViewBag.RateProfileSelectedTab = "pdr-rate-profiles";

                PDR_RateProfile profile = PDR_RateProfile.GetByID(vm.ProfileID);

                if (profile != null && profile.LoginID == ActiveLogin.LoginID)
                {
                    List<PDR_Rate> rates = PDR_Rate.GetByProfile(profile.ID).Where(o => o.Panel.ID == 1).ToList();

                    int saveCounter = 0;

                    foreach(OversizedDentSizeRowVM sizeRowVM in vm.Rows)
                    {
                        foreach(OversizedDentRateVM dentRateVM in sizeRowVM.Rates)
                        {
                            PDR_Rate rate = rates.FirstOrDefault(o => (int)o.Size == sizeRowVM.SizeID && (int)o.Depth == dentRateVM.DepthID);

                            decimal newAmount = InputHelper.GetDecimal(dentRateVM.Amount);

                            if (rate.Amount != newAmount)
                            {
                                rate.Amount = newAmount;
                                rate.Save(ActiveLogin.ID, ActiveLogin.LoginID, profile.ProfileName + " Oversized Dents " + rate.Size + " " + rate.Depth);

                                saveCounter++;
                            }
                        }
                    }

                    PDR_Rate oversizedFlatRate = rates.FirstOrDefault(o => o.Size == PDR_Size.Oversized && o.Panel.ID == 1);
                    if (oversizedFlatRate.Amount != (decimal)vm.FlatRate)
                    {
                        oversizedFlatRate.Amount = (decimal)vm.FlatRate;
                        oversizedFlatRate.Save(ActiveLogin.ID, ActiveLogin.LoginID, profile.ProfileName + " Oversized Dents " + oversizedFlatRate.Size);

                        saveCounter++;
                    }
                    

                    ViewBag.NavID = "rates";
                    vm.GoodData = true;
                    vm.Message = "Saved " + saveCounter.ToString("N0") + " rate" + (saveCounter == 1 ? "" : "s");

                    // Hilight the left menu button if global, the top "edit rate profile" button if an estimate's profile
                    if (profile.AdminInfoID == 0)
                    {
                        ViewBag.LoginID = profile.LoginID;
                        ViewBag.NavID = "rates";
                    }
                    else
                    {
                        Estimate.RefreshProcessedLines(profile.AdminInfoID);

                        ViewBag.EstimateNavID = "profile";
                        ViewBag.LoginID = profile.LoginID;
                        ViewBag.EstimateID = profile.AdminInfoID;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, 0, 0, "RateProfileController Rates");
            }

            return DoRedirect("/" + ActiveLogin.SiteUserID + "/pdr-rates/" + vm.ProfileID + "/oversized-dents");
        }
    }
}