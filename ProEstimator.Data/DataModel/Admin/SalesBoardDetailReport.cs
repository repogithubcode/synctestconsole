using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    public class SalesBoardDetailReport
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int Frame { get; set; }
        public int Ems { get; set; }
        public int MultiUser { get; set; }
        public string SalesRepID { get; set; }
        public string CompanyName { get; set; }
        public int Bundle { get; set; }
        public int QBExporter { get; set; }
        public int ProAdvisor { get; set; }
        public int ImageEditor { get; set; }
        public int EnterpriseReporting { get; set; }

        public SalesBoardDetailReport()
        {

        }

        public SalesBoardDetailReport(DataRow row)
        {
            Name = InputHelper.GetString(row["Name"].ToString());
            Count = InputHelper.GetInteger(row["Count"].ToString());
            SalesRepID = InputHelper.GetString(row["SalesRepID"].ToString());

            Frame = InputHelper.GetInteger(row["Frame"].ToString());
            Ems = InputHelper.GetInteger(row["Ems"].ToString());
            MultiUser = InputHelper.GetInteger(row["MultiUser"].ToString());
            Bundle = InputHelper.GetInteger(row["Bundle"].ToString());
            QBExporter = InputHelper.GetInteger(row["QBExporter"].ToString());
            ProAdvisor = InputHelper.GetInteger(row["ProAdvisor"].ToString());
            ImageEditor = InputHelper.GetInteger(row["ImageEditor"].ToString());
            EnterpriseReporting = InputHelper.GetInteger(row["EnterpriseReporting"].ToString());
        }

        public static List<SalesBoardDetailReport> GetForFilter(string date)
        {
            List<SalesBoardDetailReport> salesBoardDetailReports = new List<SalesBoardDetailReport>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("@date", Common.GetParameterValue(date)) { SqlDbType = SqlDbType.Date });
            DBAccessTableResult tableResult = db.ExecuteWithTable("[admin].[getSalesBoardDetail]", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                salesBoardDetailReports.Add(new SalesBoardDetailReport(row));
            }

            return salesBoardDetailReports;
        }

        public static FunctionResult InsertSalesBoard(string loginID, string est, string frame, string ems, string salesRep, 
                                                                string dateSold, string addUser, string hasQBExporter, string hasProAdvisor, string hasBundle, string hasImageEditor, string hasReporting)
        {
            DataTable dt = new DataTable();

            DBAccess db = new DBAccess();


            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", InputHelper.GetInteger(loginID)));
            parameters.Add(new SqlParameter("Est", InputHelper.GetBoolean(est)));
            parameters.Add(new SqlParameter("Frame", InputHelper.GetBoolean(frame)));
            parameters.Add(new SqlParameter("EMS", InputHelper.GetBoolean(ems)));
            parameters.Add(new SqlParameter("SalesRep", InputHelper.GetInteger(salesRep)));
            parameters.Add(new SqlParameter("DateSold", dateSold));
            parameters.Add(new SqlParameter("AddUser", InputHelper.GetBoolean(addUser)));
            parameters.Add(new SqlParameter("HasQBExporter", InputHelper.GetBoolean(hasQBExporter)));
            parameters.Add(new SqlParameter("ProAdvisor", InputHelper.GetBoolean(hasProAdvisor)));
            parameters.Add(new SqlParameter("Bundle", InputHelper.GetBoolean(hasBundle)));
            parameters.Add(new SqlParameter("ImageEditor", InputHelper.GetBoolean(hasImageEditor)));
            parameters.Add(new SqlParameter("EnterpriseReporting", InputHelper.GetBoolean(hasReporting)));

            FunctionResult functionResult = db.ExecuteNonQuery("InsertSalesBoard", parameters);

            return functionResult;
        }
    }
}