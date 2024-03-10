using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    public class ErrorLog
    {
        public int Id { get; set; }
        public int LoginID { get; set; }
        public int AdminInfoID { get; set; }
        public string ErrorText { get; set; }
        public string TimeOccurred { get; set; }
        public string SessionVars { get; set; }
        public bool ErrorFixed { get; set; }
        public string FixNote { get; set; }
        public string App { get; set; }

        public ErrorLog()
        {

        }

        public ErrorLog(DataRow row)
        {
            Id = InputHelper.GetInteger(row["id"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            AdminInfoID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            ErrorText = InputHelper.GetString(row["ErrorText"].ToString());
            ErrorFixed = InputHelper.GetBoolean(row["ErrorFixed"].ToString());
            SessionVars = InputHelper.GetString(row["SessionVars"].ToString());
            App = InputHelper.GetString(row["App"].ToString());
            TimeOccurred = InputHelper.GetString(row["TimeOccurred"].ToString());
            FixNote = InputHelper.GetString(row["FixNote"].ToString());
        }

        public static List<ErrorLog> GetForFilter(int loginID, int estimateID, string rangesStart, string rangeEnd, string errorTag, string errorText)
        {
            List<ErrorLog> errorLogs = new List<ErrorLog>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", Common.GetParameterValue(loginID)) { SqlDbType = SqlDbType.Int });
            parameters.Add(new SqlParameter("AdminInfoID", Common.GetParameterValue(estimateID)) { SqlDbType = SqlDbType.Int });

            if(!string.IsNullOrEmpty(errorTag))
            {
                parameters.Add(new SqlParameter("ErrorTag", Common.GetParameterValue(errorTag)) { SqlDbType = SqlDbType.VarChar, Size = 255 });
            }
            
            parameters.Add(new SqlParameter("RangeStart", Common.GetParameterValue(rangesStart)) { SqlDbType = SqlDbType.VarChar });
            parameters.Add(new SqlParameter("RangeEnd", Common.GetParameterValue(rangeEnd)) { SqlDbType = SqlDbType.VarChar });

            if (!string.IsNullOrEmpty(errorText))
            {
                parameters.Add(new SqlParameter("ErrorText", Common.GetParameterValue(errorText)) { SqlDbType = SqlDbType.VarChar, Size = 2000 });
            }
                
            DBAccessTableResult tableResult = db.ExecuteWithTable("Error_GetList", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                errorLogs.Add(new ErrorLog(row));
            }

            return errorLogs.OrderByDescending(item => item.TimeOccurred).ToList();
        } 

        public static List<ErrorTag> GetDistinctErrorTagList()
        {
            List<ErrorTag> errorTags = new List<ErrorTag>();
            DBAccess db = new DBAccess();

            DBAccessTableResult tableResult = db.ExecuteWithTable("ErrorTag_GetList");

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                errorTags.Add(new ErrorTag(Convert.ToString(row["FixNote"]), InputHelper.GetInteger(Convert.ToString(row["FixNoteCount"]))));
            }

            return errorTags;
        }
    }

    public class ErrorTag
    {
        public string ErrorTagText { get; set; }

        public int ErrorTagCount { get; set; }

        public ErrorTag(string errorTagText, int errorTagCount)
        {
            ErrorTagText = errorTagText;
            ErrorTagCount = errorTagCount;
        }
    }
}