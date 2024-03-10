using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
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


    }
}
