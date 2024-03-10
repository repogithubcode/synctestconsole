using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class CreateReportHistory : ProEstEntity 
    {

        public int ID { get; set; }
        public int LoginID { get; set; }
        public string ReportType { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedTimeStamp { get; set; }

        public CreateReportHistory()
        {
            ID = 0;
            LoginID = 0;
            ReportType = "";
            FileName = "";
            CreatedTimeStamp = DateTime.MinValue;
        }

        public CreateReportHistory(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            ReportType = InputHelper.GetString(row["ReportType"].ToString());
            FileName = InputHelper.GetString(row["FileName"].ToString());
            CreatedTimeStamp = InputHelper.GetDateTime(row["CreatedTimeStamp"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("FileName", InputHelper.GetString(FileName)));
            parameters.Add(new SqlParameter("ReportType", InputHelper.GetString(ReportType)));
            
            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("CreateReportHistory_Insert", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "CreateReport", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static List<CreateReportHistory> GetForLogin(int loginID, string reportType)
        {
            List<CreateReportHistory> createReportHistoryColl = new List<CreateReportHistory>();

            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("ReportType", reportType));

            DBAccessTableResult tableResult = db.ExecuteWithTable("GetCreateReportHistoryForLogin", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    createReportHistoryColl.Add(new CreateReportHistory(row));
                }
            }

            return createReportHistoryColl;
        }
    }
}