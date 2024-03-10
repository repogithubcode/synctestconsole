using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class QBExportSummaryRow
    {

        public int EstimateID { get; private set; }
		public DateTime? ExportDate { get; private set; }
        public bool HasChanges { get; private set; }
        public int ExportRows { get; private set; }
		public string CustomerName { get; private set; }
		public string VIN { get; private set; }
		public string InsuranceCompany { get; private set; }
		public DateTime ClosedRODate { get; private set; }
        public string InsuranceClaimDetails { get; private set; }
        public string QBInvoiceID { get; private set; }

        public QBExportSummaryRow(DataRow row)
        {
            EstimateID = InputHelper.GetInteger(row["EstimateID"].ToString());
            ExportDate = InputHelper.GetDateTimeNullable(row["ExportDate"].ToString());
            HasChanges = InputHelper.GetBoolean(row["HasChanges"].ToString());
            ExportRows = InputHelper.GetInteger(row["ExportRows"].ToString());
            CustomerName = InputHelper.GetString(row["CustomerName"].ToString());
            VIN = InputHelper.GetString(row["VIN"].ToString());
            InsuranceCompany = InputHelper.GetString(row["InsuranceCompany"].ToString());
            ClosedRODate = InputHelper.GetDateTime(row["ClosedRODate"].ToString());
            InsuranceClaimDetails = InputHelper.GetString(row["InsuranceClaimDetails"].ToString());
            QBInvoiceID = InputHelper.GetString(row["QBInvoiceID"].ToString());
        }

        public static FunctionResult QBExportPreprocessing(int loginID, DateTime startDate, DateTime endDate)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("StartDate", startDate));
            parameters.Add(new SqlParameter("EndDate", endDate));

            DBAccess db = new DBAccess();
            FunctionResult functionResult = db.ExecuteNonQuery("QB_Export_PreProcessing", parameters);

            return functionResult;
        }

        public static List<QBExportSummaryRow> GetDataSummary(int loginID, DateTime startDate, DateTime endDate)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("StartDate", startDate));
            parameters.Add(new SqlParameter("EndDate", endDate));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("QB_Export_Summary", parameters);

            List<QBExportSummaryRow> summaryRows = new List<QBExportSummaryRow>();

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                summaryRows.Add(new QBExportSummaryRow(row));
            }

            return summaryRows;
        }
    }
}
