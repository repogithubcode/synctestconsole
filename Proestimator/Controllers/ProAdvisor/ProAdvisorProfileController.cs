using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.ProAdvisor;

using ProEstimator.Business.Logic;

using Proestimator.ViewModel.Profiles;
using ProEstimator.DataRepositories.ProAdvisor;
using ProEstimator.Business.ProAdvisor;

namespace Proestimator.Controllers
{
    public class ProAdvisorProfileController : SiteController 
    {
        private IProAdvisorProfileService _proAdvisorProfileService;

        public ProAdvisorProfileController(IProAdvisorProfileService proAdvisorProfileService)
        {
            _proAdvisorProfileService = proAdvisorProfileService;
        }

        public ActionResult GetProfiles(
              [DataSourceRequest] DataSourceRequest request
            , int userID
            , bool showDeleted
        )
        {
            List<RateProfileGridRow> gridRows = new List<RateProfileGridRow>();

            if (IsUserAuthorized(userID))
            {
                RateProfileGridRow firstRow = CreateDefaultGridRow();
                gridRows.Add(firstRow);

                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                List<ProAdvisorPresetProfile> rateProfiles = _proAdvisorProfileService.GetAllProfilesForLogin(activeLogin.LoginID, showDeleted);

                foreach (ProAdvisorPresetProfile rateProfile in rateProfiles)
                {
                    gridRows.Add(new RateProfileGridRow(rateProfile));

                    if (rateProfile.DefaultFlag)
                    {
                        firstRow.DefaultProfile = false;
                    }
                }
            }

            return Json(gridRows.ToDataSourceResult(request));
        }

        private RateProfileGridRow CreateDefaultGridRow()
        {
            RateProfileGridRow firstRow = new RateProfileGridRow(1, "System Default");
            firstRow.DefaultProfile = true;

            return firstRow;
        }

        public ActionResult CopyRateProfile(int userID, int profileID)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            FunctionResultInt result = _proAdvisorProfileService.CopyProfile(profileID, activeLogin.LoginID, activeLogin.ID);

            if (result.Success)
            {
                return Redirect("/" + userID + "/pro-advisor/" + result.Value);
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
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            FunctionResultInt result = _proAdvisorProfileService.CopyProfile(1, activeLogin.LoginID, activeLogin.ID);

            if (result.Success)
            {
                return Redirect("/" + userID + "/pro-advisor/" + result.Value);
            }
            else
            {
                return Redirect("/" + userID + "/rates?error=" + result.ErrorMessage);
            }
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

                List<ProAdvisorPresetProfile> allProfiles = _proAdvisorProfileService.GetAllProfilesForLogin(activeLogin.LoginID, false);
                ProAdvisorPresetProfile defaultProfile = allProfiles.FirstOrDefault(o => o.DefaultFlag == true);

                if (defaultProfile == null)
                {
                    RateProfileGridRow firstRow = CreateDefaultGridRow();
                    return CopyRateProfile(userID, firstRow.ID);
                    //return Redirect("/" + userID + "/rates?error=No default profile set.");
                    //below works as well
                    //return RedirectToAction("rates", userID.ToString(), new { error = "No default profile set." });
                }
                else
                {
                    return CopyRateProfile(userID, defaultProfile.ID);
                }
            }

            return Redirect("/");
        }

