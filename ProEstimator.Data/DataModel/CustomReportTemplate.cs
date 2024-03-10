using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class CustomReportTemplate : ProEstEntity 
    {

        public int ID { get; set; }
        public int LoginID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Template { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int ReportHeaderID { get; set; }
        public int ReportFooterID { get; set; }

        public CustomReportTemplateType CustomReportTemplateType { get; set; }
        public Boolean IsSystemReport { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public string ReportFooterHtmlFileName { get { return ReportFooterID + ".html"; }  }
        public CustomReportTemplate()
        {
            ID = 0;
            LoginID = 0;
            Name = "";
            Description = "";
            Template = "";
            IsActive = true;
            IsDeleted = false;
            ReportHeaderID = 0;
            ReportFooterID = 0;
            CustomReportTemplateType = 0;
            IsSystemReport = false;
        }

        public CustomReportTemplate(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
            Description = InputHelper.GetString(row["Description"].ToString());
            Template = InputHelper.GetString(row["Template"].ToString());
            IsActive = InputHelper.GetBoolean(row["IsActive"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());
            ReportHeaderID = InputHelper.GetInteger(row["ReportHeaderID"].ToString());
            ReportFooterID = InputHelper.GetInteger(row["ReportFooterID"].ToString());

            int reportTemplateTypeID = InputHelper.GetInteger(row["CustomReportTemplateTypeID"].ToString());
            if (reportTemplateTypeID == 0)
            {
                reportTemplateTypeID = 1;
            }
            CustomReportTemplateType = (CustomReportTemplateType)reportTemplateTypeID;

            IsSystemReport = InputHelper.GetBoolean(row["IsSystemReport"].ToString());

            CreatedOn = InputHelper.GetDateTimeNullable(row["CreatedOn"].ToString());
            ModifiedOn = InputHelper.GetDateTimeNullable(row["ModifiedOn"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("Name", InputHelper.GetString(Name)));
            parameters.Add(new SqlParameter("Description", InputHelper.GetString(Description)));
            parameters.Add(new SqlParameter("Template", InputHelper.GetString(Template)));
            parameters.Add(new SqlParameter("IsActive", IsActive));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));
            parameters.Add(new SqlParameter("ReportHeaderID", ReportHeaderID));
            parameters.Add(new SqlParameter("ReportFooterID", ReportFooterID));
            parameters.Add(new SqlParameter("CustomReportTemplateTypeID", CustomReportTemplateType));
            parameters.Add(new SqlParameter("IsSystemReport", IsSystemReport));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("CustomReportTemplate_Save", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "CustomReportTemplate", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static CustomReportTemplate Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomReportTemplateGet", new SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return new CustomReportTemplate(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public static void DeleteRestoreCustomReport(int id, Boolean isDeleted)
        {
            CustomReportTemplate customReportTemplate = Get(id);
            customReportTemplate.IsDeleted = isDeleted;
            customReportTemplate.Save();
        }

        public static int CopyCustomReport(int loginID, int reportID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("ReportToCopy", reportID));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("CustomReportTemplate_Copy", parameters);

            if (result.Success)
            {
                return result.Value;
            }

            return 0;
        }

        /// <summary>
        /// Get all templates for this user and the global templates
        /// </summary>
        public static List<CustomReportTemplate> GetForLogin(int loginID, bool showDeleted = false)
        {
            List<CustomReportTemplate> templates = new List<CustomReportTemplate>();

            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("IsDeleted", showDeleted));

            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomReportTemplateGetForLogin", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    templates.Add(new CustomReportTemplate(row));
                }
            }

            return templates;
        }

        public static DataTable GetCustomReportData(int estimateID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomReport_GetData", new SqlParameter("AdminInfoID", estimateID));
            return tableResult.DataTable;
        }

        public static List<string> GetAllTags()
        {
            lock(_tagsLock)
            {
                if (_tags == null || _tags.Count == 0)
                {
                    _tags = new List<string>();

                    DataTable dataTable = GetCustomReportData(1);
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        _tags.Add(column.ColumnName);
                    }

                    _tags = _tags.OrderBy(o => o).ToList();
                }
            }

            return _tags.ToList();
        }

        private static List<string> _tags;
        private static object _tagsLock = new object();

        public void Delete(int activeLoginID = 0)
        {
            IsDeleted = true;
            Save(activeLoginID);
        }

        public void Restore(int activeLoginID = 0)
        {
            IsDeleted = false;
            Save(activeLoginID);
        }
    }

    public enum CustomReportTemplateType
    {
        Report = 1,
        Header = 2,
        Footer = 3
    }
}