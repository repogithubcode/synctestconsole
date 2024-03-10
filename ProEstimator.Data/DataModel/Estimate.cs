using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class Estimate : ProEstEntity
    {
        public int EstimateID { get; set; }

        // AdminInfo fields
        public int CreatedByLoginID { get; set; }
        public string Description { get; set; }
        public int CustomerProfileID { get; set; }
        public int AddOnProfileID { get; set; }
        public string GrandTotalString { get; set; }
        public string BettermentTotalString { get; set; }
        public int EstimateNumber { get; set; }
        public int WorkOrderNumber { get; set; }
        public bool PrintDescription { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
        public string ClaimNumber { get; set; }
        public DateTime LastView { get; set; }
        public int CustomerID { get; set; }
        public bool EstimateIsImported { get; set; }

        //EstimationData fields
        public DateTime? EstimationDate { get; set; }
        public DateTime? DateOfLoss { get; set; }
        public int CoverageType { get; set; }   // TODO - enum?
        public string EstimateLocation { get; set; }
        public string TransactionLevel { get; set; }
        public int LockLevel { get; set; }
        public int LastLineNumber { get; set; }
        public int EstimationLineItemIDLocked { get; set; }
        public string Note { get; set; }
        public bool PrintNote { get; set; }
        public DateTime? AssignmentDate { get; set; }
        public string ReportTextHeader { get; set; }
        public int AlternateIdentitiesID { get; set; }
        public int NextUniqueSequenceNumber { get; set; }
        public string PolicyNumber { get; set; }
        public decimal Deductible { get; set; }
        public int InsuranceCompanyID { get; set; }
        public string InsuranceCompanyName { get; set; }
        public bool ClaimantSameAsOwner { get; set; }
        public bool InsuredSameAsOwner { get; set; }
        public int EstimatorID { get; set; }
        public int RepairFacilityVendorID { get; set; }
        public string ImageSize { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public int AdjusterID { get; set; }
        public int ClaimRepID { get; set; }
        public bool PrintInsured { get; set; }
        public int RepairDays { get; set; }
        public byte HoursInDay { get; set; }
        public string FacilityRepairOrder { get; set; }
        public string FacilityRepairInvoice { get; set; }
        public decimal CreditCardFeePercentage { get; set; }
        public bool ApplyCreditCardFee { get; set; }
        public Boolean TaxedCreditCardFee { get; set; }

        public int CurrentJobStatusID
        {
            get
            {
                if (_currentJobStatus == -1)
                {
                    DBAccess db = new DBAccess();
                    DBAccessTableResult result = db.ExecuteWithTable("GetJobStatusCurrent", new SqlParameter("estimateID", EstimateID));
                    if (result.Success)
                    {
                        _currentJobStatus = InputHelper.GetInteger(result.DataTable.Rows[0]["JobStatusID"].ToString());
                    }
                    else
                    {
                        _currentJobStatus = 1;
                    }
                }

                return _currentJobStatus;
            }
        }
        private int _currentJobStatus = -1;

        public bool IsLocked()
        {
            return CurrentJobStatusID == 3 || CurrentJobStatusID == 4;
        }

        public Estimate()
        {

        }

        public Estimate(int estimateID)
        {
            LoadData(estimateID);
        }

        private void LoadData(int estimateID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("GetAdminInfoAndEstimateData", new SqlParameter("AdminInfoID", estimateID));
            if (tableResult.Success)
            {
                LoadData(tableResult.DataTable.Rows[0]);
            }
        }

        private void LoadData(DataRow row)
        {
            EstimateID = InputHelper.GetInteger(row["id"].ToString());

            // AdminInfo fields
            CreatedByLoginID = InputHelper.GetInteger(row["CreatorID"].ToString()); 
            Description = row["Description"].ToString();
            CustomerProfileID = InputHelper.GetInteger(row["CustomerProfilesID"].ToString());
            AddOnProfileID = InputHelper.GetInteger(row["AddOnProfileID"].ToString());
            GrandTotalString = row["GrandTotal"].ToString();
            BettermentTotalString = row["BettermentTotal"].ToString();
            EstimateNumber = InputHelper.GetInteger(row["EstimateNumber"].ToString());
            WorkOrderNumber = InputHelper.GetInteger(row["WorkOrderNumber"].ToString());
            PrintDescription = InputHelper.GetBoolean(row["PrintDescription"].ToString());
            IsArchived = InputHelper.GetBoolean(row["Archived"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["Deleted"].ToString());
            LastView = InputHelper.GetDateTime(row["LastView"].ToString());
            CustomerID = InputHelper.GetInteger(row["CustomerID"].ToString());
            EstimateIsImported = InputHelper.GetBoolean(row["IsImported"].ToString());

            //EstimationData fields
            EstimationDate = InputHelper.GetNullableDateTime(row["EstimationDate"].ToString());
            DateOfLoss = InputHelper.GetNullableDateTime(row["DateOfLoss"].ToString());
            CoverageType = InputHelper.GetInteger(row["CoverageType"].ToString(), 255);     // 0 is a selection for coverage type, 255 is no selection
            EstimateLocation = row["EstimateLocation"].ToString();
            TransactionLevel = row["TransactionLevel"].ToString();
            LockLevel = InputHelper.GetInteger(row["LockLevel"].ToString());
            LastLineNumber = InputHelper.GetInteger(row["LastLineNumber"].ToString());
            EstimationLineItemIDLocked = InputHelper.GetInteger(row["EstimationLineItemIDLocked"].ToString());
            Note = row["Note"].ToString();
            PrintNote = InputHelper.GetBoolean(row["PrintNote"].ToString());
            AssignmentDate = InputHelper.GetNullableDateTime(row["AssignmentDate"].ToString());
            ReportTextHeader = row["ReportTextHeader"].ToString();
            AlternateIdentitiesID = InputHelper.GetInteger(row["AlternateIdentitiesID"].ToString());
            NextUniqueSequenceNumber = InputHelper.GetInteger(row["NextUniqueSequenceNumber"].ToString());
            ClaimNumber = row["ClaimNumber"].ToString();
            PolicyNumber = row["PolicyNumber"].ToString();
            Deductible = InputHelper.GetDecimal(row["Deductible"].ToString());
            InsuranceCompanyID = InputHelper.GetInteger(row["InsuranceCompanyID"].ToString());
            InsuranceCompanyName = row["InsuranceCompanyName"].ToString();
            ClaimantSameAsOwner = InputHelper.GetBoolean(row["ClaimantSameAsOwner"].ToString());
            InsuredSameAsOwner = InputHelper.GetBoolean(row["InsuredSameAsOwner"].ToString());
            EstimatorID = InputHelper.GetInteger(row["EstimatorID"].ToString());
            RepairFacilityVendorID = InputHelper.GetInteger(row["RepairFacilityVendorID"].ToString());
            ImageSize = row["ImageSize"].ToString();
            PurchaseOrderNumber = row["PurchaseOrderNumber"].ToString();
            AdjusterID = InputHelper.GetInteger(row["AdjusterID"].ToString());
            ClaimRepID = InputHelper.GetInteger(row["ClaimRepID"].ToString());
            PrintInsured = InputHelper.GetBoolean(row["PrintInsured"].ToString());
            RepairDays = row.IsNull("RepairDays") ? -1 : Convert.ToInt32(row["RepairDays"]);
            HoursInDay = Convert.ToByte(row["HoursInDay"]);
            FacilityRepairOrder = row["FacilityRepairOrder"].ToString();
            FacilityRepairInvoice = row["FacilityRepairInvoice"].ToString();
            CreditCardFeePercentage = InputHelper.GetDecimal(row["CreditCardFeePercentage"].ToString());
            ApplyCreditCardFee = InputHelper.GetBoolean(row["ApplyCreditCardFee"].ToString());
            TaxedCreditCardFee = InputHelper.GetBoolean(row["TaxedCreditCardFee"].ToString());

            RowAsLoaded = row;

            if (string.IsNullOrEmpty(ReportTextHeader))
            {
                ReportTextHeader = "";
            }
        }

        public override SaveResult Save(int activeLoginID, int loginID = 0)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("AdminInfoID", EstimateID));
                parameters.Add(new SqlParameter("CreatorID", CreatedByLoginID));
                parameters.Add(new SqlParameter("Description", GetString(Description)));
                parameters.Add(new SqlParameter("CustomerProfilesID", CustomerProfileID));
                parameters.Add(new SqlParameter("AddOnProfileID", AddOnProfileID));
                //parameters.Add(new SqlParameter("GrandTotal", GetString(GrandTotalString)));
                parameters.Add(new SqlParameter("BettermentTotal", GetString(BettermentTotalString)));
                parameters.Add(new SqlParameter("EstimateNumber", EstimateNumber));
                parameters.Add(new SqlParameter("WorkOrderNumber", WorkOrderNumber));
                parameters.Add(new SqlParameter("PrintDescription", PrintDescription));
                parameters.Add(new SqlParameter("Archived", IsArchived));
                parameters.Add(new SqlParameter("Deleted", IsDeleted));
                parameters.Add(new SqlParameter("ClaimNumber", GetString(ClaimNumber)));
                parameters.Add(new SqlParameter("CustomerID", CustomerID));

                SqlParameter lastViewParameter = new SqlParameter("LastView", LastView);
                parameters.Add(lastViewParameter);

                parameters.Add(new SqlParameter("EstimationDate", EstimationDate));
                parameters.Add(new SqlParameter("DateOfLoss", DateOfLoss));
                parameters.Add(new SqlParameter("CoverageType", CoverageType));
                parameters.Add(new SqlParameter("EstimateLocation", GetString(EstimateLocation)));
                parameters.Add(new SqlParameter("TransactionLevel", GetString(TransactionLevel)));
                parameters.Add(new SqlParameter("LockLevel", LockLevel));
                parameters.Add(new SqlParameter("LastLineNumber", LastLineNumber));
                parameters.Add(new SqlParameter("EstimationLineItemIDLocked", EstimationLineItemIDLocked));
                parameters.Add(new SqlParameter("Note", GetString(Note)));
                parameters.Add(new SqlParameter("PrintNote", PrintNote));
                parameters.Add(new SqlParameter("AssignmentDate", AssignmentDate));
                parameters.Add(new SqlParameter("ReportTextHeader", GetString(ReportTextHeader)));
                parameters.Add(new SqlParameter("AlternateIdentitiesID", AlternateIdentitiesID));
                parameters.Add(new SqlParameter("NextUniqueSequenceNumber", NextUniqueSequenceNumber));
                parameters.Add(new SqlParameter("PolicyNumber", GetString(PolicyNumber)));
                parameters.Add(new SqlParameter("Deductible", Deductible));
                parameters.Add(new SqlParameter("InsuranceCompanyID", InsuranceCompanyID));
                parameters.Add(new SqlParameter("InsuranceCompanyName", GetString(InsuranceCompanyName)));
                parameters.Add(new SqlParameter("ClaimantSameAsOwner", ClaimantSameAsOwner));
                parameters.Add(new SqlParameter("InsuredSameAsOwner", InsuredSameAsOwner));
                parameters.Add(new SqlParameter("EstimatorID", EstimatorID));
                parameters.Add(new SqlParameter("RepairFacilityVendorID", RepairFacilityVendorID));
                parameters.Add(new SqlParameter("ImageSize", GetString(ImageSize)));
                parameters.Add(new SqlParameter("PurchaseOrderNumber", PurchaseOrderNumber));
                parameters.Add(new SqlParameter("AdjusterID", AdjusterID));
                parameters.Add(new SqlParameter("ClaimRepID", ClaimRepID));
                parameters.Add(new SqlParameter("PrintInsured", PrintInsured));
                if (RepairDays == -1)
                {
                    parameters.Add(new SqlParameter("RepairDays", DBNull.Value));
                }
                else
                {
                    parameters.Add(new SqlParameter("RepairDays", RepairDays));
                }
                parameters.Add(new SqlParameter("HoursInDay", HoursInDay));
                parameters.Add(new SqlParameter("FacilityRepairOrder", FacilityRepairOrder));
                parameters.Add(new SqlParameter("FacilityRepairInvoice", FacilityRepairInvoice));
                parameters.Add(new SqlParameter("CreditCardFeePercentage", CreditCardFeePercentage));
                parameters.Add(new SqlParameter("ApplyCreditCardFee", ApplyCreditCardFee));
                parameters.Add(new SqlParameter("TaxedCreditCardFee", TaxedCreditCardFee));

                DBAccess db = new DBAccess();
                DBAccessIntResult result = db.ExecuteWithIntReturn("UpdateAdminInfoAndEstimationData", parameters);
                if (result.Success)
                {
                    EstimateID = result.Value;

                    parameters.Remove(lastViewParameter);

                    ChangeLogManager.LogChange(activeLoginID, "Estimate", EstimateID, CreatedByLoginID, parameters, RowAsLoaded);
                }
            }
            catch (System.Exception ex)
            {
                return new SaveResult("Error saving Estimate information: " + ex.Message);
            }

            return new SaveResult();
        }

        /// <summary>
        /// Updates the status for this estimate.  If success, returns an empty string, if there's an error it is returned.
        /// </summary>
        public string UpdateStatus(int activeLoginID, int newStatusID, DateTime timeStamp, int loginsID)
        {
            try
            {
                JobStatusHistory history = new JobStatusHistory();
                history.EstimateID = EstimateID;
                history.JobStatus = JobStatus.GetByID(newStatusID);
                history.ActionDate = timeStamp;
                SaveResult saveResult = history.Save();

                if (saveResult.Success)
                {
                    LoadData(EstimateID);

                    // Update the Estimate's work order number
                    if (newStatusID == 2 && WorkOrderNumber == 0)
                    {
                        LoginInfo loginInfo = LoginInfo.GetByID(loginsID);
                        loginInfo.LastWorkOrderNumber++;
                        loginInfo.Save();

                        WorkOrderNumber = loginInfo.LastWorkOrderNumber;
                    }
                    
                    // Update the header
                    if (newStatusID == 2)
                    {
                        ReportTextHeader = "Repair Order";
                    }
                    else if (newStatusID == 3)
                    {
                        ReportTextHeader = "Closed Repair Order";
                    }
                    else if (newStatusID == 1)
                    {
                        ReportTextHeader = "Estimate";
                    }

                    Save(activeLoginID);
                }

                return saveResult.ErrorMessage;
            }
            catch (System.Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return ex.InnerException.Message;
                }

                return ex.Message;
            }
        }

        public string CommitEstimate()
        {
            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Estimate_Commit", new SqlParameter("AdminInfoID", EstimateID));

            if (result.Success)
            {
                if (result.Value == 1)
                {
                    return "Estimate not committed since there are no items on the estimate.";
                }
                else if (result.Value == 2)
                {
                    return "Estimate not committed since there are no items on the current supplement level.";
                }
            }
            
            return "";
        }

        public string UncommitSupplements()
        {
            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Estimate_UncommitSupplements", new SqlParameter("AdminInfoID", EstimateID));

            if (result.Success)
            {
                if (result.Value == 1)
                {
                    return "Estimate can't uncommit when there is no supplement over estimate.";
                }
                else if (result.Value == 2)
                {
                    return "Estimate can't uncommit since there are items on the estimate over supplement.";
                }
            }

            return "";
        }

        public string CheckCommitUncommit()
        {
            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Estimate_CheckCommitUncommit", new SqlParameter("AdminInfoID", EstimateID));

            if (result.Success)
            {
                if (result.Value == 1)
                {
                    return "Commit";
                }
                else if (result.Value == 2)
                {
                    return "Uncommit";
                }
            }

            return "";
        }

        public static string GetStatusName(int jobStatusID)
        {
            lock(_loadLock)
            {
                if (_statuses == null || _statuses.Count == 0)
                {
                    DBAccess db = new DBAccess();
                    DBAccessTableResult tableResult = db.ExecuteWithTable("GetJobStatuses");
                    if (tableResult.Success)
                    {
                        _statuses = new Dictionary<int, string>();

                        foreach (DataRow row in tableResult.DataTable.Rows)
                        {
                            int id = InputHelper.GetInteger(row["id"].ToString());
                            string description = row["Description"].ToString();

                            if (!_statuses.ContainsKey(id))
                            {
                                _statuses.Add(id, description);
                            }
                        }
                    }
                }
            }

            return _statuses[jobStatusID];
        }

        private static object _loadLock = new object();
        private static Dictionary<int, string> _statuses;

        public static int GetCountForCustomer(int loginID, int customerID)
        {
            if (loginID == 0 || customerID == 0)
            {
                return 0;
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginsID", loginID));
            parameters.Add(new SqlParameter("CustomerID", customerID));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("sp_GetEstimateList", parameters);

            if (tableResult.Success)
            {
                return tableResult.DataTable.Rows.Count;
            }

            return 0;
        }

        public static int GetCountForLogin(int loginID)
        {
            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("Estimate_GetTotalForLogin", new SqlParameter("LoginID", loginID));
            return intResult.Value;
        }

        public static void RefreshProcessedLines(int estimateID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("FillProcessedLines", parameters);
        }

        public static void ProcessIfNeeded(int estimateID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("FillProcessedLinesIfNeeded", parameters);
        }

        public static void MapEstimateIdQuickbookInvoiceID(int estimateID, int docNumber, string qbtype)
        {
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));
            parameters.Add(new SqlParameter("QuickbookInvoiceID", docNumber));
            parameters.Add(new SqlParameter("QuickbookType", qbtype));

            DBAccessIntResult result = db.ExecuteWithIntReturn("Map_EstimateID_QuickbookInvoiceID", parameters);
        }

        public int GetSupplementForReport()
        {
            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("Estimate_GetHighestUsedEstimate", new System.Data.SqlClient.SqlParameter("AdminInfoID", EstimateID));

            if (intResult.Success)
            {
                if (LockLevel > intResult.Value)
                {
                    return intResult.Value;
                }
            }
            return LockLevel;
        }
    }
    
}