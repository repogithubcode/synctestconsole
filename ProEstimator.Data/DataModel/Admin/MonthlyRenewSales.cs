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
    public class MonthlyRenewSales
    {
        public Boolean PaymentExtension { get; set; }
        public int CustomerID { get; set; }
        public int ContractID { get; set; }
        public int PreviousCycleContractID { get; set; }
        public string State { get; set; }
        public DateTime? ExpectedRenewalDate { get; set; }
        public string CustomerName { get; set; }
        public double SalesPrice { get; set; }
        public DateTime? ActualRenewalDate { get; set; }
        public double RenewalDiscount	{ get; set; }
        public double DownPayment { get; set; }
        public double MonthlyPayment { get; set; }
        public double PreviousCycleSalesPrice { get; set; }
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
        public int PayCode { get; set; }
        public double? Commission { get; set; }
        public bool Has1styrRenewal { get; set; }
        public bool HasLateRenewal { get; set; }

        public double? MUCommission { get; set; }
        public double? QBCommission { get; set; }
        public double? TotalCommission { get; set; }

        public MonthlyRenewSales(DataRow row)
        {
            PaymentExtension = InputHelper.GetBoolean(row["PaymentExtension"].ToString());
            CustomerID = InputHelper.GetInteger(row["CustomerID"].ToString());
            ContractID = InputHelper.GetInteger(row["ContractID"].ToString());
            PreviousCycleContractID = InputHelper.GetInteger(row["PreviousCycleContractID"].ToString());
            State = InputHelper.GetString(row["State"].ToString());
            ExpectedRenewalDate = InputHelper.GetDateTimeNullable(row["ExpectedRenewalDate"].ToString());
            ActualRenewalDate = InputHelper.GetDateTimeNullable(row["ActualRenewalDate"].ToString());
            CustomerName = InputHelper.GetString(row["CustomerName"].ToString());
            SalesPrice = InputHelper.GetDouble(row["SalesPrice"].ToString());
            RenewalDiscount = InputHelper.GetDouble(row["RenewalDiscount"].ToString());
            DownPayment = InputHelper.GetDouble(row["DownPayment"].ToString());
            MonthlyPayment = InputHelper.GetDouble(row["MonthlyPayment"].ToString());
            PreviousCycleSalesPrice = InputHelper.GetDouble(row["PreviousCycleSalesPrice"].ToString());
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
            PayCode = InputHelper.GetInteger(row["PayCode"].ToString());
            Commission = InputHelper.GetDouble(row["Commission"].ToString());
            Has1styrRenewal = InputHelper.GetBoolean(row["Has1styrRenewal"].ToString());
            HasLateRenewal = InputHelper.GetBoolean(row["HasLateRenewal"].ToString());

            MUCommission = InputHelper.GetDouble(row["MUCommission"].ToString());
            QBCommission = InputHelper.GetDouble(row["QBCommission"].ToString());
            TotalCommission = InputHelper.GetDouble(row["TotalCommission"].ToString());
        }

        public static List<MonthlyRenewSales> GetForFilter(int month, int year)
        {
            DataTable table = GetForFiltertData(month, year);
            List<MonthlyRenewSales> monthlyRenewSalesColl = new List<MonthlyRenewSales>();

            foreach (DataRow row in table.Rows)
            {
                monthlyRenewSalesColl.Add(new MonthlyRenewSales(row));
            }

            return monthlyRenewSalesColl;
        }

        public static DataTable GetForFiltertData(int month, int year)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Month", month));
            parameters.Add(new SqlParameter("Year", year));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("MonthlyRenewSalesDashboardReport", parameters);
            return tableResult.DataTable;
        }
    }
}
