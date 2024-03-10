using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using ProEstimatorData;

namespace ProEstimator.Admin.ViewModel.Logins
{
    public class LoginSearchVM
    {
        public int LoginID { get; set; }
        public string LoginName { get; set; }
        public string Name { get; set; }
        public string BusinessName { get; set; }
        public string Organization { get; set; }
        public DateTime CreationDate { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string SalesRep { get; set; }
        public bool UserDeleted { get; set; }

        public LoginSearchVM() { }

        public LoginSearchVM(DataRow row)
        {
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            LoginName = InputHelper.GetString(row["LoginName"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
            BusinessName = InputHelper.GetString(row["BusinessName"].ToString());
            Organization = InputHelper.GetString(row["Organization"].ToString());
            CreationDate = InputHelper.GetDateTime(row["CreationDate"].ToString());
            Email = InputHelper.GetString(row["Email"].ToString());
            Password = InputHelper.GetString(row["Password"].ToString());
            SalesRep = InputHelper.GetString(row["SalesRep"].ToString());
            UserDeleted = InputHelper.GetBoolean(row["UserDeleted"].ToString());
        }
    }
}