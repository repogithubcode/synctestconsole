using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimatorData.DataModel
{
    public class Email : ProEstEntity 
    {
        public int ID { get; private set; }
        public int LoginID { get; set; }
        public int TemplateID { get; set; }
        public List<string> ToAddresses { get; private set; }
        public List<string> CCAddresses { get; private set; }
        public List<string> SMSNumbers { get; private set; }
        public string ReplyTo { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<string> AttachmentPaths { get; private set; }
        public DateTime CreateStamp { get; set; }
        public DateTime? SendStamp { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        public string CompanyName { get; set; }
        public string Recipient { get; set; }

        public void AddToAddress(string address)
        {
            if (!CCAddresses.Contains(address) && !ToAddresses.Contains(address))
            {
                ToAddresses.Add(address);
            }
        }

        public void AddCCAddress(string address)
        {
            if (!CCAddresses.Contains(address) && !ToAddresses.Contains(address))
            {
                CCAddresses.Add(address);
            }
        }

        public void AddSMSNumber(string number)
        {
            if (!SMSNumbers.Contains(number))
            {
                SMSNumbers.Add(number);
            }
        }

        public void AddAttachmentPath(string attachmentPath)
        {
            if (!AttachmentPaths.Contains(attachmentPath))
            {
                AttachmentPaths.Add(attachmentPath);
            }
        }

        private void AddCompanyName()
        {
            if (string.IsNullOrEmpty(CompanyName) && !HasError)
            {
                DBAccess db = new DBAccess();
                DBAccessTableResult result = db.ExecuteWithTable("GetCompanyName", new SqlParameter("ID", ID));

                if (result.Success)
                {
                    DataRow row = result.DataTable.Rows[0];
                    CompanyName = row["CompanyName"].ToString();
                }
            }
        }

        public Email()
        {
            CreateStamp = DateTime.Now;

            ToAddresses = new List<string>();
            CCAddresses = new List<string>();
            SMSNumbers = new List<string>();
            AttachmentPaths = new List<string>();
        }

        public Email(DataRow row, bool recipient = false)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            TemplateID = InputHelper.GetInteger(row["TemplateID"].ToString());
            ToAddresses = StringToList(InputHelper.GetString(row["ToAddresses"].ToString()));
            CCAddresses = StringToList(InputHelper.GetString(row["CCAddresses"].ToString()));
            SMSNumbers = StringToList(InputHelper.GetString(row["SMSNumbers"].ToString()));
            ReplyTo = InputHelper.GetString(row["ReplyTo"].ToString());
            Subject = InputHelper.GetString(row["Subject"].ToString());
            Body = InputHelper.GetString(row["Body"].ToString());
            AttachmentPaths = StringToList(InputHelper.GetString(row["AttachmentPaths"].ToString()));
            CreateStamp = InputHelper.GetDateTime(row["CreateStamp"].ToString());
            SendStamp = InputHelper.GetDateTimeNullable(row["SendStamp"].ToString());
            HasError = InputHelper.GetBoolean(row["HasError"].ToString());
            ErrorMessage = InputHelper.GetString(row["ErrorMessage"].ToString());
            CompanyName = InputHelper.GetString(row["CompanyName"].ToString());
            if (recipient)
            {
                Recipient = InputHelper.GetString(row["Recipient"].ToString());
            }

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("TemplateID", TemplateID));
            parameters.Add(new SqlParameter("ToAddresses", ListToString(ToAddresses)));
            parameters.Add(new SqlParameter("CCAddresses", ListToString(CCAddresses)));
            parameters.Add(new SqlParameter("SMSNumbers", ListToString(SMSNumbers)));
            parameters.Add(new SqlParameter("ReplyTo", InputHelper.GetString(ReplyTo)));
            parameters.Add(new SqlParameter("Subject", InputHelper.GetString(Subject)));
            parameters.Add(new SqlParameter("Body", InputHelper.GetString(Body)));
            parameters.Add(new SqlParameter("AttachmentPaths", ListToString(AttachmentPaths)));
            parameters.Add(new SqlParameter("CreateStamp", CreateStamp));
            parameters.Add(new SqlParameter("SendStamp", SendStamp));
            parameters.Add(new SqlParameter("HasError", HasError));
            parameters.Add(new SqlParameter("ErrorMessage", InputHelper.GetString(ErrorMessage)));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("Email_Save", parameters);

            if (intResult.Success)
            {
                ID = intResult.Value;

                ChangeLogManager.LogChange(activeLoginID, "Email", ID, LoginID, parameters, RowAsLoaded);

                AddCompanyName();
            }

            return new SaveResult(intResult);
        }

        public string ListToString(List<string> list)
        {
            StringBuilder builder = new StringBuilder();
            string seperator = "";

            foreach (string item in list)
            {
                builder.Append(seperator + item);
                seperator = " ; ";
            }

            return builder.ToString();
        }

        private List<string> StringToList(string input)
        {
            List<string> results = new List<string>();

            string[] pieces = input.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string piece in pieces)
            {
                string trimmedPiece = piece.Trim();

                if (!results.Contains(trimmedPiece))
                {
                    results.Add(trimmedPiece);
                }
            }

            return results;
        }

        public static List<Email> GetUnsent()
        {
            List<Email> emails = new List<Email>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Emails_GetUnsent");

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                emails.Add(new Email(row));
            }

            return emails;
        }

        public static List<Email> GetByLoginAndTemplate(int loginID, int templateID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("TemplateID", templateID));

            List<Email> emails = new List<Email>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Emails_GetByAccountAndTemplate", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                emails.Add(new Email(row));
            }

            return emails;
        }

        public static List<Email> GetForFilter(string loginIDFilter, string rangeStartFilter, string rangeEndFilter,
                                                string addressFilter, string subjectFilter, string bodyFilter, string hasErrorFilter, string errorMessageFilter)
        {
            List<Email> emails = new List<Email>();
            DataTable dataTable = GetForFilterData(loginIDFilter, rangeStartFilter, rangeEndFilter, addressFilter, subjectFilter, bodyFilter, hasErrorFilter, errorMessageFilter);

            foreach (DataRow row in dataTable.Rows)
            {
                emails.Add(new Email(row, true));
            }

            return emails;
        }

        public static DataTable GetForFilterData(string loginIDFilter, string rangeStartFilter, string rangeEndFilter,
                                                string emailAddressFilter, string emailSubjectFilter, string emailBodyFilter, 
                                                string hasErrorFilter, string errorMessageFilter)
        {
            List<Email> emails = new List<Email>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@LoginID", Common.GetParameterValue(loginIDFilter)) { SqlDbType = SqlDbType.Int });
            parameters.Add(new SqlParameter("@DateStart", Common.GetParameterValue(rangeStartFilter)) { SqlDbType = SqlDbType.Date });
            parameters.Add(new SqlParameter("@DateEnd", Common.GetParameterValue(rangeEndFilter)) { SqlDbType = SqlDbType.Date });

            parameters.Add(new SqlParameter("@EmailAddress", Common.GetParameterValue(emailAddressFilter)) { SqlDbType = SqlDbType.VarChar });
            parameters.Add(new SqlParameter("@EmailSubject", Common.GetParameterValue(emailSubjectFilter)) { SqlDbType = SqlDbType.VarChar });
            parameters.Add(new SqlParameter("@EmailBody", Common.GetParameterValue(emailBodyFilter)) { SqlDbType = SqlDbType.VarChar });

            parameters.Add(new SqlParameter("@HasError", Common.GetParameterValue(hasErrorFilter)) { SqlDbType = SqlDbType.Bit });
            parameters.Add(new SqlParameter("@ErrorMessage", Common.GetParameterValue(errorMessageFilter)) { SqlDbType = SqlDbType.VarChar });
            
            DBAccessTableResult tableResult = db.ExecuteWithTable("[dbo].[EmailSentReport]", parameters);

            return tableResult.DataTable;
        }
    }
}
