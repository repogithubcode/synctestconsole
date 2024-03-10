using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Logic
{
    public static class WebEstMonthlySalesService
    {
        public static void InsertWebEstMonthlySales(int contractID)
        {
            DBAccess db = new DBAccess();

            try
            {
                List<SqlParameter> insertParameters = new List<SqlParameter>();
                insertParameters.Add(new SqlParameter("ContractID", contractID));

                db.ExecuteNonQuery("WebEstMonthlySales_Insert", insertParameters);
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, contractID, "WebEstMonthlySales InsertWebEstMonthlySales");
            }
        }

        //public static void GetWebEstMonthlySales(int month, int Year, List<WebEstMonthlySales> WebEstMonthlySalesColl_New, List<WebEstMonthlySales> WebEstMonthlySalesColl_Renewal,
        //                            List<WebEstMonthlySalesSummary> WebEstMonthlySalesSummary)
        //{
        //    List<WebEstMonthlySales> list = new List<WebEstMonthlySales>();

        //    DBAccess db = new DBAccess();

        //    List<SqlParameter> getParameters = new List<SqlParameter>();
        //    getParameters.Add(new SqlParameter("Month", month));
        //    getParameters.Add(new SqlParameter("Year", Year));

        //    DBAccessDataSetResult result = db.ExecuteWithDataSet("WebEstMonthlySales_Get", getParameters);

        //    if (result.Success)
        //    {
        //        result.DataSet.Tables[0].TableName = "WebEstMonthlySales_New";
        //        result.DataSet.Tables[1].TableName = "WebEstMonthlySales_Renewal";
        //        result.DataSet.Tables[2].TableName = "WebEstMonthlySales_New_Summary";

        //        // WebEstMonthlySales_New
        //        foreach (DataRow eachDataRow in result.DataSet.Tables["WebEstMonthlySales_New"].Rows)
        //        {
        //            WebEstMonthlySalesColl_New.Add(new WebEstMonthlySales(eachDataRow));
        //        }

        //        // WebEstMonthlySales_Renewal
        //        foreach (DataRow eachDataRow in result.DataSet.Tables["WebEstMonthlySales_Renewal"].Rows)
        //        {
        //            WebEstMonthlySalesColl_Renewal.Add(new WebEstMonthlySales(eachDataRow));
        //        }

        //        // WebEstMonthlySales_New_Summary
        //        foreach (DataRow eachDataRow in result.DataSet.Tables["WebEstMonthlySales_New_Summary"].Rows)
        //        {
        //            WebEstMonthlySalesSummary.Add(new WebEstMonthlySalesSummary(eachDataRow));
        //        }
        //    }
        //}

        public static void UpdateWebEstMonthlySales(string webEstMonthlySalesXML)
        {
            //List<WebEstMonthlySales> list = new List<WebEstMonthlySales>();

            //DBAccess db = new DBAccess();

            //List<SqlParameter> getParameters = new List<SqlParameter>();

            //SqlParameter sqlParameter = new SqlParameter();
            //sqlParameter.ParameterName = "WebEstMonthlySalesXML";
            //sqlParameter.Value = webEstMonthlySalesXML;
            //sqlParameter.SqlDbType = SqlDbType.Xml;

            //getParameters.Add(sqlParameter);

            //db.ExecuteNonQuery("WebEstMonthlySales_Update", getParameters);

        }

    }
}
