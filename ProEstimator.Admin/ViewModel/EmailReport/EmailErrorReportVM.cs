using ProEstimatorData.DataModel;
using ProEstimatorData;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ProEstimatorData.DataModel.Admin;
using System;

namespace ProEstimator.Admin.ViewModel.EmailReport
{
    public class EmailErrorReportVM
    {
        public int ID { get; private set; }
        public int LoginID { get; private set; }
        public string LoginCompany { get; private set; }
        public string ToAddress { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string Event { get; private set; }
        public string Reason { get; private set; }
        public string ReplyTo { get; private set; }
        public string Subject { get; private set; }
        public string AttachmentPaths { get; private set; }

        public EmailErrorReportVM() { }

        public EmailErrorReportVM(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            LoginCompany = InputHelper.GetString(row["Organization"].ToString());
            ToAddress = InputHelper.GetString(row["ToAddress"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Event = InputHelper.GetString(row["Event"].ToString());
            Reason = InputHelper.GetString(row["Reason"].ToString());
            ReplyTo = InputHelper.GetString(row["ReplyTo"].ToString());
            Subject = InputHelper.GetString(row["Subject"].ToString());
            AttachmentPaths = InputHelper.GetString(row["AttachmentPaths"].ToString());
        }

        public static List<EmailErrorReportVM> GetForFilter(string loginIDFilter, string rangeStartFilter, string rangeEndFilter,
                                                string addressFilter, string eventFilter)
        {
            List<EmailErrorReportVM> emailErrors = new List<EmailErrorReportVM>();
            DataTable dataTable = GetForFilterData(loginIDFilter, rangeStartFilter, rangeEndFilter, addressFilter, eventFilter);

            foreach (DataRow row in dataTable.Rows)
            {
                emailErrors.Add(new EmailErrorReportVM(row));
            }

            return emailErrors;
        }

        public static DataTable GetForFilterData(string loginIDFilter, string rangeStartFilter, string rangeEndFilter,
                                                string emailAddressFilter, string eventFilter)
        {
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@LoginID", Common.GetParameterValue(loginIDFilter)) { SqlDbType = SqlDbType.Int });
            parameters.Add(new SqlParameter("@StartDate", Common.GetParameterValue(rangeStartFilter)) { SqlDbType = SqlDbType.VarChar });
            parameters.Add(new SqlParameter("@EndDate", Common.GetParameterValue(rangeEndFilter)) { SqlDbType = SqlDbType.VarChar });
            parameters.Add(new SqlParameter("@EmailAddress", Common.GetParameterValue(emailAddressFilter)) { SqlDbType = SqlDbType.VarChar });
            parameters.Add(new SqlParameter("@Event", Common.GetParameterValue(eventFilter)) { SqlDbType = SqlDbType.VarChar });
                        
            DBAccessTableResult tableResult = db.ExecuteWithTable("[dbo].[EmailErrorReport]", parameters);

            return tableResult.DataTable;
        }
    }
}