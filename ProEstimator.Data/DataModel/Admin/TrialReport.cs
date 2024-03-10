using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataAdmin
{
    public class TrialReport
    {
        public int Id { get; set; }
        public int SalesRepID { get; set; }
        public string CompanyName { get; set; }
        public string Name { get; set; }
        public int ContractID { get; set; }
        public string TrialStartDate { get; set; }
        public string TrialEndDate { get; set; }
        public int EstimateCount { get; set; }
        public string ContractType { get; set; }
        public bool Trial { get; set; }
        public bool HasConverted { get; set; }

        public TrialReport()
        {

        }

        public TrialReport(DataRow row)
        {
            Id = InputHelper.GetInteger(row["Id"].ToString());
            SalesRepID = InputHelper.GetInteger(row["SalesRepID"].ToString());
            CompanyName = InputHelper.GetString(row["CompanyName"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
            ContractID = InputHelper.GetInteger(row["ContractID"].ToString());
            TrialStartDate = InputHelper.GetString(row["TrialStartDate"].ToString());
            TrialEndDate = InputHelper.GetString(row["TrialEndDate"].ToString());
            EstimateCount = InputHelper.GetInteger(row["EstimateCount"].ToString());
            ContractType = InputHelper.GetString(row["ContractType"].ToString());
            Trial = InputHelper.GetInteger(row["Trial"].ToString()) == 1 ? true : false;
        }

        public static List<TrialReport> GetForFilter(DateTime fromDate, DateTime toDate)
        {
            List<TrialReport> trialReports = new List<TrialReport>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@StartDate", fromDate));
            parameters.Add(new SqlParameter("@EndDate", toDate.AddDays(1)));

            DBAccessTableResult tableResult = db.ExecuteWithTable("[Admin].[GET_TRIAL_REPORT]", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                trialReports.Add(new TrialReport(row));
            }

            return trialReports.OrderByDescending(x => x.TrialStartDate).ToList();
        }
    }
}