using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

using ProEstimator.Business.Logic;

using ProEstimatorData;
using ProEstimatorData.Models;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using Proestimator.Helpers;
using System.Threading;
using ProEstimatorData.Helpers;
using ProEstimatorData.DataModel.Financing;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Net;

namespace Proestimator.Controllers
{

    public class SiteController : Controller
    {

        protected static SiteLoginManager _siteLoginManager = new SiteLoginManager();

        protected SitePermissionsManager PermissionManager
        {
            get
            {
                if (_permissionManager == null)
                {
                    _permissionManager = new SitePermissionsManager(GetUserIDFromPath());
                }

                return _permissionManager;
            }
        }
        private SitePermissionsManager _permissionManager;

        protected SiteActiveLogin ActiveLogin { get; private set; }
        protected string ComputerKey { get; private set; }

        protected bool IsMobile { get; private set; }

        protected void CacheActiveLoginID(int userID)
        {
            try
            {
                HttpContext.Items["ActiveLoginID"] = GetActiveLoginID(userID);
            }
            catch { }
        }

        /// <summary>
        /// This is called before every ActionResult function.  
        /// Gets a reference to an ActiveLogin instance.  If the login is not authorized, redirect to the login page.
        /// </summary>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Ignore requests to the log out function
            if (Request.Path.ToLower().Contains("/log-out"))
            {
                return;
            }

            ComputerKey = GetComputerKey();
            ViewBag.ComputerKey = ComputerKey;
            ViewBag.MenuShowGraphicalButton = false;

            IsMobile = Request.Browser.IsMobileDevice;
            ViewBag.IsMobileDevice = IsMobile;
            ViewBag.Browser = Request.Browser.Browser.ToString().ToLower();

            int userID = GetUserIDFromPath();            

