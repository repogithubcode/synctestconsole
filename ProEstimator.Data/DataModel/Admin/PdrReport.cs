using System.Collections.Generic;
using System.Data;

namespace ProEstimatorData.DataModel.Admin
{
    public class PdrReport
    {
        public int LoginId { get; set; }
        public string CompanyName { get; set; }
        public string SalesRep { get; set; }
        public int NumberOfEstimates { get; set; }
        public string Date { get; set; }

        public PdrReport()
        {

        }

        public PdrReport(DataRow row)
        {
            LoginId = InputHelper.GetInteger(row["LoginId"].ToString());
            CompanyName = InputHelper.GetString(row["CompanyName"].ToString());
            SalesRep = InputHelper.GetString(row["SalesRep"].ToString());
            NumberOfEstimates = InputHelper.GetInteger(row["NumberOfEstimates"].ToString());
            Date = InputHelper.GetString(row["Date"].ToString());
        }

        public static List<PdrReport> GetForFilter()
        {
            List<PdrReport> pdrReports = new List<PdrReport>();
            DBAccess db = new DBAccess();

            DBAccessTableResult tableResult = db.ExecuteWithTable("[Admin].[GET_PdrReport]");

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                pdrReports.Add(new PdrReport(row));
            }

            return pdrReports;
        }

        public static DataTable GetForFilterData()
        {
            List<PdrReport> pdrReports = new List<PdrReport>();
            DBAccess db = new DBAccess();

            DBAccessTableResult tableResult = db.ExecuteWithTable("[Admin].[GET_PdrReport]");

            return tableResult.DataTable;
        }
    }
}