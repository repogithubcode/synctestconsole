using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    public class UserFeedBack
    {
        public string UserFeedbackID { get; set; }
        public string LoginID { get; set; }
        public string ReporterName { get; set; }
        public string Phone { get; set; }
        public string CreatedDate { get; set; }
        public string FeedbackText { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string BrowserDetails { get; set; }
        public string DevicePlatform { get; set; }

        public List<string> ImagesPath { get; set; }
        public string ImagePaths { get; set; }

        public string ActionNote { get; set; }

        public UserFeedBack()
        {
        }

        public UserFeedBack(DataRow row)
        {
            UserFeedbackID = InputHelper.GetString(row["UserFeedbackID"].ToString());
            CreatedDate = InputHelper.GetString(row["CreatedDate"].ToString());
            FeedbackText = InputHelper.GetString(row["FeedbackText"].ToString()).Replace("\r\n", "\n").Replace("\n", "<br />");
            LoginID = InputHelper.GetString(row["LoginID"].ToString());
            CompanyName = InputHelper.GetString(row["CompanyName"].ToString());
            FirstName = InputHelper.GetString(row["FirstName"].ToString());
            LastName = InputHelper.GetString(row["LastName"].ToString());
            Phone = InputHelper.GetString(row["Phone1"].ToString());
            Email = InputHelper.GetString(row["Email"].ToString());
            BrowserDetails = InputHelper.GetString(row["BrowserDetails"].ToString());
            DevicePlatform = InputHelper.GetString(row["DevicePlatform"].ToString());
            ImagesPath = InputHelper.GetString(row["ImagesPaths"].ToString()).Split('|').ToList();
            ImagePaths = InputHelper.GetString(row["ImagesPaths"].ToString());
            ActionNote = InputHelper.GetString(row["ActionNote"].ToString());
        }

        public static List<UserFeedBack> GetForFilter(string reporterNameFilter, string rangeStartFilter, string rangeEndFilter, int? userFeedbackID = null)
        {
            List<UserFeedBack> userFeedBacks = new List<UserFeedBack>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@ID", Common.GetParameterValue(userFeedbackID)) { SqlDbType = SqlDbType.Int });
            parameters.Add(new SqlParameter("@ContactName", Common.GetParameterValue(reporterNameFilter)) { SqlDbType = SqlDbType.VarChar, Size = 200 });
            parameters.Add(new SqlParameter("@DateStart", Common.GetParameterValue(rangeStartFilter)) { SqlDbType = SqlDbType.Date });
            
            if(!string.IsNullOrEmpty(rangeEndFilter))
            {
                parameters.Add(new SqlParameter("@DateEnd", Common.GetParameterValue(DateTime.Parse(rangeEndFilter).AddDays(1).ToString())) { SqlDbType = SqlDbType.Date });
            }
            
            DBAccessTableResult tableResult = db.ExecuteWithTable("[Admin].[UserFeedBackBugReport]", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                userFeedBacks.Add(new UserFeedBack(row));
            }

            return userFeedBacks.OrderByDescending(item => Convert.ToDateTime(item.CreatedDate)).ToList();
        }

        public static FunctionResult SaveUserFeedBackBug(UserFeedBack userFeedBack)
        {
            List<UserFeedBack> userFeedBacks = new List<UserFeedBack>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@UserFeedbackID", Common.GetParameterValue(userFeedBack.UserFeedbackID)) { SqlDbType = SqlDbType.Int });
            parameters.Add(new SqlParameter("@ActionNote", Common.GetParameterValue(userFeedBack.ActionNote)) { SqlDbType = SqlDbType.NVarChar });

            FunctionResult functionResult = db.ExecuteNonQuery("[Admin].[SaveUserFeedBackBug]", parameters);

            return functionResult;
        }
    }
}