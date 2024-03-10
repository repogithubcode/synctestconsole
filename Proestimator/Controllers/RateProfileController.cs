using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data;
using System.Data.SqlClient;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using Proestimator.ViewModel.Profiles;
using Proestimator.Helpers;
using ProEstimatorData;
using ProEstimatorData.Models;
using ProEstimatorData.Models.EditorTemplateModel;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Profiles;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.Helpers;
using ProEstimator.Business.Logic;
using Proestimator.Resources;

using ProEstimator.DataRepositories.Vendors;
using ProEstimatorData.DataModel.ProAdvisor;
using ProEstimator.DataRepositories.ProAdvisor;
using ProEstimator.Business.ProAdvisor;

namespace Proestimator.Controllers
{
    public class RateProfileController : SiteController
    {

        private IEstimateService _estimateService;
        private IVendorRepository _vendorRepository;
        private IProAdvisorProfileService _proAdvisorProfileService;
        private IProAdvisorService _proAdvisorService;

        public RateProfileController(IEstimateService estimateService, IVendorRepository vendorRepository, IProAdvisorProfileService proAdvisorProfileService, IProAdvisorService proAdvisorService)
        {
            _estimateService = estimateService;
            _vendorRepository = vendorRepository;
            _proAdvisorProfileService = proAdvisorProfileService;
            _proAdvisorService = proAdvisorService;
        }

        #region Rates

        [HttpGet]
        [Route("{userID}/rates")]
        public ActionResult RateProfileList(int userID, string error = "")
        {
            RateProfileListVM vm = new RateProfileListVM();
            vm.ErrorMessage = error;

            LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
            if (loginInfo != null)
            {
                vm.UseDefaultProfile = loginInfo.UseDefaultRateProfile;
                vm.UsePDRDefaultProfile = loginInfo.UseDefaultPDRRateProfile;
                vm.UseDefaultAddOnProfile = _proAdvisorProfileService.UseDefaultProfile(ActiveLogin.LoginID);
            }

            //ProEstimatorData.DataModel.Contracts.Contract pdrContract = ProEstimator.Business.Logic.ContractManager.GetActiveContractByType(loginID, 7);
            //if (pdrContract != null && pdrContract.DaysUntilExpiration > 0)
            //{
            // Ezra 6/12/2019 - Everyone currently has access to PDR
            PDR_Manager manager = new PDR_Manager();
            manager.MakeSureDefaultExists(ActiveLogin.LoginID, ActiveLogin.ID);

            vm.ShowPDR = true;
            //}

            if (ActiveLogin.HasProAdvisorContract)
            {
                vm.ShowAddOns = true;
            }

            ViewBag.NavID = "rates";
            ViewBag.LoginID = ActiveLogin.LoginID;

            return View(vm);
        }

        [HttpGet]
        [Route("{userID}/rates/rateProfileSelect/{rateProfileSelectedTab}")]
        public ActionResult Index(int userID, string rateProfileSelectedTab)
        {
            //ViewBag.SelectedTab = rateProfileToSelect;
            TempData.Add("rateProfileSelectedTab", rateProfileSelectedTab);
            return DoRedirect("/" + userID + "/rates");
        }

        public ActionResult GetRateProfiles(
              [DataSourceRequest] DataSourceRequest request
            , int userID
            , bool showDeleted
        )
        {
            List<RateProfileGridRow> gridRows = new List<RateProfileGridRow>();

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                List<RateProfile> rateProfiles = RateProfile.GetAllForLogin(activeLogin.LoginID, showDeleted);

                foreach(RateProfile rateProfile in rateProfiles)
                {
                    gridRows.Add(new RateProfileGridRow(rateProfile));
                }
            }

            return Json(gridRows.ToDataSourceResult(request));
        }

        public ActionResult GetPDRRateProfiles(
              [DataSourceRequest] DataSourceRequest request
            , int userID
            , bool showDeleted
        )
        {
            List<RateProfileGridRow> gridRows = new List<RateProfileGridRow>();

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                List<PDR_RateProfile> rateProfiles = PDR_RateProfile.GetByLogin(activeLogin.LoginID, showDeleted);

                foreach (PDR_RateProfile rateProfile in rateProfiles)
                {
                    gridRows.Add(new RateProfileGridRow(rateProfile));
                }
            }

