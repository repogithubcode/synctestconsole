using System;
using ProEstimatorData;

namespace ProEstimator.Admin.ViewModel.Logins
{
    public class AutoRenewStatusVM
    {
        public int LoginID { get; private set; }
        public string When { get; private set; }
        public string Action { get; private set; }
        public string SalesRepName { get; private set; }
        public AutoRenewStatusVM(int login, DateTime time, string what, string who)
        {
            LoginID = InputHelper.GetInteger(login.ToString());
            When = InputHelper.GetString(time.ToString());
            Action = InputHelper.GetString(what.ToString());
            SalesRepName = InputHelper.GetString(who.ToString());
        }
    }
}