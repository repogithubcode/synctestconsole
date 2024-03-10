using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimator.Business.Logic;
using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.Controllers
{
    public class AdminController : Controller
    {

        protected static AdminLoginManager _activeLoginManager = new AdminLoginManager();

        protected ActiveLogin ActiveLogin { get; private set; }

        protected bool AdminIsValidated()
        {
            int salesRepID = GetSalesRepID();
            if (salesRepID > -1)
            {
                return true;
            }

            return false;
        }

        public int GetSalesRepID()
        {
            int salesRepID = -1;

            try
            {
                if (Session["SalesRepID"] != null)
                {
                    salesRepID = (int)Session["SalesRepID"];
                }
            }
            catch (Exception ex)
            {
                // ErrorLogger.LogError(ex.Message, "AdminController GetSalesRepID");
            }

            return salesRepID;
        }

        public int GetActiveLoginID()
        {
            int activeLoginID = InputHelper.GetInteger(Session["ActiveLoginID"].ToString());
            return activeLoginID;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            
            base.OnActionExecuting(filterContext);

            int salesRepID = GetSalesRepID();

            if (salesRepID > -1)
            {
                ViewBag.SessionSalesRepID = salesRepID;
                SalesRep salesRep = SalesRep.Get(ViewBag.SessionSalesRepID);
                if (salesRep != null)
                {
                    ViewBag.SalesRepName = salesRep.UserName;
                }
                else
                {
                    ViewBag.SalesRepName = "";
                }

                int activeLoginID = GetActiveLoginID();
                ActiveLogin = _activeLoginManager.ActiveLogins.FirstOrDefault(o => o.ID == activeLoginID) as ActiveLogin;
            }
        }

        public string ClearFrontEndCache(int salesRepID, string code)
        {
            string frontEndRoot = "https://proestimator.web-est.com/";

            try
            {
                frontEndRoot = System.Configuration.ConfigurationManager.AppSettings.Get("FrontEndRootUrl").ToString();
            }
            catch
            {
                ErrorLogger.LogError("Admin, FrontEndRootUrl config not set.", "Admin FrontEndRootUrl");
            }

            try
            {
                System.Net.WebClient client = new System.Net.WebClient();

                int combined = salesRepID + DateTime.Now.Minute;
                string combinedEncoded = ProEstimatorData.Encryptor.Encrypt(combined.ToString());
                combinedEncoded = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(combinedEncoded);
                string link = frontEndRoot + "Home/LKJLKDFLKE?one=" + salesRepID.ToString() + "&two=" + combinedEncoded + "&three=" + code;

                ErrorLogger.LogError(link, "CacheLink");

                string result = client.DownloadString(link);

                if (result == "AAA")
                {
                    return "Minute compare did not match.  Try again.";
                }
                else if (result == "ZZZ")
                {
                    return "Cache cleared";
                }
                else if (result == "XXX")
                {
                    return "No code match.";
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "AdminController ClearFrontEndCache");
            }

            return "No result.";
        }
    }
}