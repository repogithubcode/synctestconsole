using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProEstimatorData.DataModel;
using ProEstimator.Admin.ViewModel.LogIn;
using System.Net;

namespace ProEstimator.Admin.Controllers
{
    public class LogInController : AdminController
    {
        [HttpGet]
        public ActionResult Index()
        {
            LogInPageVM vm = new LogInPageVM();

            HttpCookie ckloginname = Request.Cookies["ProEAdminLoginName"];
            HttpCookie ckpassword = Request.Cookies["ProEAdminPassword"];

            if (ckloginname != null && ckpassword != null)
            {
                vm.UserName = ckloginname.Value;
                vm.Password = ckpassword.Value;
                vm.RememberMe = true;
            }

            return View(vm);
        }

        [HttpPost]
        public ActionResult Index(LogInPageVM vm)
        {
            SalesRep salesRep = SalesRep.GetAll().FirstOrDefault(o => o.UserName == vm.UserName);
            if (salesRep != null)
            {
                bool passwordMatch = salesRep.Password == vm.Password;

#if DEBUG
                passwordMatch = true;
#endif

                if (passwordMatch)
                {
                    if (vm.RememberMe)
                    {
                        HttpCookie Mycookie = new HttpCookie("ProEAdminLoginName");
                        Mycookie.Value = vm.UserName;
                        Mycookie.Expires = DateTime.MaxValue;
                        Mycookie.HttpOnly = true;
                        Mycookie.Secure = true;
                        Response.Cookies.Add(Mycookie);

                        Mycookie = new HttpCookie("ProEAdminPassword");
                        Mycookie.Value = vm.Password;
                        Mycookie.Expires = DateTime.MaxValue;
                        Mycookie.HttpOnly = true;
                        Mycookie.Secure = true;
                        Response.Cookies.Add(Mycookie);
                    }
                    else
                    {
                        HttpCookie LNcookie = Request.Cookies["ProEAdminLoginName"];
                        HttpCookie passCookie = Request.Cookies["ProEAdminPassword"];
                        if (LNcookie != null)
                        {
                            LNcookie.Expires = DateTime.Now;
                            LNcookie.Value = "";
                            LNcookie.HttpOnly = true;
                            LNcookie.Secure = true;
                            Response.SetCookie(LNcookie);
                        }

                        if (passCookie != null)
                        {
                            passCookie.Expires = DateTime.Now;
                            passCookie.Value = "";
                            passCookie.HttpOnly = true;
                            passCookie.Secure = true;
                            Response.SetCookie(passCookie);
                        }
                    }

                    _activeLoginManager.AuthorizeAdmin(salesRep.SalesRepID, Request.UserHostAddress, Request.UserAgent, Request.Browser.Browser);

                    return Redirect("/Logins/List/0");
                }
                else
                {
                    vm.ErrorMessage = "Wrong Password";
                }
            }
            else
            {
                vm.ErrorMessage = "Invalid User Name";
            }

            return View(vm);
        }

        [HttpGet]
        [Route("LogOut")]
        public ActionResult LogOut()
        {
            Session["ActiveLoginID"] = "";
            Session["SalesRepID"] = "";
            return Redirect("/");
        }

        //TODO: This is a copy of the code in Proestimator.Controllers.SiteController.GetComputerKey()
        //      This should be refactored to share the same implementation
        private string GetComputerKey()
        {
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

    }
}