            if (userID > 0)
            {
                if (!PermissionManager.IsPageAllowed(Request.Path.ToLower()))
                {
                    filterContext.Result = new RedirectResult("/" + userID);
                    return;
                }

                ActiveLogin = _siteLoginManager.GetActiveLogin(userID, ComputerKey);
               
                if (ActiveLogin != null)
                {
                    ViewBag.UserID = ActiveLogin.SiteUserID;
                    ViewBag.LoginID = ActiveLogin.LoginID;
                    ViewBag.UserEmailAddress = ActiveLogin.UserEmailAddress;

                    if(ViewBag.Browser == "safari")
                    {
                        ViewBag.UseLegacyPartsSectionDropdown = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "UseLegacyPartsSectionDropdown", "AddPartsOptions", (false).ToString()).ValueString);
                    }
                    else
                    {
                        ViewBag.UseLegacyPartsSectionDropdown = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "UseLegacyPartsSectionDropdown", "AddPartsOptions", (false).ToString()).ValueString);
                    }

                    PDFDownloadSetting downloadSetting = (PDFDownloadSetting)InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "DownloadPDF", "ReportOptions", ((int)PDFDownloadSetting.OpenNewTab).ToString()).ValueString);
                    ViewBag.QuickPrintTarget = downloadSetting == PDFDownloadSetting.Download ? "_self" : "_blank";

                    // Register this thread with the active login ID so any DBAccess instances created in this thread can find it.
                    HttpContext.Items["ActiveLoginID"] = ActiveLogin.ID;

                    // Only log user activity on GET requests, otherwise we would log twice for every page update
                    if (HttpContext.Request.HttpMethod == "GET")
                    {
                        ActiveLogin.MarkActivity();
                    }

                    // Most pages need an active contract to see.
                    if (PageNeedsActiveContract(Request.Path))
                    {
                        // Jump to the contract page if there is no contract
                        if (!ActiveLogin.HasContract)
                        {
                            SiteUser siteUser = SiteUser.Get(userID);
                            Contract activeContract = Contract.GetActive(siteUser.LoginID);
                            if (activeContract != null && !activeContract.HasPayment)
                            {
                                filterContext.Result = new RedirectResult("/" + userID + "/invoice/subscription-confirm/" + activeContract.ID);
                            }

                            // See if we have an in progress contract
                            Contract inProgressContract = Contract.GetInProgress(siteUser.LoginID);
                            if (inProgressContract != null)
                            {
                                filterContext.Result = new RedirectResult("/" + userID + "/invoice/subscription-confirm/" + inProgressContract.ID);
                            }
                            else
                            {
                                filterContext.Result = new RedirectResult("/" + userID + "/invoice/pick-contract");
                            }
                        }

                        if (ActiveLogin.InvoiceNeedsPayment)
                        {
                            filterContext.Result = new RedirectResult("/" + userID + "/invoice/customer-invoice");
                        }

                        // Jump to the digital signature page if the user needs to sign it
                        if (ActiveLogin.HasUnsignedContract && !Request.Path.ToLower().Contains("/digital-signature/") && !ActiveLogin.IsImpersonated)
                        {
                            SiteUser siteUser = SiteUser.Get(userID);
                            Contract activeContract = Contract.GetActive(siteUser.LoginID);
                            if (activeContract != null && activeContract.HasPayment)
                            {
                                filterContext.Result = new RedirectResult("/" + userID + "/digital-signature/" + activeContract.ID);
                            }
                        }
                    }

                    // Make sure the LanguagePreference is loaded.
                    if (string.IsNullOrEmpty(ActiveLogin.LanguagePreference))
                    {
                        ActiveLogin.LanguagePreference = InputHelper.GetString(SiteSettings.Get(ActiveLogin.LoginID, "Culture", "LanguagePreferences", "en-us").ValueString);
                    }

                    // ShowChatIconDesktop
                    ActiveLogin.ShowChatIconDesktop = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "ShowChatIconDesktop", "ProgramPreferences", "1").ValueString);
                    ViewBag.ShowChatIconDesktop = ActiveLogin.ShowChatIconDesktop;

                    // ShowChatIconMobile
                    ActiveLogin.ShowChatIconMobile = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "ShowChatIconMobile", "ProgramPreferences", "0").ValueString);
                    ViewBag.ShowChatIconMobile = ActiveLogin.ShowChatIconMobile;

                    // Set the language
                    string cultureName = ActiveLogin.LanguagePreference;
                    var cultureInfo = CultureInfo.GetCultureInfo(cultureName);
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;

                    ViewBag.IsImpersonated = ActiveLogin.IsImpersonated;
                    ViewBag.AdminIsImpersonating = ActiveLogin.AdminIsImpersonating;
                    ViewBag.AutoSaveTurnedOnTechSupport = ActiveLogin.AutoSaveTurnedOnTechSupport;
                    ViewBag.AutoSaveTurnedOnSiteUser = ActiveLogin.AutoSaveTurnedOnSiteUser;
                    TempData["IsImpersonated"] = ActiveLogin.IsImpersonated;

                    ViewBag.HeaderShowEarlyRenewal = ActiveLogin.ShowEarlyRenewal;

                    ViewBag.IsAdmin = PermissionManager.HasPermission("Admin");
                    ViewBag.RatesPermit = ViewBag.IsAdmin || PermissionManager.HasPermission("Rates");
                    ViewBag.CustListPermit = ViewBag.IsAdmin || PermissionManager.HasPermission("Customer List");
                    ViewBag.InsurancePermit = ViewBag.IsAdmin || PermissionManager.HasPermission("Insurance");
                    ViewBag.ReportsPermit = ViewBag.IsAdmin || PermissionManager.HasPermission("Reports");
                    ViewBag.VendorPermit = ViewBag.IsAdmin || PermissionManager.HasPermission("Vendor");
                    ViewBag.AddOnsPermit = ViewBag.IsAdmin || PermissionManager.HasPermission("AddOns");
                    ViewBag.SettingsPermit = ViewBag.IsAdmin || PermissionManager.HasPermission("Settings");
                    ViewBag.EstDelPermit = ViewBag.IsAdmin || PermissionManager.HasPermission("Delete Estimate");

                    ViewBag.IsRemoteSupportOn = RemoteSupportService.GetInstance().IsTurnOnRemoteSupportLink(ActiveLogin.LoginID);

                    // 12/5/23 - EVB: Ensuring all wisetack financing content is not shown in the app
                    ViewBag.ShowFinancing = false;
                    //var financingAllowForLoginIDs = ConfigurationManager.AppSettings.Get("FinancingAllowForLoginIDs").ToString();
                    //ViewBag.ShowFinancing = !string.IsNullOrEmpty(financingAllowForLoginIDs) &&
                    //    (financingAllowForLoginIDs == "*" ||
                    //    $",{financingAllowForLoginIDs},".IndexOf($",{ActiveLogin.LoginID},") >= 0);
                    //ViewBag.IsFinancingApproved = false;
                    //if (ViewBag.ShowFinancing)
                    //{
                    //    var merchantInfo = FinancingService.Instance.GetWisetackMerchantData(ActiveLogin.LoginID);
                    //    ViewBag.IsFinancingApproved = merchantInfo?.Status == "APPLICATION_APPROVED";
                    //}

                    ViewBag.ShowCreditCardPayment = CreditCardPaymentService.IsAuthorized(ActiveLogin.LoginID);
                }
                else
                {
                    //filterContext.Result = new RedirectResult("/?redirect=" + Request.Path);
                    filterContext.Result = new RedirectResult("/");
                    return;
                }
            }
        }

        //protected override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //    base.OnActionExecuted(filterContext);

        //    DBAccess.UnregisterThread(Thread.CurrentThread.ManagedThreadId);
        //}

        private bool PageNeedsActiveContract(string path)
        {
            path = path.ToLower();
            if (path.Contains("/invoice/") || path.Contains("/settings/") && path.Contains("digital-signature"))
            {
                return false;
            }

            return true;
        }

        protected int GetUserIDFromPath()
        {
            string fullPath = Request.Path.Substring(1, Request.Path.Length - 1);
            int firstSlash = fullPath.IndexOf('/');
            if (firstSlash < 0)
            {
                return InputHelper.GetInteger(fullPath);
            }
            else
            {
                return InputHelper.GetInteger(fullPath.Substring(0, firstSlash));
            }
        }

        /// <summary>
        /// Gets a 10 digit unique ID for the user's computer.  This value is saved in a cookie.  If there is no current value a new one is created and saved in a cookie.
        /// </summary>
        protected string GetComputerKey()
        {
            // VERY HACKY - for the test project, there is no request, and it needs to use the test computer key.
            if (Request == null)
            {
                return "ABCDEFGHIJ";
            }

            // Check for an existing saved ID
            HttpCookie existingID = Request.Cookies["ProEID"];
            if (existingID != null)
            {
                if (existingID.Value.Length == 10)
                {
                    return existingID.Value;
                }
            }

            // No ID found, create a new one
            string computerID = Guid.NewGuid().ToString().Replace("-", "").ToUpper().Substring(0, 10);

            // Save the ID to a cookie
            HttpCookie newID = new HttpCookie("ProEID");
            newID.Value = computerID;
            newID.Expires = DateTime.MaxValue;
            newID.HttpOnly = true;
            newID.Secure = true;
            Response.Cookies.Add(newID);

            return computerID;
        }

        protected bool IsUserAuthorized(int userID)
        {
            return _siteLoginManager.IsUserAuthorized(userID, GetComputerKey());
        }

        public int GetActiveLoginID(int userID = 0)
        {
            if (userID == 0)
            {
                userID = GetUserIDFromPath();
            }

            if (userID > 0)
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                if (activeLogin != null)
                {
                    return activeLogin.ID;
                }
            }

            return 0;
        }

        /// <summary>
        /// Buttons set the value of the hidden field "redirectDataField" to a path to redirect to.
        /// </summary>
        protected RedirectResult DoRedirect(string defaultPath)
        {
            string redirectData = Request["redirectDataField"];
            if (!string.IsNullOrEmpty(redirectData))
            {
                return Redirect(redirectData);
            }
            else if (!string.IsNullOrEmpty(defaultPath))
            {
                return Redirect(defaultPath);
            }

            return null;
        }

        public void TestProjectUserLogin(int userID, string computerKey)
        {
            _siteLoginManager.AuthorizeSiteUserID(userID, computerKey, false, "", "", DeviceType.Desktop, "");
        }
    }
}