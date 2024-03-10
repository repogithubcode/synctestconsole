using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace ProEstimatorData.DataModel
{
    public class EmailCustomTemplate : ProEstEntity
    {
        public int ID { get; set; }
        public int LoginID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Template { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDefault { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public EmailCustomTemplate() { }

        public EmailCustomTemplate(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
            Description = InputHelper.GetString(row["Description"].ToString());
            Template = InputHelper.GetString(row["Template"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());
            IsDefault = InputHelper.GetBoolean(row["IsDefault"].ToString());
            CreatedDate = InputHelper.GetDateTimeNullable(row["CreatedDate"].ToString());
            ModifiedDate = InputHelper.GetDateTimeNullable(row["ModifiedDate"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("Name", InputHelper.GetString(Name)));
            parameters.Add(new SqlParameter("Description", InputHelper.GetString(Description)));
            parameters.Add(new SqlParameter("Template", InputHelper.GetString(Template)));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));
            parameters.Add(new SqlParameter("IsDefault", IsDefault));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("EmailCustomTemplate_Save", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "EmailCustomTemplate", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static EmailCustomTemplate Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("EmailCustomTemplateGet", new SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return new EmailCustomTemplate(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<EmailCustomTemplate> GetForLogin(int loginID, bool includedDeleted)
        {
            var templates = new List<EmailCustomTemplate>();

            var db = new DBAccess();

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("LoginID", loginID)
            };

            var tableResult = db.ExecuteWithTable("EmailCustomTemplateGetForLogin", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    if (includedDeleted || !InputHelper.GetBoolean(row["IsDeleted"].ToString()))
                        templates.Add(new EmailCustomTemplate(row));
                }
            }

            return templates;
        }

        public void SetIsDeleted(int activeLoginID, bool isDeleted)
        {
            IsDeleted = isDeleted;
            Save(activeLoginID);
        }
    }
}

