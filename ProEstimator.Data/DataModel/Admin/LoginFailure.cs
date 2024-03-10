using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.Admin
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
            Password = InputHelper.GetString(row["Password"].ToString());
            TimeDate = InputHelper.GetString(row["TimeDate"].ToString());
            UserAddress = InputHelper.GetString(row["User_Addr"].ToString());
            Reason = InputHelper.GetString(row["Reason"].ToString());
            SalesRep = InputHelper.GetString(row["SalesRep"].ToString());
        }

        public static List<LoginFailure> GetForFilter(int loginIDFilter, string loginNameFilter, string passwordFilter)
        {
            List<LoginFailure> loginFailures = new List<LoginFailure>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginIDFilter));
            parameters.Add(new SqlParameter("LoginNameFilter", loginNameFilter));
            parameters.Add(new SqlParameter("Password", passwordFilter));

            DBAccessTableResult tableResult = db.ExecuteWithTable("getLoginFailures", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                loginFailures.Add(new LoginFailure(row));
            }

            return loginFailures.OrderByDescending(item => item.ID).ToList();
        }

        public static LoginFailure CreateLoginFailure(DataRow row)
        {
            LoginFailure temp = new LoginFailure();

            temp.ID = InputHelper.GetInteger(row["ID"].ToString());
            temp.LoginName = InputHelper.GetString(row["LoginName"].ToString());
            temp.Organization = InputHelper.GetString(row["Organization"].ToString());
            temp.Password = InputHelper.GetString(row["Password"].ToString());
            temp.TimeDate = InputHelper.GetString(row["TimeDate"].ToString());
            temp.UserAddress = InputHelper.GetString(row["User_Addr"].ToString());
            temp.Reason = InputHelper.GetString(row["Reason"].ToString());

            return temp;
        }

        public static List<LoginFailure> GetLoginFailures(string email, string pwd, string sDate, string eDate)
        {
            List<LoginFailure> loginFailures = new List<LoginFailure>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Email", email));
            parameters.Add(new SqlParameter("Password", pwd));
            parameters.Add(new SqlParameter("StartDate", sDate));
            parameters.Add(new SqlParameter("EndDate", eDate));

            DBAccessTableResult tableResult = db.ExecuteWithTable("LoginFailures_Get", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                loginFailures.Add(CreateLoginFailure(row));
            }

            return loginFailures;
        }
    }
}