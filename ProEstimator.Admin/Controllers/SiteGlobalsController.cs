using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

using ProEstimatorData;
using ProEstimatorData.DataModel;

using ProEstimator.Admin.ViewModel;

namespace ProEstimator.Admin.Controllers
{
    public class SiteGlobalsController : AdminController
    {
        [HttpGet]
        public ActionResult Index()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/SiteGlobals";
                return Redirect("/LogOut");
            }
            else
            {
                SiteGlobalsPageVM pageVM = new SiteGlobalsPageVM();

                SiteGlobals siteGlobals = SiteGlobals.Get();
                pageVM.HomePageMessage = siteGlobals.HomePageMessage;

                return View(pageVM);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Index(SiteGlobalsPageVM vm)
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/SiteGlobals";
                return Redirect("/LogOut");
            }
            else
            {
                SiteGlobals siteGlobals = SiteGlobals.Get();

                siteGlobals.HomePageMessage = vm.HomePageMessage;
                SaveResult result = siteGlobals.Save(ActiveLogin.ID);

                vm.ErrorMessage = result.ErrorMessage;

                if (result.Success)
                {
                    string frontEndRoot = "https://proestimator.web-est.com/";

                    try
                    {
                        frontEndRoot = ConfigurationManager.AppSettings.Get("FrontEndRootUrl").ToString();
                    }
                    catch
                    {
                        ErrorLogger.LogError("Admin, FrontEndRootUrl config not set.", "Admin FrontEndRootUrl");
                    }

                    try
                    {
                        System.Net.WebClient client = new System.Net.WebClient();
                        client.DownloadData(frontEndRoot + "Home/ReloadSiteGlobals");
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, 0, "SiteGlobals Call ReloadSiteGlobals");
                        vm.ErrorMessage = "Error calling ReloadSiteGlobals: " + ex.Message;
                    }
                }

                return View(vm);
            }
        }
    }
}