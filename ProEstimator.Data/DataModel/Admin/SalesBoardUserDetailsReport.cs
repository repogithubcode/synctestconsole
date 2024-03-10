using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    public class SalesBoardUserDetailsReport
    {
        public string Name { get; set; }
        public int SalesBoardID { get; set; }
        public int LoginId { get; set; }
        public int? NumberSold { get; set; }
        public string CompanyName { get; set; }
        public string DateSold { get; set; }

        public int? Frame { get; set; }
        public int? Ems { get; set; }
        public int AddUser { get; set; }
        
        
        public int ProE { get; set; }
        public int HasBundle { get; set; }
        public int HasQBExporter { get; set; }
        public int HasProAdvisor { get; set; }
        public int HasImages { get; set; }
        public int HasReporting { get; set; }

        public SalesBoardUserDetailsReport()
        {

        }

        public SalesBoardUserDetailsReport(DataRow row)
        {
            Name = InputHelper.GetString(row["Name"].ToString());
            SalesBoardID = InputHelper.GetInteger(row["salesBoardID"].ToString());
            LoginId = InputHelper.GetInteger(row["loginID"].ToString());
            CompanyName = InputHelper.GetString(row["Company"].ToString());
            NumberSold = InputHelper.GetInteger(row["numberSold"].ToString());        
            DateSold = InputHelper.GetDateTime(row["dateSold"].ToString()).ToShortDateString();

            Frame = InputHelper.GetInteger(row["frame"].ToString());
            Ems = InputHelper.GetInteger(row["ems"].ToString());
            AddUser = InputHelper.GetInteger(row["addUser"].ToString());

            ProE = InputHelper.GetInteger(row["ProE"].ToString());
            HasBundle = InputHelper.GetInteger(row["bundle"].ToString());
            HasQBExporter = InputHelper.GetInteger(row["HasQBExporter"].ToString());
            HasProAdvisor = InputHelper.GetInteger(row["proAdvisor"].ToString());
            HasImages = InputHelper.GetInteger(row["imageEditor"].ToString());
            HasReporting = InputHelper.GetInteger(row["enterpriseReporting"].ToString());
        }

        public static List<SalesBoardUserDetailsReport> GetForFilter(string salesRepID, string date)
        {
            List<SalesBoardUserDetailsReport> salesBoardUserReports = new List<SalesBoardUserDetailsReport>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@SalesRepID", Common.GetParameterValue(salesRepID)));
            parameters.Add(new SqlParameter("@date", Common.GetParameterValue(date)));

            DBAccessTableResult tableResult = db.ExecuteWithTable("getSalesBoardUser", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                salesBoardUserReports.Add(new SalesBoardUserDetailsReport(row));
            }

            return salesBoardUserReports;
        }

        public static List<SalesBoardUserDetailsReport> GetForDateFilter(string date)
        {
            List<SalesBoardUserDetailsReport> salesBoardUserReports = new List<SalesBoardUserDetailsReport>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@date", Common.GetParameterValue(date)));

            DBAccessTableResult tableResult = db.ExecuteWithTable("getSalesBoardAll", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                salesBoardUserReports.Add(new SalesBoardUserDetailsReport(row));
            }

            return salesBoardUserReports;
        }

        public static FunctionResult DeleteSalesBoardUser(int salesBoardID)
        {
            DBAccess db = new DBAccess();
            FunctionResult functionResult = db.ExecuteNonQuery("delSalesBoard", new SqlParameter("salesBoardID", salesBoardID));

            return functionResult;
        }
    }
}