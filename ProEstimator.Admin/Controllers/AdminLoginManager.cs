using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Business.Logic;
using System.Data;

namespace ProEstimator.Admin.Controllers
{
    public class AdminLoginManager : LoginManager<ActiveLogin>
    {
        public AdminLoginManager() 
            : base(true)
        {
        }

        protected override ActiveLogin InstantiateNewActiveLogin()
        {
            return new ActiveLogin();
        }

        protected override ActiveLogin LoadActiveLogin(DataRow row)
        {
            return new ActiveLogin(row);
        }

        public void AuthorizeAdmin(int salesRepID, string ipAddress, string userAgent, string browser)
        {
            LogOutAdmin(salesRepID);

            ActiveLogin activeLogin = InstantiateNewActiveLogin();
            activeLogin.SalesRepID = salesRepID;
            activeLogin.StartTime = DateTime.Now;
            activeLogin.IPAddress = ipAddress;
            activeLogin.ComputerName = userAgent;
            activeLogin.LastActivity = DateTime.Now;
            // activeLogin.DeviceType = deviceType;
            activeLogin.Browser = browser;
            activeLogin.Save();
                
            ActiveLogins.Add(activeLogin);

            HttpContext.Current.Session["ActiveLoginID"] = activeLogin.ID;
            HttpContext.Current.Session["SalesRepID"] = salesRepID;
        }

        private void LogOutAdmin(int salesRepID)
        {
            List<ActiveLogin> activeLogins = ActiveLogins.Where(o => o.SalesRepID == salesRepID).ToList();
            foreach(ActiveLogin activeLogin in activeLogins)
            {
                activeLogin.DeleteKey = ActiveLoginDeleteKey.LoggedOut;
                activeLogin.Save();

                ActiveLogins.Remove(activeLogin);
            }
        }
    }
}