            return Json(gridRows.ToDataSourceResult(request));
        }        

        #endregion

        #region Rates

        [HttpGet]
        [Route("{userID}/rates/{profileID}")]
        public ActionResult Rates(int userID, int profileID)
        {
            RateVM model = new RateVM();

            //if (CurrentSession.EstimateID > 0 && profileID == 0)
            //{
            //    return RedirectToAction("SelectRateProfile", "Estimate");
            //}

            try
            {
                LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
                model.UseDefaultProfile = loginInfo.UseDefaultRateProfile;

                RateProfile profile = RateProfile.Get(profileID);
                if (profile != null && profile.LoginID == ActiveLogin.LoginID)
                {
                    model.LoginID = ActiveLogin.LoginID;
                    ViewBag.EstimateID = profile.EstimateID;

                    // Hilight the left menu button if global, the top "edit rate profile" button if an estimate's profile
                    if (profile.EstimateID == 0)
                    {
                        ViewBag.NavID = "rates";
                        model.IsGlobalProfile = true;

                        List<RateProfile> rateProfiles = RateProfile.GetAllForLogin(userID, false);

                        rateProfiles = rateProfiles.Where(eachRateProfile => 
                                            string.Compare(eachRateProfile.Name, profile.Name, true) == 0).ToList<RateProfile>();

                        if(rateProfiles.Count > 1)
                        {
                            model.ErrorMessage = "There are " + rateProfiles.Count() + " rate profiles with this same name";
                        }
                    }
                    else
                    {
                        ViewBag.NavID = "rates";
                        ViewBag.EstimateNavID = "profile";
                        model.IsGlobalProfile = false;

                        Estimate estimate = new Estimate(profile.EstimateID);
                        model.EstimateIsLocked = estimate.IsLocked();
                    }

                    model.ProfileIsValid = true;
                    model.Description = profile.Description;
                    model.ProfileName = profile.Name;
                    model.ProfileID = profileID;
                    model.ProfileDefault = profile.IsDefault;
                    model.PresetsDefault = profile.IsDefaultPresets;
                    model.EstimateID = profile.EstimateID;
                    model.CapRatesAfterInclude = profile.CapRatesAfterInclude;
                    model.CapSuppliesAfterInclude = profile.CapSuppliesAfterInclude;
                    model.IsFullEstimateDiscountMarkup = profile.IsFullEstimateDiscountMarkup;
                    model.FullEstimateDiscountMarkupValue = profile.FullEstimateDiscountMarkupValue;
                    model.CreditCardFeePercentage = profile.CreditCardFeePercentage;
                    model.ApplyCreditCardFee = profile.ApplyCreditCardFee;
                    model.TaxedCreditCardFee = profile.TaxedCreditCardFee;

                    List<Rate> rates = Rate.GetForProfile(profileID);
                    if (rates != null && rates.Count > 0)
                    {
                        for (int CurrentRow = 0; CurrentRow < rates.Count; CurrentRow++)
                        {
                            Rate rate = rates[CurrentRow];

                            if (rate.RateType.RateName.Contains("Finish"))
                            {
                                // I think Finish is an old unused rate but it's still in the database
                                continue;
                            }

                            if (rate.RateType.RateName.Contains("Labor"))
                            {
                                model.LaborRates.Add(new RateLabor(rate, GetLaborDropDown(rate.RateType.ID)));
                            }
                            else if (rate.RateType.RateName.Contains("Supplies"))
                            {
                                List<SimpleListItem> dropDownList = null;

                                if (rate.RateType.ID == 22)
                                {
                                    dropDownList = new List<SimpleListItem>() { new SimpleListItem("", "0"), new SimpleListItem("Paint", "7") };
                                }

                                model.SuppliesRates.Add(new RateLabor(rate, dropDownList));
                            }
                            else if (rate.RateType.RateName.Contains("Parts"))
                            {
                                model.PartsRates.Add(new RatePart(rate));
                            }
                            else
                            {
                                model.OtherRates.Add(new RatePart(rate));
                            }
                        }
                    }
                    else
                    {
                        ErrorLogger.LogError("No rates found for " + profileID.ToString(), ActiveLogin.LoginID, 0, "RateProfileController LoopRates");
                    }

                    // This is very hacky.  After a certain point we want to rename Detail to Aluminum
                    if (profile.EstimateID == 0 || ManualEntryDetail.UseAluminum(profile.EstimateID))
                    {
                        RateLabor rateLabor = model.LaborRates.FirstOrDefault(o => o.RateType == 14);
                        if (rateLabor != null)
                        {
                            rateLabor.Name = "Aluminum";
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, ActiveLogin.LoginID, 0, "RateProfileController Rates");
            }

            ViewBag.RateProfileNavID = 0;

            // Spanish translation
            string cultureName = CultureHelper.GetCurrentCulture();
            if (cultureName == "es")
            {
                // Labor Rates
                foreach (RateLabor rateLabor in model.LaborRates)
                {
                    rateLabor.Name = ProStrings.ResourceManager.GetString(rateLabor.Name) ?? rateLabor.Name;
                }

                // Other Rates
                foreach (RatePart ratePart in model.OtherRates)
                {
                    ratePart.Name = ProStrings.ResourceManager.GetString(ratePart.Name) ?? ratePart.Name;
                }

                // Supplies Rates
                foreach (RateLabor rateLabor in model.SuppliesRates)
                {
                    rateLabor.Name = ProStrings.ResourceManager.GetString(rateLabor.Name) ?? rateLabor.Name;
                }

                // Part Rates
                foreach (RatePart ratePart in model.PartsRates)
                {
                    ratePart.Name = ProStrings.ResourceManager.GetString(ratePart.Name) ?? ratePart.Name;
                }
            }

            return View(model);
        }

        private List<SimpleListItem> GetLaborDropDown(int rateType)
        {
            List<SimpleListItem> dropDownList = null;

            // Most drop down lists allow 
            if (rateType == 21)
            {
                // Clearcoat can only be included in Paint
                dropDownList = new List<SimpleListItem>() { new SimpleListItem("", "0"), new SimpleListItem("Paint", "2") };
            }
            else if (rateType == 2)
            {
                // Paint has no drop down
            }
            else
            {
                // The Extra labor types can be included in any other extra labor type.
                dropDownList = GetLaborList(new List<string>() { rateType.ToString() });
            }

            return dropDownList;
        }        

        [HttpPost]
        [Route("{userID}/rates/{profileID}")]
        public ActionResult Rates(RateVM model)
        {
            bool estimateLocked = false;
            if (model.EstimateID > 0)
            {
                Estimate estimate = new Estimate(model.EstimateID);
                estimateLocked = estimate.IsLocked();
            }

            if (model.EstimateID == 0 || !estimateLocked)
            {
                // Make sure there aren't any 2 level deep includes.
                foreach (RateLabor rate in model.LaborRates)
                {
                    if (rate.IncludeLabor != 0)
                    {
                        RateLabor otherRate = model.LaborRates.FirstOrDefault(o => o.RateType == rate.IncludeLabor);
                        if (otherRate != null && otherRate.IncludeLabor != 0)
                        {
                            return ReturnRatesWithMessage(model, "Included labor categories cannot be nested.  " + rate.Name + " is included in " + otherRate.Name + " so " + otherRate.Name + " cannot be included in another category.");
                        }
                    }
                }

                // Save changes to the base profile record
                RateProfile profile = RateProfile.Get(model.ProfileID);
                profile.Name = model.ProfileName;
                profile.Description = model.Description;
                profile.CapRatesAfterInclude = model.CapRatesAfterInclude;
                profile.CapSuppliesAfterInclude = model.CapSuppliesAfterInclude;
                profile.IsFullEstimateDiscountMarkup = model.IsFullEstimateDiscountMarkup;
                profile.FullEstimateDiscountMarkupValue = model.FullEstimateDiscountMarkupValue;

                // Keep any changes to CC fee or the flag in sync with the estimate (shown on Details page)
                if (profile.EstimateID > 0 &&
                    (profile.CreditCardFeePercentage != model.CreditCardFeePercentage ||
                    profile.ApplyCreditCardFee != model.ApplyCreditCardFee ||
                    profile.TaxedCreditCardFee != model.TaxedCreditCardFee))
                {
                    var estimate = new Estimate(profile.EstimateID);
                    if (estimate != null)
                    {
                        estimate.CreditCardFeePercentage = model.CreditCardFeePercentage;
                        estimate.ApplyCreditCardFee = model.ApplyCreditCardFee;
                        estimate.TaxedCreditCardFee = model.TaxedCreditCardFee;
                        estimate.Save(ActiveLogin.ID);
                    }
                }

                profile.CreditCardFeePercentage = model.CreditCardFeePercentage;
                profile.ApplyCreditCardFee = model.ApplyCreditCardFee;
                profile.TaxedCreditCardFee = model.TaxedCreditCardFee;
                profile.Save(ActiveLogin.ID, ActiveLogin.LoginID);

                List<Rate> rates = Rate.GetForProfile(model.ProfileID);
                foreach (RateLabor laborRate in model.LaborRates)
                {
                    Rate rate = rates.FirstOrDefault(o => o.ID == laborRate.ID);
                    if (rate != null)
                    {
                        laborRate.FillRate(rate, model.RateCapSelectionCancelled);
                        rate.Save(ActiveLogin.ID, ActiveLogin.LoginID, model.ProfileName + " Labor " + laborRate.Name);
                    }
                }

                foreach (RateLabor laborRate in model.SuppliesRates)
                {
                    Rate rate = rates.FirstOrDefault(o => o.ID == laborRate.ID);
                    if (rate != null)
                    {
                        laborRate.FillRate(rate, model.RateCapSelectionCancelled);
                        rate.Save(ActiveLogin.ID, ActiveLogin.LoginID, model.ProfileName + " Supplies " + laborRate.Name);
                    }
                }

                foreach (RatePart partRate in model.PartsRates)
                {
                    Rate rate = rates.FirstOrDefault(o => o.ID == partRate.ID);
                    if (rate != null)
                    {
                        partRate.FillRate(rate, model.RateCapSelectionCancelled);
                        rate.Save(ActiveLogin.ID, ActiveLogin.LoginID, model.ProfileName + " Parts " + partRate.Name);
                    }

                }

                foreach (RatePart partRate in model.OtherRates)
                {
                    Rate rate = rates.FirstOrDefault(o => o.ID == partRate.ID);
                    if (rate != null)
                    {
                        partRate.FillRate(rate, model.RateCapSelectionCancelled);
                        rate.Save(ActiveLogin.ID, ActiveLogin.LoginID, model.ProfileName + " Other " + partRate.Name);
                    }
                }

                UpdateEstimateTotals(model.ProfileID);

                SuccessBoxFeatureLog.LogFeature(model.LoginID, SuccessBoxModule.RateProfile, "RateProfile_Update", ActiveLogin.ID);
            }

            return DoRedirect("/" + ActiveLogin.SiteUserID + "/rates/" + model.ProfileID);
        }

        private ViewResult ReturnRatesWithMessage(RateVM model, string message)
        {
            model.ErrorMessage = message;
            model.ProfileIsValid = true;

            ViewBag.NavID = "rates";
            ViewBag.RateProfileNavID = 0;

            // The drop down data didn't survive the post back, refil them
            foreach (RateLabor laborItem in model.LaborRates)
            {
                laborItem.DropDownList = GetLaborDropDown(laborItem.RateType);
            }

            return View(model);
        }

        #endregion

        #region Paint Finish

        [HttpGet]
        [Route("{userID}/paint-finish/{profileID}")]
        public ActionResult PaintFinish(int userID, int profileID)
        {
            RateProfile profile = RateProfile.Get(profileID);

            PaintFinishVM model = new PaintFinishVM();
            model.LoginID = ActiveLogin.LoginID;

            if (profile != null)
            {
                ViewBag.EstimateID = profile.EstimateID;

                // Hilight the left menu button if global, the top "edit rate profile" button if an estimate's profile
                if (profile.EstimateID == 0)
                {
                    ViewBag.NavID = "rates";
                    model.IsGlobalProfile = true;
                }
                else
                {
                    ViewBag.EstimateNavID = "profile";
                    model.IsGlobalProfile = false;

                    Estimate estimate = new Estimate(profile.EstimateID);
                    model.EstimateIsLocked = estimate.IsLocked();
                }

                model.ProfileIsValid = true;

                model.Description = profile.Description;
                model.ProfileName = profile.Name;
                model.ProfileDefault = profile.IsDefault;
                model.PresetsDefault = profile.IsDefaultPresets;
                model.ProfileID = profileID;
                model.EstimateID = profile.EstimateID;

                LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
                model.UseDefaultProfile = loginInfo.UseDefaultRateProfile;

                PaintSettings paintRates = PaintSettings.GetForProfile(profileID);
                if (paintRates != null)
                {
                    model.paintProfile = paintRates;
                    model.AllowDeduction = Convert.ToBoolean(model.paintProfile.AllowDeductions);
                    model.EdgeInterior = Convert.ToBoolean(model.paintProfile.EdgeInteriorTimes);
                    model.ThreeStageInner = Convert.ToBoolean(model.paintProfile.ThreeStageInner);
                    model.TwoToneInner = Convert.ToBoolean(model.paintProfile.TwoToneInner);
                    model.ThreeStagePillars = Convert.ToBoolean(model.paintProfile.ThreeStagePillars);
                    model.TwoTonePillars = Convert.ToBoolean(model.paintProfile.TwoTonePillars);
                    model.ThreeStateInterior = Convert.ToBoolean(model.paintProfile.ThreeStateInterior);
                    model.TwoToneInterior = Convert.ToBoolean(model.paintProfile.TwoToneInterior);
                }
            }

            ViewBag.RateProfileNavID = 1;

            TempData.Keep("profileName");
            return View(model);
        }

        [HttpPost]
        [Route("{userID}/paint-finish/{profileID}")]
        public ActionResult PaintFinish(PaintFinishVM model)
        {
            bool estimateLocked = false;
            if (model.EstimateID > 0)
            {
                Estimate estimate = new Estimate(model.EstimateID);
                estimateLocked = estimate.IsLocked();
            }

            if (model.EstimateID == 0 || !estimateLocked)
            {
                if (ModelState.IsValid)
                {
                    PaintSettings paintRates = PaintSettings.GetForProfile(model.ProfileID);
                    if (paintRates != null)
                    {
                        paintRates.AllowDeductions = model.AllowDeduction;
                        paintRates.EdgeInteriorTimes = model.EdgeInterior;
                        paintRates.ThreeStageInner = model.ThreeStageInner;
                        paintRates.ThreeStagePillars = model.ThreeStagePillars;
                        paintRates.ThreeStateInterior = model.ThreeStateInterior;
                        paintRates.TwoToneInner = model.TwoToneInner;
                        paintRates.TwoTonePillars = model.TwoTonePillars;
                        paintRates.TwoToneInterior = model.TwoToneInterior;
                        paintRates.ManualPaintOverlap = model.paintProfile.ManualPaintOverlap;
                        paintRates.AutomaticOverlap = model.paintProfile.AutomaticOverlap; //
                        paintRates.AdjacentDeduction = model.paintProfile.AdjacentDeduction;
                        paintRates.NonAdjacentDeduction = model.paintProfile.NonAdjacentDeduction;
                        paintRates.MajorClearCoat = model.paintProfile.MajorClearCoat;
                        paintRates.MajorThreeStage = model.paintProfile.MajorThreeStage;
                        paintRates.MajorTwoTone = model.paintProfile.MajorTwoTone;
                        paintRates.OverlapClearCoat = model.paintProfile.OverlapClearCoat;
                        paintRates.OverlapThreeStage = model.paintProfile.OverlapThreeStage;
                        paintRates.OverLapTwoTone = model.paintProfile.OverLapTwoTone;
                        paintRates.Blend = model.paintProfile.Blend;
                        paintRates.Underside = model.paintProfile.Underside;
                        paintRates.Edging = model.paintProfile.Edging;
                        paintRates.ThreeTwoBlend = model.paintProfile.ThreeTwoBlend;

                        paintRates.Save(ActiveLogin.ID, ActiveLogin.LoginID);

                        UpdateEstimateTotals(model.ProfileID);
                    }
                }
            }
            return DoRedirect("/" + ActiveLogin.SiteUserID + "/paint-finish/" + model.ProfileID);
        }

        #endregion

        #region Print Options

        [HttpGet]
        [Route("{userID}/print-options/{profileID}")]
        public ActionResult PrintingOptions(int userID, int profileID)
        {
            RateProfile profile = RateProfile.Get(profileID);

            PrintVM model = new PrintVM();
            if (profile != null)
            {
                model.LoginID = ActiveLogin.LoginID;
                ViewBag.EstimateID = profile.EstimateID;

                // Hilight the left menu button if global, the top "edit rate profile" button if an estimate's profile
                if (profile.EstimateID == 0)
                {
                    ViewBag.NavID = "rates";
                    model.IsGlobalProfile = true;
                }
                else
                {
                    ViewBag.EstimateNavID = "profile";
                    model.IsGlobalProfile = false;

                    Estimate estimate = new Estimate(profile.EstimateID);
                    model.EstimateIsLocked = estimate.IsLocked();
                }

                model.ProfileIsValid = true;
                model.Description = profile.Description;
                model.ProfileName = profile.Name;
                model.ProfileDefault = profile.IsDefault;
                model.PresetsDefault = profile.IsDefaultPresets;
                model.ProfileID = profileID;
                model.EstimateID = profile.EstimateID;

                PrintSettings printSettings = PrintSettings.GetForProfile(profileID);
                if (printSettings != null)
                {
                    model.AdminInfoID = printSettings.AdminInfoID;
                    if (printSettings.ContactOption != null)
                    {
                        model.ContactOption = printSettings.ContactOption;
                    }
                    else
                    {
                        model.ContactOption = "Label";
                    }

                    model.Dollars = Convert.ToBoolean(printSettings.Dollars);
                    model.EstimateNumber = printSettings.EstimateNumber;
                    model.FooterText = printSettings.FooterText;
                    model.GraphicsQuality = printSettings.GraphicsQuality;
                    model.GreyBars = Convert.ToBoolean(printSettings.GreyBars);
                    model.ID = printSettings.ID;
                    model.NoHeaderLogo = Convert.ToBoolean(printSettings.NoHeaderLogo);
                    model.NoInsuranceSection = Convert.ToBoolean(printSettings.NoInsuranceSection);
                    model.NoPhotos = Convert.ToBoolean(printSettings.NoPhotos);
                    model.NoVehicleAccessories = printSettings.NoVehicleAccessories == "1";
                    model.OrderBy = printSettings.OrderBy;
                    model.PrintPaymentInfo = Convert.ToBoolean(printSettings.PrintPaymentInfo);
                    model.PrintPrivateNotes = Convert.ToBoolean(printSettings.PrintPrivateNotes);
                    model.PrintPublicNotes = Convert.ToBoolean(printSettings.PrintPublicNotes);
                    model.SeparateLabor = Convert.ToBoolean(printSettings.SeparateLabor);
                    model.SupplementOption = printSettings.SupplementOption;
                    model.PrintEstimator = Convert.ToBoolean(printSettings.PrintEstimator);
                    model.PrintVendors = Convert.ToBoolean(printSettings.PrintVendors);
                    model.PrintTechnicians = Convert.ToBoolean(printSettings.PrintTechnicians);
                    model.NoFooterDateTimeStamp = Convert.ToBoolean(printSettings.NoFooterDateTimeStamp);
                }
            }

            LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
            model.UseDefaultProfile = loginInfo.UseDefaultRateProfile;

            ViewBag.RateProfileNavID = 2;

            TempData.Keep("profileName");
            return View(model);
        }

        [HttpPost]
        [Route("{userID}/print-options/{profileID}")]
        public ActionResult PrintingOptions(PrintVM model)
        {
            bool estimateLocked = false;
            if (model.EstimateID > 0)
            {
                Estimate estimate = new Estimate(model.EstimateID);
                estimateLocked = estimate.IsLocked();
            }

            if (model.EstimateID == 0 || !estimateLocked)
            {
                if (ModelState.IsValid)
                {
                    PrintSettings printSettings = PrintSettings.GetForProfile(model.ProfileID);
                    if (printSettings != null)
                    {
                        printSettings.GraphicsQuality = model.GraphicsQuality;
                        printSettings.NoHeaderLogo = model.NoHeaderLogo;
                        printSettings.NoPhotos = model.NoPhotos;
                        printSettings.NoInsuranceSection = model.NoInsuranceSection;
                        printSettings.NoVehicleAccessories = model.NoVehicleAccessories ? "1" : "0";
                        printSettings.FooterText = model.FooterText == null ? "" : model.FooterText.Replace(":o", "");// When pasting from Word, some tags have o:
                        printSettings.PrintPrivateNotes = model.PrintPrivateNotes;
                        printSettings.PrintPublicNotes = model.PrintPublicNotes;
                        printSettings.ContactOption = model.ContactOption;
                        printSettings.OrderBy = "Step";
                        printSettings.SeparateLabor = model.SeparateLabor;
                        printSettings.EstimateNumber = model.EstimateNumber;
                        printSettings.AdminInfoID = model.AdminInfoID;
                        printSettings.PrintPaymentInfo = model.PrintPaymentInfo;
                        printSettings.Dollars = model.Dollars;
                        printSettings.GreyBars = model.GreyBars;
                        printSettings.PrintEstimator = model.PrintEstimator;
                        printSettings.PrintVendors = model.PrintVendors;
                        printSettings.PrintTechnicians = model.PrintTechnicians;
                        printSettings.NoFooterDateTimeStamp = model.NoFooterDateTimeStamp;

                        printSettings.Save(ActiveLogin.ID, ActiveLogin.LoginID);

                        UpdateEstimateTotals(model.ProfileID);
                    }
                }
            }
            return DoRedirect("/" + ActiveLogin.SiteUserID + "/print-options/" + model.ProfileID);
        }

        #endregion

        #region Taxes

        [HttpGet]
        [Route("{userID}/taxes/{profileID}")]
        public ActionResult Taxes(int userID, int profileID)
        {
            ViewBag.RateProfileNavID = 3;
            TempData.Keep("profileName");

            RateProfile profile = RateProfile.Get(profileID);

            TaxesVM model = new TaxesVM();
            if (profile != null)
            {
                ViewBag.EstimateID = profile.EstimateID;

                // Hilight the left menu button if global, the top "edit rate profile" button if an estimate's profile
                if (profile.EstimateID == 0)
                {
                    ViewBag.NavID = "rates";
                    model.IsGlobalProfile = true;
                }
                else
                {
                    ViewBag.EstimateNavID = "profile";
                    model.IsGlobalProfile = false;

                    Estimate estimate = new Estimate(profile.EstimateID);
                    model.EstimateIsLocked = estimate.IsLocked();
                }

                model.ProfileIsValid = true;
                model.Description = profile.Description;
                model.ProfileName = profile.Name;
                model.ProfileDefault = profile.IsDefault;
                model.PresetsDefault = profile.IsDefaultPresets;
                model.ProfileID = profileID;
                model.EstimateID = profile.EstimateID;

                MiscSettings miscSettings = MiscSettings.GetForProfile(profileID);
                if (miscSettings != null)
                {
                    model.ACChargeAmount = miscSettings.ACChargeAmount;
                    model.ChargeForAimingHeadlights = miscSettings.ChargeForAimingHeadlights;
                    model.ChargeForPowerUnits = miscSettings.ChargeForPowerUnits;
                    model.ChargeForRefrigRecovery = miscSettings.ChargeForRefrigRecovery;
                    model.DoNotMarkChanges = miscSettings.DoNotMarkChanges;
                    model.ID = miscSettings.ID;
                    model.IncludeStructureInBody = miscSettings.IncludeStructureInBody;
                    model.LKQText = miscSettings.LKQText;
                    model.SecondTaxRate = miscSettings.SecondTaxRate.ToString();
                    model.SecondTaxRateStart = miscSettings.SecondTaxRateStart.ToString();
                    model.SuppLevel = miscSettings.SuppLevel;
                    model.SuppressAddRelatedPrompt = miscSettings.SuppressAddRelatedPrompt;
                    model.TaxRate = miscSettings.TaxRate.ToString();
                    model.TotalLossPerc = miscSettings.TotalLossPerc.ToString();
                    model.TaxLaborAndPartsSeparately = miscSettings.UseSepPartLaborTax;
                    model.PartsTaxRate = miscSettings.PartTax.ToString();
                }
            }

            LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
            model.UseDefaultProfile = loginInfo.UseDefaultRateProfile;

            return View(model);
        }

        [HttpPost]
        [Route("{userID}/taxes/{profileID}")]
        public ActionResult Taxes(TaxesVM model)
        {
            bool estimateLocked = false;
            if (model.EstimateID > 0)
            {
                Estimate estimate = new Estimate(model.EstimateID);
                estimateLocked = estimate.IsLocked();
            }

            if (model.EstimateID == 0 || !estimateLocked)
            {
                if (ModelState.IsValid)
                {
                    MiscSettings miscSettings = MiscSettings.GetForProfile(model.ProfileID);

                    if (miscSettings == null)
                    {
                        miscSettings = new MiscSettings();
                        miscSettings.CustomerProfilesID = model.ProfileID;
                    }

                    miscSettings.TaxRate = InputHelper.GetDouble(model.TaxRate);
                    miscSettings.SecondTaxRateStart = InputHelper.GetDouble(model.SecondTaxRateStart);
                    miscSettings.SecondTaxRate = InputHelper.GetDouble(model.SecondTaxRate);
                    miscSettings.ACChargeAmount = model.ACChargeAmount;
                    miscSettings.LKQText = model.LKQText;
                    miscSettings.TotalLossPerc = InputHelper.GetDouble(model.TotalLossPerc);
                    miscSettings.ChargeForAimingHeadlights = model.ChargeForAimingHeadlights;
                    miscSettings.ChargeForPowerUnits = model.ChargeForPowerUnits;
                    miscSettings.ChargeForRefrigRecovery = model.ChargeForRefrigRecovery;
                    miscSettings.SuppressAddRelatedPrompt = model.SuppressAddRelatedPrompt;
                    miscSettings.UseSepPartLaborTax = model.TaxLaborAndPartsSeparately;
                    miscSettings.PartTax = InputHelper.GetDouble(model.PartsTaxRate);

                    miscSettings.Save();

                    UpdateEstimateTotals(model.ProfileID);
                }
            }

            return DoRedirect("/" + ActiveLogin.SiteUserID + "/taxes/" + model.ProfileID);
        }

        #endregion

        #region Preset Charges

        [HttpGet]
        [Route("{userID}/preset-charges/{profileID}")]
        public ActionResult PresetCharges(int userID, int profileID)
        {
            ViewBag.RateProfileNavID = 4;
            TempData.Keep("profileName");

            RateProfile profile = RateProfile.Get(profileID);

            PresetChargesVM model = new PresetChargesVM();
            model.LoginID = ActiveLogin.LoginID;

            if (profile != null)
            {
                ViewBag.EstimateID = profile.EstimateID;
                ViewBag.ProfileID = profileID;
                bool hasSupplement = false;

                // Hilight the left menu button if global, the top "edit rate profile" button if an estimate's profile
                if (profile.EstimateID == 0)
                {
                    ViewBag.NavID = "rates";
                    model.IsGlobalProfile = true;
                }
                else
                {
                    ViewBag.EstimateNavID = "profile";
                    model.IsGlobalProfile = false;

                    Estimate estimate = new Estimate(profile.EstimateID);
                    model.EstimateIsLocked = estimate.IsLocked();
                    hasSupplement = estimate.LockLevel > 0;
                }

                model.ProfileIsValid = true;
                model.Description = profile.Description;
                model.ProfileName = profile.Name;
                model.ProfileDefault = profile.IsDefault;
                model.PresetsDefault = profile.IsDefaultPresets;
                model.ProfileID = profileID;
                model.EstimateID = profile.EstimateID;

                Session["CurrentProfileDefault"] = model.ProfileDefault;
                Session["CurrentPresetsDefault"] = model.PresetsDefault;
                Session["CurrentProfileName"] = model.ProfileName;
                Session["CurrentProfileDescription"] = model.Description;
                Session["CurrentProfileID"] = profileID;

                ManualEntry me = new ManualEntry();
                me.List = new ManualEntryList();
                me.List.ItemID = profileID;
                me.List.MEMode = "Preset";
                me.List.EstimateIsLocked = false;
                me.List.HasSupplement = hasSupplement;
                me.Details = new ManualEntryDetail(ManualEntryDetail.UseAluminum(profile.EstimateID));
                me.Details.Quantity = 1;
                me.Details.LineID = 0;
                me.Details.TheID = profileID;
                me.Details.Action = "Add";
                me.Details.MEMode = "Preset";

                me.Details.SectionList = _estimateService.GetSections(0, 0);

                FillGainValues(me.Details.PaintGainValues, profileID);

                model.ManualEntry = me;
            }

            LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
            model.UseDefaultProfile = loginInfo.UseDefaultRateProfile;

            return View(model);
        }

        private void FillGainValues(PaintGainValues paintGainValues, int profileID)
        {
            PaintSettings paintRates = PaintSettings.GetForProfile(profileID);

            paintGainValues.PaintGain_Blend = paintRates.Blend / 100;
            paintGainValues.PaintGain_ThreeTwoBlend = paintRates.ThreeTwoBlend / 100;
            paintGainValues.PaintGain_Underside = paintRates.Underside / 100;
            paintGainValues.PaintGain_EdgingMin = paintRates.Edging;
            paintGainValues.PaintGain_2ToneMajor = paintRates.MajorTwoTone / 100;
            paintGainValues.PaintGain_2ToneNonAdjacent = paintRates.OverLapTwoTone / 100;
            paintGainValues.PaintGain_3StageMajor = paintRates.MajorThreeStage / 100;
            paintGainValues.PaintGain_3StageNonAdjacent = paintRates.OverlapThreeStage / 100;
            paintGainValues.PaintGain_ClearCoatMajor = paintRates.MajorClearCoat / 100;
            paintGainValues.PaintGain_ClearCoatNonAdj = paintRates.OverlapClearCoat / 100;
        }

        [HttpPost]
        [Route("{userID}/preset-charges/{profileID}")]
        public ActionResult PresetCharges(PresetChargesVM model)
        {
            bool estimateLocked = false;
            if (model.EstimateID > 0)
            {
                Estimate estimate = new Estimate(model.EstimateID);
                estimateLocked = estimate.IsLocked();
            }

            if (model.EstimateID == 0 || !estimateLocked)
            {
                ManualEntryDetail detail = model.ManualEntry.Details;

                string Action = "Add";
                int TheID = model.ProfileID;
                if (detail.LineID > 0)
                {
                    Action = "Update";
                    TheID = detail.LineID;
                }
                Helpers.ManualEntryHelper.SaveManualEntry(model.EstimateID, detail, "Preset", Action, TheID, ActiveLogin.ID, ActiveLogin.LoginID);

                UpdateEstimateTotals(model.ProfileID);
            }
            return DoRedirect("/" + ActiveLogin.SiteUserID + "/preset-charges/" + model.ProfileID);
        }

        #endregion

        #region Create and Delete profiles

        public ActionResult CreateRateProfile(int userID)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            int newID = RateProfileManager.CreateBlankRateProfile(activeLogin.LoginID);

            SuccessBoxFeatureLog.LogFeature(activeLogin.LoginID, SuccessBoxModule.RateProfile, "RateProfile_Create", activeLogin.ID);

            return Redirect("/" + activeLogin.SiteUserID + "/rates/" + newID);
        }

        public ActionResult CopyFromDefault(int userID)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            int newID = RateProfileManager.CreateDefaultCopy(activeLogin.LoginID);

            SuccessBoxFeatureLog.LogFeature(activeLogin.LoginID, SuccessBoxModule.RateProfile, "RateProfile_Create", activeLogin.ID);

            return Redirect("/" + activeLogin.SiteUserID + "/rates/" + newID);
        }

        public ActionResult CopyRateProfile(int userID, int profileID)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            int newID = RateProfileManager.CopyProfile(activeLogin.LoginID, profileID);
            return Redirect("/" + activeLogin.SiteUserID + "/rates/" + newID);
        }

        public JsonResult DeleteRateProfile(int userID, int profileID)
        {
            CacheActiveLoginID(userID);

            FunctionResult result = new FunctionResult();

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                // Get all default rate profiles for the logged in user
                RateProfile profile = RateProfile.Get(profileID);
                List<RateProfile> allProfiles = RateProfile.GetAllForLogin(activeLogin.LoginID);

                if (allProfiles.Count == 1)
                {
                    result.ErrorMessage = @Proestimator.Resources.ProStrings.CannotDeleteOnlyProfile;
                }
                else if (profile == null)
                {
                    result.ErrorMessage = @Proestimator.Resources.ProStrings.ProfileNotFound;
                }
                else if (profile.IsDefault || profile.IsDefaultPresets)
                {
                    result.ErrorMessage = @Proestimator.Resources.ProStrings.CannotDeleteDefaultProfilePreset;
                }
                else if (profile.LoginID != activeLogin.LoginID)
                {
                    result.ErrorMessage = @Proestimator.Resources.ProStrings.InvalidRateProfile;
                }
                else
                {
                    RateProfileManager.DeleteProfile(profileID);
                }

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    result.Success = false;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region View Deleted Profiles

        public JsonResult RestoreRateProfile(int userID, int profileID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                RateProfile rateProfile = RateProfile.Get(profileID);
                rateProfile.IsDeleted = false;
                SaveResult saveResult = rateProfile.Save();

                return Json(saveResult, JsonRequestBehavior.AllowGet);
            }

            return Json(new FunctionResult("Unauthorized Login"), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Presets

        protected void UpdateEstimateTotals(int profileID)
        {
            RateProfile profile = RateProfile.Get(profileID);
            if (profile != null && profile.EstimateID > 0)
            {
                Estimate.RefreshProcessedLines(profile.EstimateID);
            }
        }
        
        public JsonResult GetPresetME(int userID, int estimateID, int PresetID, string ts)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                Estimate estimate = new Estimate(estimateID);
                ManualEntryDetail detail = Helpers.ManualEntryHelper.GetPresetLine(_vendorRepository, estimate.CreatedByLoginID, PresetID);

                // The SectionID will be an ID of a PDR_Panel, which we also use now for Manual car estimates and presets.
                // This section ID will not be in the sections list because the IDs are from different tables.  We need to get the PDR panel name
                // and find the first ID in the sections list that matches
                int sectionID = InputHelper.GetInteger(detail.SelectedSection);
                if (sectionID > 0)
                {
                    ProEstimatorData.DataModel.PDR.PDR_Panel pdrPanel = ProEstimatorData.DataModel.PDR.PDR_Panel.GetByID(sectionID);
                    if (pdrPanel != null)
                    {
                        string panelName = pdrPanel.PanelName.ToLower().Replace("right ", "").Replace("left ", "");

                        Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimateID);
                        if (vehicle != null)
                        {
                            List<SimpleListItem> sectionsList = _estimateService.GetSections(0, vehicle.VehicleID);
                            SimpleListItem firstMatch = sectionsList.FirstOrDefault(o => o.Text.ToLower().Contains(panelName));

                            if (firstMatch != null)
                            {
                                detail.SelectedSection = firstMatch.Value;
                            }
                        }
                    }

                }

                return Json(new { detail = detail }, JsonRequestBehavior.AllowGet);
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditME(int userID, int estimateID, int lineID, string meMode)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                ManualEntryDetail detail = Helpers.ManualEntryHelper.GetMELine(_vendorRepository, estimateID, lineID, meMode);
                return Json(new { detail = detail }, JsonRequestBehavior.AllowGet);
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteME(int userID, int estimateID, int lineID, string meMode)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                if (estimateID == 0)
                {
                    Helpers.ManualEntryHelper.DeleteLineItem(lineID, meMode, false);
                }
                else
                {
                    Estimate estimate = new Estimate(estimateID);

                    if (estimate != null && !estimate.IsLocked())
                    {
                        Helpers.ManualEntryHelper.DeleteLineItem(lineID, meMode, estimate.LockLevel > 0);

                        Estimate.RefreshProcessedLines(estimateID);

                        SiteActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                        if (activeLogin != null)
                        {
                            _proAdvisorService.RefreshTotalForEstimate(estimate.EstimateID);
                        }
                    }
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetDefaultRateProfile(int userID, int profileID)
        {
            CacheActiveLoginID(userID);

            FunctionResult result = new FunctionResult();

            if (IsUserAuthorized(userID))
            {
                try
                {
                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                    RateProfile profile = RateProfile.Get(profileID);
                    if (profile != null && profile.LoginID == activeLogin.LoginID)
                    {
                        profile.SetAsDefaultProfile();
                        result.Success = true;
                        result.ErrorMessage = Proestimator.Resources.ProStrings.Message_DefaultRateProfileChanged;
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorMessage = Proestimator.Resources.ProStrings.Message_InvalidProfileID;
                    }
                }
                catch (System.Exception ex)
                {
                    result.Success = false;

                    result.ErrorMessage = Proestimator.Resources.ProStrings.Message_ErrorSettingDefaultProfileID + ": " + ex.Message;
                    if (ex.InnerException != null)
                    {
                        result.ErrorMessage += Environment.NewLine + ex.InnerException.Message;
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetDefaultRatePreset(int userID, int profileID)
        {
            CacheActiveLoginID(userID);

            FunctionResult result = new FunctionResult();

            if (IsUserAuthorized(userID))
            {
                try
                {
                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                    RateProfile profile = RateProfile.Get(profileID);
                    if (profile != null && profile.LoginID == activeLogin.LoginID)
                    {
                        profile.IsDefaultPresets = !profile.IsDefaultPresets;
                        SaveResult saveResult = profile.Save();
                        
                        result.Success = saveResult.Success;
                        result.ErrorMessage = Proestimator.Resources.ProStrings.Message_DefaultRatePresetChanged;
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorMessage = Proestimator.Resources.ProStrings.Message_InvalidProfileID;
                    }
                }
                catch (System.Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = Proestimator.Resources.ProStrings.Message_ErrorSettingDefaultProfileID + ": " + ex.Message;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ToggleUseDefaultProfile(int userID)
        {
            CacheActiveLoginID(userID);

            ToggleUseDefaultProfileResult result = new ToggleUseDefaultProfileResult();

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                LoginInfo loginInfo = LoginInfo.GetByID(activeLogin.LoginID);
                loginInfo.UseDefaultRateProfile = !loginInfo.UseDefaultRateProfile;
                SaveResult saveResult = loginInfo.Save();

                if (saveResult.Success)
                {
                    result.Success = true;
                    result.UseDefaultProfile = loginInfo.UseDefaultRateProfile;
                }
                else
                {
                    result.Success = false;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public Boolean ReOrderCustomerProfilePresets(List<string> data, int profileID, string meMode)
        {
            string mode = (meMode == "Preset" ? "Presets" : "LineItems");
            
            try
            {
                List<ManualEntryListItem> list = ManualEntryHelper.getManualEntryList(mode, profileID);
                int index = 0;
                foreach (var estimatorId in data)
                {
                    ManualEntryListItem manualEntryListItem = list.FirstOrDefault(x => x.ID == int.Parse(estimatorId));
                    if (manualEntryListItem != null)
                    {
                        manualEntryListItem.OrderNumber = index++;
                        ManualEntryHelper.UpdateManualEntryReorder(manualEntryListItem);
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public class ToggleUseDefaultProfileResult
        {
            public bool Success { get; set; }
            public bool UseDefaultProfile { get; set; }
        }        

        private static List<SimpleListItem> _suppliesDropDownItems = new List<SimpleListItem>()
        {
            new SimpleListItem("","0"),
            new SimpleListItem("Body", "6"),
            new SimpleListItem("Paint", "7"),
            new SimpleListItem("Clearcoat", "22")
        };

        private List<SimpleListItem> GetSuppliesList(List<string> exceptions)
        {
            return _suppliesDropDownItems.Where(o => !exceptions.Contains(o.Value)).ToList();
        }

        private static List<SimpleListItem> _laborDropDownItems = new List<SimpleListItem>() 
        {
            new SimpleListItem("", "0"),
            new SimpleListItem("Body", "1"),
            new SimpleListItem("Mechanical", "3"),
            new SimpleListItem("Frame", "4"),
            new SimpleListItem("Structure", "5"),
            new SimpleListItem("Electrical", "12"),
            new SimpleListItem("Glass", "20"),
            new SimpleListItem("Detail", "14"),
            new SimpleListItem("Cleanup", "15"),
            new SimpleListItem("Other", "16")
        };

        private List<SimpleListItem> GetLaborList(List<string> exceptions)
        {
            return _laborDropDownItems.Where(o => !exceptions.Contains(o.Value)).ToList();
        }

        public ActionResult GetHistoryReport(
              [DataSourceRequest] DataSourceRequest request
            , int userID
            , string rateProfile
            , int id
        )
        {
            List<ChangeLogRowSummary> gridRows = new List<ChangeLogRowSummary>();

            if (IsUserAuthorized(userID))
            {
                if(rateProfile == "rateProfile")
                {
                    RateProfile profile = RateProfile.Get(id);
                    if (profile != null)
                    {
                        gridRows = profile.GetHistory();
                    }
                }
                else if(rateProfile == "pdrProfile")
                {
                    PDR_RateProfile profile = PDR_RateProfile.GetByID(id);
                    if (profile != null)
                    {
                        gridRows = profile.GetHistory();
                    }
                }
                else if(rateProfile == "addonProfile")
                {
                    ProAdvisorPresetProfile profile = _proAdvisorProfileService.GetProfile(id);
                    if (profile != null)
                    {
                        gridRows = profile.GetHistory();
                    }
                }
            }

            return Json(gridRows.ToDataSourceResult(request));
        }

        public ActionResult GetChangeLogDetailsGrid([DataSourceRequest] DataSourceRequest request, int changeLogID)
        {
            List<ChangeLogItem> details = new List<ChangeLogItem>();
            if (changeLogID > 0)
            {
                details = ChangeLogItem.GetForChangeLog(changeLogID);
            }

            return Json(details.ToDataSourceResult(request));
        }

        public JsonResult GetChangeLogDetails(int changeLogID)
        {
            ChangeLogDetails vm = new ChangeLogDetails();

            ChangeLogItemDetails details = ChangeLogItemDetails.GetForID(changeLogID);
            vm.Browser = details.Browser;
            vm.ComputerKey = details.ComputerKey;
            vm.Device = details.DeviceType;
            vm.EmailAddress = details.EmailAddress;
            vm.SiteUser = details.Name;
            vm.TimeStamp = details.TimeStamp.ToShortDateString() + " " + details.TimeStamp.ToShortTimeString();

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        public class ChangeLogDetails
        {
            public string TimeStamp { get; set; }
            public string SiteUser { get; set; }
            public string EmailAddress { get; set; }
            public string Browser { get; set; }
            public string Device { get; set; }
            public string ComputerKey { get; set; }
        }
    }

        #endregion
}