using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.Admin
{
    public class ContractAddOnReport
    {
        public int AddOnTypeID { get; set; }
        public string Type { get; set; }
        public int ContractCount { get; set; }

        public ContractAddOnReport()
        {

        }

        public ContractAddOnReport(DataRow row)
        {
            AddOnTypeID = InputHelper.GetInteger(row["AddOnTypeID"].ToString());
            Type = InputHelper.GetString(row["Type"].ToString());
            ContractCount = InputHelper.GetInteger(row["ContractCount"].ToString());
        }

        public static List<ContractAddOnReport> GetForFilter(string contractAddOnDate)
        {
            List<ContractAddOnReport> contractAddOnReport = new List<ContractAddOnReport>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@contractAddOnDate", contractAddOnDate));

            DBAccessTableResult tableResult = db.ExecuteWithTable("ContractAddOnReport", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                contractAddOnReport.Add(new ContractAddOnReport(row));
            }

            return contractAddOnReport;
        }

        public static DataTable GetForFilterData(string contractAddOnDate)
        {
            List<ContractAddOnReport> contractAddOnReport = new List<ContractAddOnReport>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@contractAddOnDate", contractAddOnDate));

            DBAccessTableResult tableResult = db.ExecuteWithTable("ContractAddOnReport", parameters);

            return tableResult.DataTable;
        }
    }
}