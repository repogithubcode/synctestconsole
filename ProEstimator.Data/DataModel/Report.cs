using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimatorData.DataModel
{
    public class Report : ProEstEntity
    {

        public int ID { get; set; }
        public int EstimateID { get; set; }
        public ReportType ReportType { get; set; }
        public int CustomReportTemplateID { get; set; }
        public string FileName { get; set; }
        public DateTime DateCreated { get; private set; }

        public DateTime? DeleteStamp { get; private set; }
        public bool IsDeleted { get { return DeleteStamp.HasValue; } }

        public bool ReportExists { get; private set; }

        public bool ImagesOnlyChecked { get; set; }

        public string ReportFooterHtmlFilePath { get; set; }
        public string Notes { get; set; }
        public string Version { get; set; }
        public int ContractID { get; set; }
        public int LoginID { get; set; }

        /// <summary>
        /// Gets the disk path of the report file
        /// </summary>
        public string GetDiskPath()
        {
            Estimate estimate = new Estimate(EstimateID);
            return Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), estimate.CreatedByLoginID.ToString(), EstimateID.ToString(), "Reports", FileName + "." + GetFileExtension());
        }

        /// <summary>
        /// Gets the disk path of the report file without extension
        /// </summary>
        public string GetDiskPath(string filename)
        {
            Estimate estimate = new Estimate(EstimateID);
            return Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), estimate.CreatedByLoginID.ToString(), EstimateID.ToString(), "Reports", filename);
        }

        /// <summary>
        /// Gets the disk path of the contract report file
        /// </summary>
        public string GetContractReportDiskPath()
        {
            return Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), LoginID.ToString(), ContractID.ToString(), "Reports", FileName + "." + GetFileExtension());
        }

        /// <summary>
        /// Gets the disk path of the report file
        /// </summary>
        public string GetPreviewReportDiskPath()
        {
            Estimate estimate = new Estimate(EstimateID);
            return Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), CustomReportTemplateID.ToString(), "Reports", FileName + "." + GetFileExtension());
        }

        public string GetFileExtension()
        {
            if (ReportType != null && ReportType.Tag == "EMS")
            {
                return "zip";
            }
            else
            {
                return "pdf";
            }
        }

        public string GetContentType()
        {
            if (ReportType.Tag == "EMS")
            {
                return "application/zip";
            }
            else
            {
                return "application/pdf";
            }
        }

        public Report()
        {
            DateCreated = DateTime.Now;
            Version = "";
        }

        public Report(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            EstimateID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            string reportType = InputHelper.GetString(row["ReportType"].ToString());
            ReportType = ReportType.GetAll().FirstOrDefault(o => o.Tag == (reportType.IndexOf(" ") > 0 ? reportType.Substring(0, reportType.IndexOf(" ")) : reportType));
            if(ReportType == null)
            {
                ReportType = ReportType.GetAll2().FirstOrDefault(o => o.Tag == (reportType.IndexOf(" ") > 0 ? reportType.Substring(0, reportType.IndexOf(" ")) : reportType));
            }
            CustomReportTemplateID = InputHelper.GetInteger(row["CustomReportTemplateID"].ToString());
            FileName = InputHelper.GetString(row["FileName"].ToString());
            DateCreated = InputHelper.GetDateTime(row["DateCreated"].ToString());
            DeleteStamp = InputHelper.GetNullableDateTime(row["DeleteStamp"].ToString());
            ImagesOnlyChecked = InputHelper.GetBoolean(row["ImagesOnlyChecked"].ToString());
            Notes = InputHelper.GetString(row["Notes"].ToString());
            ContractID = InputHelper.GetInteger(row["ContractID"].ToString());

            Contract contract = Contract.Get(ContractID);
            if(contract != null)
            {
                LoginID = contract.LoginID;
            }

            RowAsLoaded = row;
        }


        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("EstimateID", EstimateID));
            parameters.Add(new SqlParameter("ReportType", ReportType == null ? "" : ReportType.Tag + Version));
            parameters.Add(new SqlParameter("CustomReportTemplateID", CustomReportTemplateID));
            parameters.Add(new SqlParameter("FileName", FileName));
            parameters.Add(new SqlParameter("DateCreated", DateCreated));
            parameters.Add(new SqlParameter("DeleteStamp", DeleteStamp));
            parameters.Add(new SqlParameter("ImagesOnlyChecked", ImagesOnlyChecked));
            parameters.Add(new SqlParameter("Notes", Notes == null ? "" : Notes));
            parameters.Add(new SqlParameter("ContractID", ContractID));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Report_AddOrUpdate", parameters);
            if (result.Success)
            {
                ID = result.Value;
                ChangeLogManager.LogChange(activeLoginID, "Report", ID, ChangeLogManager.GetLoginIDFromEstimate(EstimateID), parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        /// <summary>
        /// Returns True if this report is attached to a sent e-mail.  These reports should not be deleted.
        /// </summary>
        public bool IsAttachedToSentEmail()
        {
            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Report_GetAttachmentCount", new SqlParameter("ReportID", this.ID));
            if (result.Success && result.Value > 0)
            {
                return true;
            }

            return false;
        }

        public void Delete()
        {
            //if (!IsAttachedToSentEmail())
            //{
            //    string diskPath = GetDiskPath();
            //    if (System.IO.File.Exists(diskPath))
            //    {
            //        try
            //        {
            //            System.IO.File.Delete(diskPath);
            //        }
            //        catch (System.Exception ex)
            //        {
            //            ErrorLogger.LogError(ex, 0, 0, "Report Delete");
            //        }
            //    }
            //}

            DeleteStamp = DateTime.Now;
            Save();
        }

        public bool ReportBelongsToLogin(int loginID)
        {
            Estimate estimate = new Estimate(this.EstimateID);
            if (estimate.CreatedByLoginID == loginID)
            {
                return true;
            }
            return false;
        }

        public bool ReportContractBelongsToLogin(int loginID)
        {
            Contract contract = Contract.Get(this.ContractID);
            if (contract.LoginID == loginID)
            {
                return true;
            }
            return false;
        }

        public static Report Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Report_Get", new SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return new Report(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<Report> GetReportsForEstimate(int estimateID, bool includeDeleted = false)
        {
            List<Report> returnList = new List<Report>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("EstimateID", estimateID));
            parameters.Add(new SqlParameter("IncludeDeleted", includeDeleted));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Report_GetByEstimate", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new Report(row));
                }
            }

            return returnList;
        }

        public static List<Report> GetReportsForContract(int contractID)
        {
            List<Report> returnList = new List<Report>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractID", contractID));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Report_GetByContract", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new Report(row));
                }
            }

            return returnList;
        }

        public static List<Report> GetReportsForEmail(int emailID)
        {
            List<Report> returnList = new List<Report>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Report_GetByEmail", new SqlParameter("EmailID", emailID));

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new Report(row));
                }
            }

            return returnList;
        }

        /// <summary>
        /// Generate a name for the report.  The format will be "FirstLast_MakeModel_ReportType_ClaimNumber"
        /// Report type is only added for non "estimate" reports.  Claim number is only added when one exists.
        /// </summary>
        public static string GetReportName(int estimateID, string reportType)
        {
            Estimate estimate = new Estimate(estimateID);

            string reportName = "";

            if (estimate.CustomerID > 0)
            {
                Customer customer = Customer.Get(estimate.CustomerID);

                if (customer != null)
                {
                    // Put together a report name with some descriptive information.
                    // FirstLast_MakeModel_ClaimNumber
                    reportName = customer.Contact.FirstName + customer.Contact.LastName;
                }
            }

            string makeModel = ProEstimatorData.DataModel.Vehicle.GetMakeAndModelString(estimateID).Replace(" ", "");
            if (!string.IsNullOrEmpty(makeModel))
            {
                reportName += "_" + makeModel;
            }

            if (reportType != "Estimate")
            {
                reportName += "_" + reportType;
            }

            if (!string.IsNullOrEmpty(estimate.ClaimNumber))
            {
                reportName += "_" + estimate.ClaimNumber;
            }

            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9_]");
            reportName = rgx.Replace(reportName, "");

            return reportName;
        }

        /// <summary>
        /// Generate a unique name for the report.  The format will be "FirstLast_MakeModel_ReportType_ClaimNumber"
        /// Report type is only added for non "estimate" reports.  Claim number is only added when one exists.
        /// </summary>
        public static string GetUniqueReportName(int estimateID, string reportType)
        {
            string reportName = GetReportName(estimateID, reportType);

            // To make sure the report name is unique, get the total number of reports for this estimate and use the count as a last piece of info
            List<Report> existingReports = Report.GetReportsForEstimate(estimateID);
            reportName += "_" + (existingReports.Where(o => o.ReportType != null && o.ReportType.Tag == reportType).ToList().Count + 1).ToString();

            return reportName;
        }

        public void Restore()
        {
            DeleteStamp = null;
            Save();
        }

        public static string GetUniqueContractReportName(int contractID, string reportType)
        {
            string reportName = GetReportNameForContract(contractID, reportType);

            // To make sure the report name is unique, get the total number of reports for this estimate and use the count as a last piece of info
            List<Report> existingReports = Report.GetReportsForContract(contractID);
            reportName += "_" + (existingReports.Where(o => o.ReportType != null && o.ReportType.Tag == reportType).ToList().Count + 1).ToString();

            return reportName;
        }

        public static string GetReportNameForContract(int contractID, string reportType)
        {
            Contract contract = Contract.GetHeaderContractInvoicesReport(contractID);

            string reportName = "";

            if (contract != null)
            {
                // Put together a report name with some descriptive information.
                // FirstLast_MakeModel_ClaimNumber
                reportName = contract.CompanyName + "_" + contract.CompanyContactName;
            }

            if (!string.IsNullOrEmpty(contract.ContractTermDescription))
            {
                reportName += "_" + contract.ContractTermDescription;
            }

            if (reportType != "Estimate")
            {
                reportName += "_" + reportType;
            }

            if (contract.EffectiveDate != null)
            {
                reportName += "_" + contract.EffectiveDate;
            }

            if (contract.ExpirationDate != null)
            {
                reportName += "_" + contract.ExpirationDate;
            }

            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9_]");
            reportName = rgx.Replace(reportName, "");

            return reportName;
        }

    }

    public class ReportFunctionResult : FunctionResult
    {
        public Report Report { get; private set; }
        public string ReportFullName { get; set; }

        public ReportFunctionResult()
        {
            base.ErrorMessage = "";
            base.Success = true;
        }

        public ReportFunctionResult(string errorMessage)
        {
            base.ErrorMessage = errorMessage;
            base.Success = false;
        }

        public ReportFunctionResult(Report report)
        {
            Report = report;
            ReportFullName = report.FileName + "." + report.GetFileExtension();
        }
    }
}