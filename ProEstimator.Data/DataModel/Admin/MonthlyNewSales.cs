using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimatorData.DataModel.Admin
{
    public class MonthlyNewSales
    {
        public Boolean PaymentExtension { get; set; }
        public int CustomerID { get; set; }
        public int ContractID { get; set; }
        public string State { get; set; }
        public DateTime? SalesDate { get; set; }
        public string CustomerName { get; set; }
        public double SalesPrice { get; set; }
        public double Promo	{ get; set; }
        public double DownPayment { get; set; }
        public double MonthlyPayment { get; set; }
        public bool HasFrame { get; set; }
        public bool HasEMS { get; set; }
        public bool HasPDR { get; set; }
        public bool HasMultiUser { get; set; }
        public bool HasQBExport { get; set; }
        public bool HasProAdvisor { get; set; }
        public bool HasImageEditor { get; set; }
        public bool HasBundle { get; set; }
        public bool HasReporting { get; set; }
        public int SalesRepID { get; set; }
        public string SalesRep { get; set; }

        public string WalkThrough { get; set; }
        public double? Commission { get; set; }
        public int PayCode { get; set; }
        public Boolean PayrollSubmitted { get; set; }

        public double? MUCommission { get; set; }
        public double? QBCommission { get; set; }
        public double? TotalCommission { get; set; }

    public string ContractTermDescription { get; set; }

        public MonthlyNewSales(DataRow row)
        {
            PaymentExtension = InputHelper.GetBoolean(row["PaymentExtension"].ToString());
            CustomerID = InputHelper.GetInteger(row["CustomerID"].ToString());
            ContractID = InputHelper.GetInteger(row["ContractID"].ToString());
            State = InputHelper.GetString(row["State"].ToString());
            SalesDate = InputHelper.GetDateTimeNullable(row["SalesDate"].ToString());
            CustomerName = InputHelper.GetString(row["CustomerName"].ToString());
            SalesPrice = InputHelper.GetDouble(row["SalesPrice"].ToString());
            Promo = InputHelper.GetDouble(row["Promo"].ToString());
            DownPayment = InputHelper.GetDouble(row["DownPayment"].ToString());
            MonthlyPayment = InputHelper.GetDouble(row["MonthlyPayment"].ToString());

            HasFrame = InputHelper.GetBoolean(row["HasFrame"].ToString());
            HasEMS = InputHelper.GetBoolean(row["HasEMS"].ToString());
            HasPDR = InputHelper.GetBoolean(row["HasPDR"].ToString());
            HasMultiUser = InputHelper.GetBoolean(row["HasMultiUser"].ToString());
            HasQBExport = InputHelper.GetBoolean(row["HasQBExport"].ToString());
            HasProAdvisor = InputHelper.GetBoolean(row["HasProAdvisor"].ToString());
            HasImageEditor = InputHelper.GetBoolean(row["HasImageEditor"].ToString());
            HasBundle = InputHelper.GetBoolean(row["HasBundle"].ToString());
            HasReporting = InputHelper.GetBoolean(row["HasReporting"].ToString());

            SalesRepID = InputHelper.GetInteger(row["SalesRepID"].ToString());
            SalesRep = InputHelper.GetString(row["SalesRep"].ToString());

            WalkThrough = InputHelper.GetString(row["WalkThrough"].ToString());
            PayCode = InputHelper.GetInteger(row["PayCode"].ToString());
            Commission = InputHelper.GetDouble(row["Commission"].ToString());

            MUCommission = InputHelper.GetDouble(row["MUCommission"].ToString());
            QBCommission = InputHelper.GetDouble(row["QBCommission"].ToString());
            TotalCommission = InputHelper.GetDouble(row["TotalCommission"].ToString());

            ContractTermDescription = InputHelper.GetString(row["ContractTermDescription"].ToString());
        }

        public static List<MonthlyNewSales> GetForFilter(int month, int year)
        {
            DataTable table = GetForFiltertData(month, year);
            List<MonthlyNewSales> monthlyNewSalesColl = new List<MonthlyNewSales>();

            foreach (DataRow row in table.Rows)
            {
                monthlyNewSalesColl.Add(new MonthlyNewSales(row));
            }

            return monthlyNewSalesColl;
        }

        public static DataTable GetForFiltertData(int month, int year)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Month", month));
            parameters.Add(new SqlParameter("Year", year));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("MonthlyNewSalesDashboardReport", parameters);
            return tableResult.DataTable;
        }
    }
}
