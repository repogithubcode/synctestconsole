using System.Collections.Generic;
using System.Data;

namespace ProEstimatorData.DataModel.Admin
{
    public class SalesBoardReport
    {
        public int Year { get; set; }
        public int Jan { get; set; }
        public int Feb { get; set; }
        public int Mar { get; set; }
        public int Apr { get; set; }
        public int May { get; set; }
        public int Jun { get; set; }
        public int Jul { get; set; }
        public int Aug { get; set; }
        public int Sept { get; set; }
        public int Oct { get; set; }
        public int Nov { get; set; }
        public int Dec { get; set; }
        public int? Total { get; set; }

        public SalesBoardReport()
        {

        }

        public SalesBoardReport(DataRow row)
        {
            Year = InputHelper.GetInteger(row["Year"].ToString());
            Jan = InputHelper.GetInteger(row["Jan"].ToString());
            Feb = InputHelper.GetInteger(row["Feb"].ToString());
            Mar = InputHelper.GetInteger(row["Mar"].ToString());
            Apr = InputHelper.GetInteger(row["Apr"].ToString());
            May = InputHelper.GetInteger(row["May"].ToString());
            Jun = InputHelper.GetInteger(row["Jun"].ToString());
            Jul = InputHelper.GetInteger(row["Jul"].ToString());
            Aug = InputHelper.GetInteger(row["Aug"].ToString());
            Sept = InputHelper.GetInteger(row["Sep"].ToString());
            Oct = InputHelper.GetInteger(row["Oct"].ToString());
            Nov = InputHelper.GetInteger(row["Nov"].ToString());
            Dec = InputHelper.GetInteger(row["Dec"].ToString());
            Total = InputHelper.GetInteger(row["Total"].ToString());
        }

        public static List<SalesBoardReport> GetForFilter()
        {
            List<SalesBoardReport> salesBoardReports = new List<SalesBoardReport>();
            DBAccess db = new DBAccess();

            DBAccessTableResult tableResult = db.ExecuteWithTable("getSalesBoard");

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                salesBoardReports.Add(new SalesBoardReport(row));
            }

            return salesBoardReports;
        }
    }
}