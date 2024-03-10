using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using ProEstimatorData;
using ProEstimator.Business.Model.Admin;

namespace ProEstimator.Admin.ViewModel.Logins
{
    public class LoginFailureReportVM
    {
        public int ID { get; set; }
        public int LoginId { get; set; }
        public string LoginName { get; set; }
        public string Organization { get; set; }
        public string Password { get; set; }
        public string TimeDate { get; set; }
        public string UserAddress { get; set; }
        public string Reason { get; set; }
        public string SalesRep { get; set; }

        public LoginFailureReportVM() { }

        public LoginFailureReportVM(VmLoginFailureReport vmLoginFailureReport)
        {
            ID = InputHelper.GetInteger(vmLoginFailureReport.ID.ToString());
            LoginId = InputHelper.GetInteger(vmLoginFailureReport.LoginId.ToString());
            LoginName = InputHelper.GetString(vmLoginFailureReport.LoginName.ToString());
            Organization = InputHelper.GetString(vmLoginFailureReport.Organization.ToString());
            Password = InputHelper.GetString(vmLoginFailureReport.Password.ToString());
            TimeDate = InputHelper.GetString(vmLoginFailureReport.TimeDate.ToString());
            UserAddress = InputHelper.GetString(vmLoginFailureReport.UserAddress.ToString());
            Reason = InputHelper.GetString(vmLoginFailureReport.Reason.ToString());
            SalesRep = InputHelper.GetString(vmLoginFailureReport.SalesRep.ToString());
        }
    }
}