using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ProEstimator.Business.Extension;
using ProEstimator.Business.Model.Admin;
using ProEstimatorData;

namespace ProEstimator.Business.Logic.Admin
{
    public  class SalesBoardReportService
    {
       public const string SalesReport_TABLE = "SalesBoard";
       public const string SalesReportAll_TABLE = "salesboard";

       /// <summary>
       ///Get Sales Board Report
       /// </summary>
       public DataTable GetSales()
       {
           var result = new DataTable();
           DBAccess db = new DBAccess();

           var item = db.ExecuteWithTable("getSalesBoard");
           //var item = db.ExecuteWithTable(AdminConstants.Get_SalesBoard);
           if (item != null && item.DataTable != null)
           {
               item.DataTable.TableName = SalesReport_TABLE;
               return item.DataTable;
           }
           return result;
       }

       public List<VmSalesBoardReport> GetSalesReport()
       {
           var dt = GetSales();
           var result = (from DataRow row in dt.Rows select row.ToModel<VmSalesBoardReport>()).ToList();

           result = result.OrderByDescending(item => item.Year).ToList();
           return result;
       }

       /// <summary>
       /// Get All Sales Board Report
       /// </summary>
       public DataTable GetSalesBoardAll(string date)
       {
           var result = new DataTable();
           DBAccess db = new DBAccess();
           List<SqlParameter> parameters = new List<SqlParameter>();
           parameters.Add(new SqlParameter("date", date));

           var item = db.ExecuteWithTable("getSalesBoardAll", parameters);
           //var item = db.ExecuteWithTable(AdminConstants.Get_SalesBoardAll,parameters);
           if (item != null && item.DataTable != null)
           {
               item.DataTable.TableName = SalesReportAll_TABLE;
               return item.DataTable;
           }
           return result;
       }

       public List<VmSalesBoardAll> GetSalesBoardAllReport(string date)
       {
           var dt = GetSalesBoardAll(date);
           var result = (from DataRow row in dt.Rows select row.ToModel<VmSalesBoardAll>()).ToList();
           if (
               string.IsNullOrEmpty(date))
           {
               result = result.OrderByDescending(item => item.Name).ToList();
           }

           return result;
       }
         /// <summary>
        /// Get Sales Board Detail 
       /// </summary>
       public DataTable GetSalesDetail(string date)
       {
           var result = new DataTable();
           DBAccess db = new DBAccess();
           List<SqlParameter> parameters = new List<SqlParameter>();
           parameters.Add(new SqlParameter("date", date));

           var item = db.ExecuteWithTable("[admin].[getSalesBoardDetail]", parameters);
           //var item = db.ExecuteWithTable(AdminConstants.Get_SalesBoardDetail, parameters);
           if (item != null && item.DataTable != null)
           {
               item.DataTable.TableName = SalesReportAll_TABLE;
               return item.DataTable;
           }
           return result;
       }

       public List<VmSalesBoardDetail> GetSalesBoardDetail(string date)
       {
           var dt = GetSalesDetail(date);
           var result = (from DataRow row in dt.Rows select row.ToModel<VmSalesBoardDetail>()).ToList();
           if (
               string.IsNullOrEmpty(date))
           {
               result = result.OrderByDescending(item => item.Name).ToList();
           }

           return result;
       }

        /// <summary>
        /// Get Sales Board User 
        /// </summary>
       public DataTable GetUser(string date, int SalesRepID)
       {
           var result = new DataTable(); 
           DBAccess db = new DBAccess();
           List<SqlParameter> parameters = new List<SqlParameter>();
           parameters.Add(new SqlParameter("date", date));
           parameters.Add(new SqlParameter("SalesRepID", SalesRepID));

           var item = db.ExecuteWithTable("getSalesBoardUser", parameters);
           //var item = db.ExecuteWithTable(AdminConstants.Get_SalesBoardUser, parameters);
           if (item != null && item.DataTable != null)
           {
               item.DataTable.TableName = SalesReportAll_TABLE;
               return item.DataTable;
           }

           return result;
       }

       public List<VmSalesBoardUser> GetSalesUser(string date, int SalesRepID)
       {
           var dt = GetUser(date, SalesRepID);
           var result = (from DataRow row in dt.Rows select row.ToModel<VmSalesBoardUser>()).ToList();
           if (
               string.IsNullOrEmpty(date))
           {
               result = result.OrderByDescending(item => item.SalesBoardId).ToList();
           }

           return result;
       }


       /// <summary>
       /// Delete Sales Board User  
       /// </summary>
       public DataTable DeleteUser(int salesBoardID)
       {
           var result = new DataTable();
           DBAccess db = new DBAccess();
           List<SqlParameter> parameters = new List<SqlParameter>();
           parameters.Add(new SqlParameter("salesBoardID", salesBoardID));
           
           var item = db.ExecuteWithTable("delSalesBoard", parameters);
           if (item != null && item.DataTable != null)
           {
               item.DataTable.TableName = SalesReportAll_TABLE;
               return item.DataTable;
           }
           return result;
       }
       public void DeleteSalesUser(int salesBoardID)
       {
           var dt = DeleteUser(salesBoardID);
      
       }
       public DataTable GetSalesReps()
       {
           var result = new DataTable();
           DBAccess db = new DBAccess();
           List<SqlParameter> parameters = new List<SqlParameter>();
           parameters.Add(new SqlParameter("SalesRepID", null));

           var item = db.ExecuteWithTable("GetSalesReps2", parameters);
           if (item != null && item.DataTable != null)
           {
               item.DataTable.TableName = SalesReportAll_TABLE;
               return item.DataTable;
           }

           return result;
       }
   }
}
