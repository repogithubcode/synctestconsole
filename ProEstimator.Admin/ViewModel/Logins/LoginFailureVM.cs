using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;
using ProEstimatorData;
using ProEstimator.Business.Model.Admin;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel.Logins
{
    public class LoginFailureVM
    {
        public int ID { get; set; }
        public int LoginId { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string TimeDate { get; set; }
        public string UserAddress { get; set; }
        public string Reason { get; set; }
        public string SalesRep { get; set; }

        public LoginFailureVM() { }

        public LoginFailureVM(LoginFailure loginFailure)
        {
            ID = InputHelper.GetInteger(loginFailure.ID.ToString());
            LoginId = InputHelper.GetInteger(loginFailure.LoginId.ToString());
            LoginName = InputHelper.GetString(loginFailure.LoginName.ToString());
            Password = InputHelper.GetString(loginFailure.Password.ToString());
            TimeDate = InputHelper.GetString(loginFailure.TimeDate.ToString());
            UserAddress = InputHelper.GetString(loginFailure.UserAddress.ToString());
            Reason = InputHelper.GetString(loginFailure.Reason.ToString());
            SalesRep = InputHelper.GetString(loginFailure.SalesRep.ToString());
        }
    }
}