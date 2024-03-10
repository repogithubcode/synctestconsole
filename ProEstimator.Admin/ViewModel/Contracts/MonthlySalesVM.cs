using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel.Contracts;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class MonthlySalesVM
    {
        public string WebEstMonthlySalesID { get; set; }

        public string CustNo { get; set; }
        public string PriceLevel { get; set; }
        public string CompanyName { get; set; }

        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        public string Zip { get; set; }
        public string Phone1 { get; set; }
        public Boolean walkthough { get; set; }

        public string Contract { get; set; }
        public string Comm { get; set; }
        public string frame { get; set; }

        public string DateVal { get; set; }
        public string Total { get; set; }
        public string Down { get; set; }
        public string PaymentAmount { get; set; }

        public string PromoAmount { get; set; }

        public MonthlySalesVM()
        {
            

        }

        public MonthlySalesVM(WebEstMonthlySales eachWebEstMonthlySales)
        {
            this.WebEstMonthlySalesID = Convert.ToString(eachWebEstMonthlySales.WebEstMonthlySalesID);

            this.CustNo = Convert.ToString(eachWebEstMonthlySales.CustNo);
            this.PriceLevel = Convert.ToString(eachWebEstMonthlySales.PriceLevel);
            this.CompanyName = Convert.ToString(eachWebEstMonthlySales.CompanyName);

            this.Address1 = Convert.ToString(eachWebEstMonthlySales.Address1);
            this.City = Convert.ToString(eachWebEstMonthlySales.City);
            this.State = Convert.ToString(eachWebEstMonthlySales.State);

            this.Zip = Convert.ToString(eachWebEstMonthlySales.Zip);
            this.Phone1 = Convert.ToString(eachWebEstMonthlySales.Phone1);
            this.walkthough = eachWebEstMonthlySales.walkthough;

            this.Contract = Convert.ToString(eachWebEstMonthlySales.Contract);
            this.Comm = Convert.ToString(eachWebEstMonthlySales.Comm);
            this.frame = Convert.ToString(eachWebEstMonthlySales.frame);

            this.DateVal = Convert.ToString(eachWebEstMonthlySales.DateVal);
            this.Total = Convert.ToString(eachWebEstMonthlySales.Total);
            this.Down = Convert.ToString(eachWebEstMonthlySales.DepositAmount);

            this.PaymentAmount = Convert.ToString(eachWebEstMonthlySales.PaymentAmount);
            this.PromoAmount = Convert.ToString(eachWebEstMonthlySales.PromoAmount);
        }
    }    
}