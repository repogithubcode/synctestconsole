using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class ReportEmail
    {
        public int ID { get; private set; }
        public DateTime SentStamp { get; set; }
        public DateTime? DeleteStamp { get; private set; }
        public int EstimateID { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ToAddresses { get; set; }
        public string CCAddresses { get; set; }
        public string PhoneNumbers { get; set; }
        public string Errors { get; set; }
        public int EmailID { get; set; }

        public bool IsDeleted
        {
            get
            {
                return DeleteStamp.HasValue;
            }
        }

        public ReportEmail()
        {

        }

        public ReportEmail(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            SentStamp = InputHelper.GetDateTime(row["SentStamp"].ToString());
            DeleteStamp = InputHelper.GetNullableDateTime(row["DeleteStamp"].ToString());
            EstimateID = InputHelper.GetInteger(row["EstimateID"].ToString());
            Subject = row["Subject"].ToString();
            Body = row["Body"].ToString();
            ToAddresses = row["ToAddresses"].ToString();
            CCAddresses = row["CCAddresses"].ToString();
            PhoneNumbers = row["PhoneNumbers"].ToString();
            Errors = row["Errors"].ToString();
            EmailID = InputHelper.GetInteger(row["EmailID"].ToString());
        }

        public SaveResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("SentStamp", SentStamp));
            parameters.Add(new SqlParameter("DeleteStamp", DeleteStamp));
            parameters.Add(new SqlParameter("EstimateID", EstimateID));
            parameters.Add(new SqlParameter("Subject", Subject));
            parameters.Add(new SqlParameter("Body", Body));
            parameters.Add(new SqlParameter("ToAddresses", ToAddresses));
            parameters.Add(new SqlParameter("CCAddresses", CCAddresses));
            parameters.Add(new SqlParameter("PhoneNumbers", PhoneNumbers));
            parameters.Add(new SqlParameter("Errors", InputHelper.GetString(Errors)));
            parameters.Add(new SqlParameter("EmailID", EmailID));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("ReportEmail_AddOrUpdate", parameters);
            if (result.Success)
            {
                ID = result.Value;
            }

            return new SaveResult(result);
        }  

        public static ReportEmail Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("ReportEmail_Get", new SqlParameter("ID", id));

            if (result.Success)
            {
                return new ReportEmail(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static ReportEmail GetForSMSCode(string code)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("ReportEmail_GetBySMSCode", new SqlParameter("Code", code));

            if (result.Success)
            {
                return new ReportEmail(result.DataTable.Rows[0]);
            }

            return null;
        }


        public static List<ReportEmail> GetForEstimate(int estimateID)
        {
            List<ReportEmail> emails = new List<ReportEmail>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("ReportEmail_GetByEstimate", new SqlParameter("EstimateID", estimateID));

            if (result.Success)
            {
                foreach(DataRow row in result.DataTable.Rows)
                {
                    emails.Add(new ReportEmail(row));
                }
            }

            return emails;
        }

        public void InsertReportAttachment(int reportID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("EmailID", this.ID));
            parameters.Add(new SqlParameter("ReportID", reportID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("ReportEmailAttachment_Insert", parameters);
        }
    }
}
