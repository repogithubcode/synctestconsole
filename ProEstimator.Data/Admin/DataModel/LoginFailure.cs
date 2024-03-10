using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.Admin.DataModel
{
    public class LoginFailure
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

        public LoginFailure()
        {

        }

        public LoginFailure(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginId = InputHelper.GetInteger(row["LoginId"].ToString());
            LoginName = InputHelper.GetString(row["LoginName"].ToString());
            Organization = InputHelper.GetString(row["Organization"].ToString());
            Password = InputHelper.GetString(row["Password"].ToString());
            TimeDate = InputHelper.GetString(row["TimeDate"].ToString());
            UserAddress = InputHelper.GetString(row["User_Addr"].ToString());
            Reason = InputHelper.GetString(row["Reason"].ToString());
            SalesRep = InputHelper.GetString(row["SalesRep"].ToString());
        }

        public static List<LoginFailure> Get(string LoginNameFilter, string OrganizationFilter)
        {
            LoginFailure loginFailure = null;
            List<LoginFailure> loginFailureListColl = new List<LoginFailure>();
            var result = new DataTable();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginNameFilter", LoginNameFilter));
            parameters.Add(new SqlParameter("OrganizationFilter", OrganizationFilter));

            DBAccessTableResult tableResult = db.ExecuteWithTable("getLoginFailures", parameters);

            if (tableResult != null && tableResult.DataTable != null)
            {
                foreach (DataRow eachDataRow in tableResult.DataTable.Rows)
                {
                    loginFailure = new LoginFailure(eachDataRow);
                    loginFailureListColl.Add(loginFailure);
                }
            }

            return loginFailureListColl.OrderByDescending(item => item.ID).ToList();
        }
    }
}