        public JsonResult DeleteRateProfile(int userID, int profileID)
        {
            if (IsUserAuthorized(userID))
            {
                CacheActiveLoginID(userID);
                
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                FunctionResult result = _proAdvisorProfileService.DeleteRateProfile(activeLogin.ID, activeLogin.LoginID, userID, profileID);
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new FunctionResult("Unauthorized Login"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult RestoreRateProfile(int userID, int profileID)
        {
            if (IsUserAuthorized(userID))
            {
                CacheActiveLoginID(userID);

                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                ProAdvisorPresetProfile profile = _proAdvisorProfileService.GetProfile(profileID);
                if (profile != null)
                {
                    profile.Deleted = false;
                    profile.Save(activeLogin.ID);

                    return Json(new FunctionResult(), JsonRequestBehavior.AllowGet);
                }

                return Json(new FunctionResult("Profile not found."), JsonRequestBehavior.AllowGet);
            }

            return Json(new FunctionResult("Unauthorized Login"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetDefaultRateProfile(int userID, int profileID)
        {
            FunctionResult result = new FunctionResult();

            if (IsUserAuthorized(userID))
            {
                CacheActiveLoginID(userID);

                try
                {
                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                    result = _proAdvisorProfileService.SetDefaultProfile(activeLogin.LoginID, profileID);

                    if (result.Success)
                    {
                        result.ErrorMessage = Resources.ProStrings.Message_DefaultRateProfileChanged;
                    }
                }
                catch (System.Exception ex)
                {
                    result.Success = false;

                    result.ErrorMessage = Resources.ProStrings.Message_ErrorSettingDefaultProfileID + ": " + ex.Message;
                    if (ex.InnerException != null)
                    {
                        result.ErrorMessage += Environment.NewLine + ex.InnerException.Message;
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ToggleUseDefaultProfile(int userID)
        {
            ToggleUseDefaultProfileResult result = new ToggleUseDefaultProfileResult();

            if (IsUserAuthorized(userID))
            {
                CacheActiveLoginID(userID);

                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                bool useDefault = _proAdvisorProfileService.UseDefaultProfile(activeLogin.LoginID);
                _proAdvisorProfileService.SetUseDefaultProfile(activeLogin.ID, activeLogin.LoginID, !useDefault);

                result.Success = true;
                result.UseDefaultProfile = !useDefault;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public class ToggleUseDefaultProfileResult
        {
            public bool Success { get; set; }
            public bool UseDefaultProfile { get; set; }
        }    

        #region Add Ons

        [HttpGet]
        [Route("{userID}/pro-advisor/{profileID}")]
        public ActionResult Index(int userID, int profileID)
        {
            AddOnsPageVM vm = new AddOnsPageVM();
            vm.UserID = userID;
            vm.LoginID = ActiveLogin.LoginID;
            vm.ProfileID = profileID;

            ViewBag.RateProfileNavID = 5;
            ViewBag.RateProfileSelectedTab = "pro-advisor-profiles";

            ProAdvisorPresetProfile profile = _proAdvisorProfileService.GetProfile(profileID);

            if (profile != null && (profile.LoginID == ActiveLogin.LoginID) || profileID == 1)
            {
                vm.ProfileIsValid = true;
                vm.ProfileName = profile.Name;
                vm.ProfileDefault = profile.DefaultFlag;
                vm.ProfileID = profileID;

                if (profileID == 1 && profile.LoginID != ActiveLogin.LoginID)
                {
                    vm.CanBeEdited = false;
                }
                else
                {
                    vm.CanBeEdited = true;
                }

                ViewBag.NavID = "rates";
                vm.IsGlobalProfile = true;
            }
            else
            {
                return Redirect("/" + userID + "/rates");
            }

            return View(vm);
        }

        [HttpPost]
        [Route("{userID}/pro-advisor/{profileID}")]
        public ActionResult Index(AddOnsPageVM vm)
        {
            RedirectResult redirectResult = DoRedirect("");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            return DoRedirect("/" + vm.UserID + "/pro-advisor");
        }

        public ActionResult GetProfileGrid([DataSourceRequest] DataSourceRequest request, int siteUserID, int profileID, string name, string operationType, string laborType)
        {
            SiteActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(siteUserID, GetComputerKey());

            List<ProAdvisorPreset> presets = _proAdvisorProfileService.GetPresetsByFilters(siteUserID, profileID, activeLogin.HasProAdvisorContract, name, operationType, laborType);

            List<AddOnPresetVM> vms = new List<AddOnPresetVM>();

            foreach(ProAdvisorPreset preset in presets)
            {
                vms.Add(new AddOnPresetVM(preset));
            }

            return Json(vms.ToDataSourceResult(request));
        }

        public JsonResult SavePresets(int userID, int profileID, string profileName, List<PresetData> presets)
        {
            CacheActiveLoginID(userID);
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            FunctionResult result = _proAdvisorProfileService.SavePresets(activeLogin.ID, userID, profileID, profileName, presets);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

    }
}