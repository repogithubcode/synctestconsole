using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimatorData.DataModel.Admin
{
    public class RenewalDetails
    {
        public int LoginID { get; private set; }
		public int ContractID { get; private set; }
		public double ContractTotal	{ get; private set; }
		public double TotalPaid	{ get; private set; }
		public double TotalDue { get; private set; }
		public DateTime ContractStart { get; private set; }
		public DateTime ContractEnd { get; private set; }
        public int TotalYears { get; private set; }
        public bool DidRenew { get; private set; }
		public int SalesRepID { get; private set; }
		public string SalesRep { get; private set; }
		public string CompanyName { get; private set; }
		public string Contact { get; private set; }
		public string PhoneNumber { get; private set; }
		public string State	{ get; private set; }
		public bool HasFrame { get; private set; }
		public bool HasEMS { get; private set; }
		public bool HasMU { get; private set; }
        public bool HasQB { get; private set; }
        public bool HasPA { get; private set; }
        public bool HasIE { get; private set; }
        public bool HasER { get; private set; }
        public bool HasBundle { get; private set; }
        public int TotalEstimates { get; private set; }
        public int CurrentEstimates { get; private set; }
		public bool WillRenew { get; set; }
        public bool WillNotRenew { get; set; }
        public string Source { get; set; }
        public string Notes { get; set; }
        public DateTime? PETrial { get; set; }
        public bool AutoRenew { get; set; }
        public bool IsContractSigned { get; set; }

        public RenewalDetails(DataRow row)
        {
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            ContractID = InputHelper.GetInteger(row["ContractID"].ToString());
            ContractTotal = InputHelper.GetDouble(row["ContractTotal"].ToString());
            TotalPaid = InputHelper.GetDouble(row["TotalPaid"].ToString());
            TotalDue = InputHelper.GetDouble(row["TotalDue"].ToString());
            ContractStart = InputHelper.GetDateTime(row["ContractStart"].ToString()).Date;
            ContractEnd = InputHelper.GetDateTime(row["ContractEnd"].ToString()).Date;
            TotalYears = InputHelper.GetInteger(row["TotalYears"].ToString());
            DidRenew = InputHelper.GetBoolean(row["DidRenew"].ToString());
            SalesRepID = InputHelper.GetInteger(row["SalesRepID"].ToString());
            SalesRep = InputHelper.GetString(row["SalesRep"].ToString());
            CompanyName = InputHelper.GetString(row["CompanyName"].ToString());
            Contact = InputHelper.GetString(row["Contact"].ToString());
            PhoneNumber = InputHelper.GetString(row["PhoneNumber"].ToString());
            State = InputHelper.GetString(row["State"].ToString());
            HasFrame = InputHelper.GetBoolean(row["HasFrame"].ToString());
            HasEMS = InputHelper.GetBoolean(row["HasEMS"].ToString());
            HasMU = InputHelper.GetBoolean(row["HasMU"].ToString());
            HasQB = InputHelper.GetBoolean(row["HasQB"].ToString());
            HasPA = InputHelper.GetBoolean(row["HasPA"].ToString());
            HasIE = InputHelper.GetBoolean(row["HasIE"].ToString());
            HasER = InputHelper.GetBoolean(row["HasER"].ToString());
            HasBundle = InputHelper.GetBoolean(row["HasBundle"].ToString());
            TotalEstimates = InputHelper.GetInteger(row["TotalEstimates"].ToString());
            CurrentEstimates = InputHelper.GetInteger(row["CurrentEstimates"].ToString());
            WillRenew = InputHelper.GetBoolean(row["WillRenew"].ToString());
            WillNotRenew = InputHelper.GetBoolean(row["WillNotRenew"].ToString());
            Source = InputHelper.GetString(row["Source"].ToString());
            Notes = InputHelper.GetString(row["Notes"].ToString());
            PETrial = InputHelper.GetDateTimeNullable(row["PETrial"].ToString());
            AutoRenew = InputHelper.GetBoolean(row["AutoRenew"].ToString());
            IsContractSigned = InputHelper.GetBoolean(row["IsContractSigned"].ToString());
        }

        public SaveResult Save()
        {
            Contract contract = Contract.Get(ContractID);
            if (contract == null)
            {
                return new SaveResult("Contract " + ContractID + " not found.");
            }

            contract.WillRenew = WillRenew;
            contract.WillNotRenew = WillNotRenew;
            contract.Notes = Notes;
            return contract.Save();
        }

        public static List<RenewalDetails> GetRenewalReport(int year, int month, int salesRepID = -1)
        {
            DataTable table = GetRenewalReportData(year, month, salesRepID);
            List<RenewalDetails> details = new List<RenewalDetails>();

            foreach (DataRow row in table.Rows)
            {
                details.Add(new RenewalDetails(row));
            }

            return details;
        }

        public static DataTable GetRenewalReportData(int year, int month, int salesRepID = -1)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Year", year));
            parameters.Add(new SqlParameter("Month", month));
            parameters.Add(new SqlParameter("SalesRepID", salesRepID));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("RenewalReport_Get", parameters);
            return tableResult.DataTable;
        }
    }
}
