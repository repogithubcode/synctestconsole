using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    public class WebEstSales
    {
        public List<WebEstMonthlySales> WebEstMonthlySalesNew { get; set; }
        public List<WebEstMonthlySales> WebEstMonthlySalesRenew { get; set; }
        public List<WebEstMonthlySalesSummary> WebEstMonthlySalesSummary { get; set; }

        public WebEstSales()
        {


        }

        public WebEstSales GetForFilter(int month, int Year)
        {
            WebEstSales webEstSales = new WebEstSales();
            webEstSales.WebEstMonthlySalesNew = new List<WebEstMonthlySales>();
            webEstSales.WebEstMonthlySalesRenew = new List<WebEstMonthlySales>();
            webEstSales.WebEstMonthlySalesSummary = new List<WebEstMonthlySalesSummary>();

            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Month", Common.GetParameterValue(month)));
            parameters.Add(new SqlParameter("Year", Common.GetParameterValue(Year)));

            DBAccessDataSetResult datasetResult = db.ExecuteWithDataSet("WebEstMonthlySales_Get", parameters);

            if (datasetResult.Success)
            {
                datasetResult.DataSet.Tables[0].TableName = "WebEstMonthlySales_New";
                datasetResult.DataSet.Tables[1].TableName = "WebEstMonthlySales_Renewal";
                datasetResult.DataSet.Tables[2].TableName = "WebEstMonthlySales_New_Summary";

                // WebEstMonthlySales_New
                foreach (DataRow eachDataRow in datasetResult.DataSet.Tables["WebEstMonthlySales_New"].Rows)
                {
                    webEstSales.WebEstMonthlySalesNew.Add(new WebEstMonthlySales(eachDataRow));
                }

                // WebEstMonthlySales_Renewal
                foreach (DataRow eachDataRow in datasetResult.DataSet.Tables["WebEstMonthlySales_Renewal"].Rows)
                {
                    webEstSales.WebEstMonthlySalesRenew.Add(new WebEstMonthlySales(eachDataRow));
                }

                // WebEstMonthlySales_New_Summary
                foreach (DataRow eachDataRow in datasetResult.DataSet.Tables["WebEstMonthlySales_New_Summary"].Rows)
                {
                    webEstSales.WebEstMonthlySalesSummary.Add(new WebEstMonthlySalesSummary(eachDataRow));
                }
            }

            return webEstSales;
        }
    }
}
