using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    public class WebEstMonthlySalesSummary
    {
        // Database fields
        public int CustCount { get; set; }
        public int RegContracts { get; set; }
        public int CommPkg { get; set; }
        public int Frame { get; set; }
        public int MTHCustCount { get; set; }
        public int YTDCustCount { get; set; }

        public decimal MTHTotalPrice { get; set; }
        public decimal YTDTotalPrice { get; set; }
        public decimal AVG1YTDTotalPrice { get; set; }
        public decimal AVG2YTDTotalPrice { get; set; }

        public WebEstMonthlySalesSummary()
        {

        }

        public WebEstMonthlySalesSummary(DataRow row)
        {
            CustCount = InputHelper.GetInteger(row["CustCount"].ToString());
            RegContracts = InputHelper.GetInteger(row["RegContracts"].ToString());
            CommPkg = InputHelper.GetInteger(row["CommPkg"].ToString());

            Frame = InputHelper.GetInteger(row["Frame"].ToString());
            MTHCustCount = InputHelper.GetInteger(row["MTH(CustCount)"].ToString());
            YTDCustCount = InputHelper.GetInteger(row["YTD(CustCount)"].ToString());

            MTHTotalPrice = InputHelper.GetDecimal(row["MTH(Total Price)"].ToString());
            YTDTotalPrice = InputHelper.GetDecimal(row["YTD(Total Price)"].ToString());
            AVG1YTDTotalPrice = InputHelper.GetDecimal(row["AVG1 YTD(Total Price)"].ToString());
            AVG2YTDTotalPrice = InputHelper.GetDecimal(row["AVG2 YTD(Total Price)"].ToString());
        }
    }
}
