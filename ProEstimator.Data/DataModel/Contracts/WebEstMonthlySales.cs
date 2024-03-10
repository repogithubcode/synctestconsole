using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Contracts
{
    public class WebEstMonthlySales
    {
        // Database fields
        public int WebEstMonthlySalesID { get; set; }
        public int CustNo { get; set; }
        public int PriceLevel { get; set; }

        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone1 { get; set; }

        public Boolean walkthough { get; set; }

        public int Contract { get; set; }
        public int Comm { get; set; }
        public int frame { get; set; }

        public DateTime? DateVal { get; set; }

        public decimal Total { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal PromoAmount { get; set; }

        public WebEstMonthlySales()
        {

        }

        public WebEstMonthlySales(DataRow row)
        {
            WebEstMonthlySalesID = InputHelper.GetInteger(row["WebEstMonthlySalesID"].ToString());
            CustNo = InputHelper.GetInteger(row["CustNo"].ToString());
            PriceLevel = InputHelper.GetInteger(row["PriceLevel"].ToString());
            CompanyName = InputHelper.GetString(row["CompanyName"].ToString());
            Address1 = InputHelper.GetString(row["Address1"].ToString());
            City = InputHelper.GetString(row["City"].ToString());
            State = InputHelper.GetString(row["State"].ToString());
            Zip = InputHelper.GetString(row["Zip"].ToString());
            Phone1 = InputHelper.GetString(row["Phone1"].ToString());
            walkthough = InputHelper.GetBoolean(row["walkthough"].ToString());

            Contract = InputHelper.GetInteger(row["Contract"].ToString());
            Comm = InputHelper.GetInteger(row["Comm"].ToString());
            frame = InputHelper.GetInteger(row["frame"].ToString());

            DateVal = InputHelper.GetDateTime(row["DateVal"].ToString());

            Total = InputHelper.GetDecimal(row["Total"].ToString());
            DepositAmount = InputHelper.GetDecimal(row["DepositAmount"].ToString());
            PaymentAmount = InputHelper.GetDecimal(row["PaymentAmount"].ToString());
            PromoAmount = InputHelper.GetDecimal(row["PromoAmount"].ToString());
        }

        //public SaveResult Save()
        //{
        //    List<SqlParameter> parameters = new List<SqlParameter>();
        //    parameters.Add(new SqlParameter("InvoiceID", ID));
        //    parameters.Add(new SqlParameter("LoginID", LoginID));
        //    parameters.Add(new SqlParameter("ContractID", ContractID));
        //    parameters.Add(new SqlParameter("AddOnID", AddOnID));
        //    parameters.Add(new SqlParameter("FeeInvoiceID", FeeInvoiceID));
        //    parameters.Add(new SqlParameter("PaymentNumber", PaymentNumber));
        //    parameters.Add(new SqlParameter("InvoiceTypeID", InvoiceType.ID));
        //    parameters.Add(new SqlParameter("InvoiceAmount", InvoiceAmount));
        //    parameters.Add(new SqlParameter("SalesTax", SalesTax));
        //    parameters.Add(new SqlParameter("DueDate", DueDate));
        //    parameters.Add(new SqlParameter("Notes", InputHelper.GetString(Notes)));
        //    parameters.Add(new SqlParameter("Summary", InputHelper.GetString(Summary)));
        //    parameters.Add(new SqlParameter("Paid", Paid));
        //    parameters.Add(new SqlParameter("DatePaid", Paid ? DatePaid : null));
        //    parameters.Add(new SqlParameter("PaymentID", PaymentID));
        //    parameters.Add(new SqlParameter("IsDeleted", IsDeleted));

        //    DBAccess db = new DBAccess();
        //    DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdateInvoice", parameters);
        //    if (result.Success)
        //    {
        //        ID = result.Value;
        //    }

        //    return new SaveResult(result);
        //}
    }

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
