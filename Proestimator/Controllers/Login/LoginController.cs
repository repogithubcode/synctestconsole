using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using Proestimator.ViewModel;
using ProEstimator.Business.Logic.Admin;
using ProEstimator.Business.Logic;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.DataModel.Profiles;
using ProEstimator.Business.Model;
using ProEstimatorData.DataModel.ProAdvisor;
using ProEstimator.Business.OptOut;
using Proestimator.Controllers.Login.Commands;
using Proestimator.Controllers.Vendors.Commands;
using ProEstimator.Business.ProAdvisor;
using static Proestimator.Controllers.InsuranceController;
using System.Configuration;
using System.Net;

namespace Proestimator.Controllers
{
    [AllowAnonymous]
    public class LoginController : SiteController
    {

        private IOptOutService _optOutService;
        private IProAdvisorProfileService _proAdvisorProfileService;


        public LoginController(IOptOutService optOutService, IProAdvisorProfileService proAdvisorProfileService)
        {
            _optOutService = optOutService;
            _proAdvisorProfileService = proAdvisorProfileService;
        }

        /// <param name="mobile"></param>
        /// <param name="redirect"></param>
        /// <param name="link">pass a LoginID::RedirectLink text that's been encrypted.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index(string mobile = "", string redirect = "", string link = "")
        {
            //CleanVendorsTableCommand command = new CleanVendorsTableCommand();
            //command.Execute();

            LoginVM vm = new LoginVM();

            if (!string.IsNullOrEmpty(link))
            {
                ProcessLoginLinkCommand processLoginLink = new ProcessLoginLinkCommand(_siteLoginManager, link, GetComputerKey());
                if (processLoginLink.Execute())
                {
                    return Redirect(processLoginLink.RedirectLink);
                }
                else
                {
                    vm.Redirect = processLoginLink.RedirectLink;
                }
            }

            HttpCookie userNameCookie = Request.Cookies["PEUserName"];

            if (userNameCookie != null)
            {
                vm.UserName = userNameCookie.Value;
                vm.RememberMe = true;
            }
            else
            {
                HttpCookie orgCookie = Request.Cookies["Org"];
                if (orgCookie != null)
                {
                    vm.UserName = orgCookie.Value;
                    vm.RememberMe = true;
                }
            }

            // During the transition, before this udpate we saved the users encrypted password.  If it is there, load it into the form then clear the cookie so the user's password manager can take over
            try
            {
                HttpCookie passwordCookie = Request.Cookies["PETransitionPassword"];
                if (passwordCookie != null)
                {
                    string password = ProEstimatorData.Encryptor.Decrypt(passwordCookie.Value);
                    vm.Password = password;
                }
            }
            catch { }

            // Try impersonating the user, this link would come from the Admin tool.
            string impersonateEncrypted = Request.QueryString["impersonate"];
            if (!string.IsNullOrEmpty(impersonateEncrypted))
            {
                try
                {
                    string decrypted = ProEstimatorData.Encryptor.Decrypt(impersonateEncrypted);
                    int userID = InputHelper.GetInteger(decrypted);
                    if (userID > 0)
                    {
                        SiteUser user = SiteUser.Get(userID);
                        if (user != null)
                        {
                            return DoLogin(user, false, true);
                        }
                        else
                        {
                            vm.ErrorMessage = string.Format(@Proestimator.Resources.ProStrings.LoginNotFound, userID.ToString());
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    vm.ErrorMessage = @Proestimator.Resources.ProStrings.InvalidImpersonateToken;
                }
            }

            // The redirect link could have been set earlier, only use the passed redirect link if it hasn't.
            if (string.IsNullOrEmpty(vm.Redirect))
            {
                vm.Redirect = redirect;
            }

            return View(vm);
        }

        
        [HttpPost]
        public ActionResult Index(LoginVM vm)
        {
            string ipAddress = GetClientIpAddress(Request);
            LogInFunctionResult result = _siteLoginManager.AuthenticateUser(vm.UserName, vm.Password, ComputerKey, ipAddress, vm.OtherLogins, vm.NoOfLoginsExceeded);
            
            if (result.Success)
            {
                return DoLogin(result.SiteUser, vm.RememberMe, false, vm.Redirect); 
            }
            else
            {
                if (result.MultipleMatches)
                {
                    return Redirect("old-login/update");
                }
                else if (result.OtherLoginsCount > 0)
                {
                    vm.ErrorMessage = result.ErrorMessage;
                    vm.OtherLogins = true;
                    vm.NoOfLoginsExceeded = result.NoOfLoginsExceeded;
                    ModelState.Remove("OtherLogins");
                    ModelState.Remove("NoOfLoginsExceeded");
                    return View(vm);
                }
                else
                {
                    vm.ErrorMessage = result.ErrorMessage;
                    return View(vm);
                }                
            }
        }

        public JsonResult SendForgotPassword(string emailAddress)
        {
            FunctionResult result = _siteLoginManager.SendForgotPasswordEmail(emailAddress);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private ActionResult DoLogin(SiteUser siteUser, bool rememberMe, bool isImpersonated, string redirect = "")
        {
            string ipAddress = GetClientIpAddress(Request);
            DeviceType device = GetDeviceType(Request);

            Boolean autosaveturnedontechsupport = true;
            if (Request.QueryString["autosaveturnedontechsupport"] != null)
            {
                autosaveturnedontechsupport = InputHelper.GetBoolean(Request.QueryString["autosaveturnedontechsupport"]);
            }

            Boolean autosaveturnedonsiteuser = true;
            if (Request.QueryString["autosaveturnedonsiteuser"] != null)
            {
                autosaveturnedonsiteuser = InputHelper.GetBoolean(Request.QueryString["autosaveturnedonsiteuser"]);
            }

            _siteLoginManager.AuthorizeSiteUserID(siteUser.ID, ComputerKey, isImpersonated, ipAddress, Request.UserAgent, device, Request.Browser.Browser, autosaveturnedontechsupport, autosaveturnedonsiteuser);

            LoginTracking.LogTracking(siteUser.LoginID, Request.ServerVariables.Get("REMOTE_ADDR"), Request.Browser.Type, Request.Browser.Browser, Request.Browser.Version, Request.Browser.Platform, Request.Browser.IsMobileDevice, Request.UserAgent);
            UpdateUserNameCookie(siteUser.EmailAddress, rememberMe);

            Contract activeContract = Contract.GetActive(siteUser.LoginID);

            SiteActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(siteUser.ID, ComputerKey);

            SuccessBoxFeatureLog.LogFeature(siteUser.LoginID, SuccessBoxModule.EstimateWriting, "User logged into system", activeLogin.ID);

            if (activeLogin != null && activeContract != null)
            {
                LoginAutoRenew autoRenew = LoginAutoRenew.GetLastForLogin(activeLogin.LoginID);
                if (autoRenew == null || !autoRenew.Enabled)
                {
                    activeLogin.ShowEarlyRenewal = ContractManager.IsInEarlyRenewalPeriod(activeContract);
                }
            }

            // If there is an in progress contract go to the confirmation page
            if (activeContract == null)
            {
                Contract inProgress = Contract.GetInProgress(activeLogin.LoginID);
                if (inProgress != null)
                {
                    return Redirect("/" + activeLogin.SiteUserID + "/invoice/subscription-confirm/" + inProgress.ID);
                }
            }

            // Check if the active contract will expire within 90 days and the user hasn't answered the survey
            if (activeContract != null && activeContract.ExpirationDate < DateTime.Now.AddDays(90))
            {
                RenewalSurvey survey = RenewalSurvey.GetForContract(activeContract.ID);
                if (survey == null)
                {
                    return Redirect("/" + activeLogin.SiteUserID + "/survey");
                }
            }

            if (activeLogin != null)
            {
                Boolean loginToSave = false;
                LoginInfo loginInfo = LoginInfo.GetByID(activeLogin.LoginID);

                if (loginInfo.UseDefaultRateProfile == false)
                {
                    List<RateProfile> rateProfiles = RateProfile.GetAllForLogin(activeLogin.LoginID);

                    if(rateProfiles!= null && rateProfiles.Count == 1)
                    {
                        if (rateProfiles[0].IsDefault == false)
                        {
                            rateProfiles[0].SetAsDefaultProfile(); // set customer profiles flag set to 1
                        }

                        // following updates flag in logininfo table for for UseDefaultRateProfile
                        loginInfo.UseDefaultRateProfile = !loginInfo.UseDefaultRateProfile;
                        loginToSave = true;
                    }
                }

                // PDR Rate Profile
                List<PDR_RateProfile> profiles = PDR_RateProfile.GetByLogin(activeLogin.LoginID).Where(o => o.AdminInfoID == 0).ToList();
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
                        loginToSave = true;
                    }
                }

                // save rate profile and PDR rate profile
                if(loginToSave)
                {
                    loginInfo.Save();
                }
                
                // ProAdvisor rate profile
                if (((SiteActiveLogin)activeLogin).HasProAdvisorContract)
                {
                    bool useDefault = _proAdvisorProfileService.UseDefaultProfile(activeLogin.LoginID);

                    if (useDefault == false)
                    {
                        List<ProAdvisorPresetProfile> addOnPresetProfiles = _proAdvisorProfileService.GetAllProfilesForLogin(activeLogin.LoginID, false);
                        if (addOnPresetProfiles != null && addOnPresetProfiles.Count == 0) 
                        {
                            _proAdvisorProfileService.SetUseDefaultProfile(activeLogin.ID, activeLogin.LoginID, !useDefault);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(redirect))
            {
                // If the redirect doesn't start with the user ID, add it
                if (!redirect.StartsWith("/" + activeLogin.SiteUserID))
                {
                    redirect = "/" + activeLogin.SiteUserID + redirect;
                }

                return Redirect(redirect);
            }

            // The first time a user account is created, we need the user to confirm their user name
            //if (!siteUser.Confirmed)
            //{
            //    return Redirect("/" + activeLogin.SiteUserID.ToString() + "/confirm");
            //}

            return Redirect("/" + activeLogin.SiteUserID.ToString()); 
        }

        public string GetClientIpAddress(HttpRequestBase request)
        {
            // Check if the X-Forwarded-For header is present
            string xForwardedFor = request.Headers["X-Forwarded-For"];

            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                // Get the first IP address in the X-Forwarded-For header
                string[] ipAddresses = xForwardedFor.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (ipAddresses.Length > 0)
                {
                    return ipAddresses[0].Trim();
                }
            }

            // If X-Forwarded-For header is not present, return the UserHostAddress
            return request.UserHostAddress;
        }       

        public static DeviceType GetDeviceType(HttpRequestBase request)
        {
            var userAgent = request.UserAgent.ToLower();

            // Check for mobile devices
            if (userAgent.Contains("mobile") && !userAgent.Contains("ipad") && !userAgent.Contains("tablet"))
            {
                return DeviceType.Mobile;
            }

            // Check for tablets
            if (userAgent.Contains("tablet") || userAgent.Contains("ipad") ||
                (userAgent.Contains("android") && !userAgent.Contains("mobile")))
            {
                return DeviceType.Tablet;
            }

            // If not mobile or tablet, assume it's a desktop
            return DeviceType.Desktop;
        }

        [Route("{siteUserID}/log-out")]
        public ActionResult Logout(int siteUserID)
        {
            ActiveLogin activeLogin = _siteLoginManager.DeactivateSiteUser(siteUserID, GetComputerKey());

            Object isImpersonatedVal = false;

            if (activeLogin != null && TempData.TryGetValue("IsImpersonated", out isImpersonatedVal) == true)
            {
                _siteLoginManager.LogoutImpersonationActiveLoginForLoginID(activeLogin.LoginID);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult ImpersonationLogout(int loginID)
        {
            AjaxResult ajaxResult = new AjaxResult();

            _siteLoginManager.LogoutImpersonationActiveLoginForLoginID(loginID);
            ajaxResult.Success = true;

            return Json(ajaxResult, JsonRequestBehavior.AllowGet);
        }

        private void UpdateUserNameCookie(string userName, bool chkRememberMe)
        {
            if (chkRememberMe)
            {
                HttpCookie Mycookie = new HttpCookie("PEUserName");
                Mycookie.Value = userName;
                Mycookie.Expires = DateTime.MaxValue;
                Mycookie.HttpOnly = true;
                Mycookie.Secure = true;
                Response.Cookies.Add(Mycookie);
            }
            else
            {
                HttpCookie LNcookie = Request.Cookies["PEUserName"];
                if (LNcookie != null)
                {
                    LNcookie.Expires = DateTime.Now;
                    LNcookie.Value = "";
                    LNcookie.HttpOnly = true;
                    LNcookie.Secure = true;
                    Response.SetCookie(LNcookie);
                }
            }   

            try
            {
                HttpCookie LNcookie = Request.Cookies["PETransitionPassword"];
                if (LNcookie != null)
                {
                    LNcookie.Expires = DateTime.Now;
                    LNcookie.Value = "";
                    LNcookie.HttpOnly = true;
                    LNcookie.Secure = true;
                    Response.SetCookie(LNcookie);
                }
            }
            catch { }
        }
        
        public JsonResult KickLogin(int loginID, string code)
        {
            if (InputHelper.GetHash(loginID.ToString(), 10) != code)
            {
                return Json(@Proestimator.Resources.ProStrings.BadHash, JsonRequestBehavior.AllowGet);
            }

            int kickedCount = _siteLoginManager.KickOutLogin(loginID);

            ErrorLogger.LogError("Kicked " + kickedCount + " sessions for account " + loginID, "KickLogin");

            return Json("Canceled " + kickedCount + " sessions.", JsonRequestBehavior.AllowGet);
        }

        public JsonResult KickUser(int userID, string code)
        {
            if (InputHelper.GetHash(userID.ToString(), 10) != code)
            {
                return Json(@Proestimator.Resources.ProStrings.BadHash, JsonRequestBehavior.AllowGet);
            }

            int kickedCount = _siteLoginManager.KickOutSiteUser(userID);

            ErrorLogger.LogError("Kicked " + kickedCount + " sessions for site user " + userID, "KickLogin");

            return Json("Canceled " + kickedCount + " sessions.", JsonRequestBehavior.AllowGet);
        }

        #region Old Logins

        [HttpGet]
        [Route("old-login/update")]
        public ActionResult OldLogin()
        {
            LoginOldVM vm = new LoginOldVM();

            HttpCookie ckloginname = Request.Cookies["LN"];
            HttpCookie ckorganization = Request.Cookies["Org"];

            if (ckloginname != null && ckorganization != null)
            {
                vm.UserName = ckloginname.Value;
                vm.Organization = ckorganization.Value;
            }

            return View(vm);
        }

        [HttpPost]
        [Route("old-login/update")]
        public ActionResult OldLogin(LoginOldVM vm)
        {
            // Make sure the old credentials are correct
            List<LoginInfo> loginInfoList = LoginInfo.GetByCredentials(vm.UserName, vm.Organization, vm.Password);
            if (loginInfoList == null || loginInfoList.Count == 0)
            {
                vm.ErrorMessage = "Old credentials are invalid.";
            }

            SiteLoginManager loginManager = new ProEstimator.Business.Logic.SiteLoginManager();
            FunctionResult passwordValid = loginManager.ValidatePassword(vm.NewPassword, vm.NewPassword2);
            if (passwordValid.Success)
            {
                FunctionResultInt userResult = SiteLoginManager.CreateUser(loginInfoList[0].ID, vm.NewUserName, vm.NewPassword, true);

                if (userResult.Success)
                {
                    SiteUser newUser = SiteUser.Get(userResult.Value);
                    LogInFunctionResult result = _siteLoginManager.AuthenticateUser(newUser.EmailAddress, vm.NewPassword, ComputerKey);

                    if (result.Success)
                    {
                        return DoLogin(result.SiteUser, true, false, "/" + result.SiteUser.ID.ToString() + "/settings/users");
                    }
                    else
                    {
                        vm.ErrorMessage = result.ErrorMessage;
                        return View(vm);
                    }
                }
                else
                {
                    vm.ErrorMessage = userResult.ErrorMessage;
                }
            }
            else
            {
                vm.ErrorMessage = passwordValid.ErrorMessage;
            }

            return View("OldLogin", vm);
        }

        public JsonResult CheckOldCredentials(string loginName, string organization, string password)
        {
            AjaxResult ajaxResult = new AjaxResult();
            ajaxResult.Success = true;

            loginName = loginName.Trim();
            organization = organization.Trim();
            password = password.Trim();

            // Get the LoginInfo record for the passed credentials
            List<LoginInfo> loginInfoList = LoginInfo.GetByCredentials(loginName, organization, password);
            if (loginInfoList == null)
            {
                ajaxResult.Success = false;
                ajaxResult.ErrorMessage = "Invalid credentials";
            }
            else
            {
                LoginInfo loginInfo = loginInfoList[0];

                if (loginInfo.Disabled)
                {
                    ajaxResult.Success = false;
                    ajaxResult.ErrorMessage = Proestimator.Resources.ProStrings.Login_Error_AccountDisabled;
                }
                else
                {
                    // Make sure there is no SiteUser already set up for this account.
                    if (SiteUser.GetForLogin(loginInfo.ID).Count > 0)
                    {
                        ajaxResult.Success = false;
                        ajaxResult.ErrorMessage = "A user has already been set up for this account, please log in with the existing user account or contact support for assistance.";
                    }
                    else
                    {
                        // Everything checks out, try getting the email address from the main contact for the first account.
                        try
                        {
                            Contact contact = Contact.GetContact(loginInfo.ContactID);
                            if (contact != null)
                            {
                                ajaxResult.Message = contact.Email;
                            }
                        }
                        catch { }
                    }
                }
            }

            return Json(ajaxResult, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Password Reset

        [HttpGet]
        [Route("password-reset/{code}")]
        public ActionResult PasswordReset(string code)
        {
            PasswordResetVM vm = new PasswordResetVM();
            vm.GoodLink = false;

            PasswordResetLink link = PasswordResetLink.GetByCode(code);
            if (link != null && link.TimeStamp.AddMinutes(30) > DateTime.Now)
            {
                SiteUser siteUser = SiteUser.Get(link.SiteUserID);
                if (siteUser != null)
                {
                    vm.GoodLink = true;

                    vm.UserName = siteUser.EmailAddress;
                    vm.Code = link.Code;
                }
            }

            return View(vm);
        }

        [HttpPost]
        [Route("password-reset/{code}")]
        public ActionResult PasswordReset(PasswordResetVM vm)
        {
            PasswordResetLink link = PasswordResetLink.GetByCode(vm.Code);
            if (link != null && link.TimeStamp.AddMinutes(30) > DateTime.Now)
            {
                if (vm.NewPassword1 != vm.NewPassword2)
                {
                    vm.ErrorMessage = "Password fields must match.";
                }
                else
                {
                    FunctionResult result = _siteLoginManager.ChangeUserPassword(0, link.SiteUserID, vm.NewPassword1);

                    if (result.Success)
                    {
                        SiteUser siteUser = SiteUser.Get(link.SiteUserID);
                        return DoLogin(siteUser, true, false);
                    }
                    else
                    {
                        vm.ErrorMessage = result.ErrorMessage;
                    }
                }
            }
            else
            {
                vm.ErrorMessage = "The code is either invalid or expired.  Codes expire in 30 minutes.  Please try using the Forgot Password feature again.";
            }

            return View("PasswordReset", vm);
        }

        #endregion Password Reset

    }
}