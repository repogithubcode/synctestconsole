using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class ContractPageVM
    {

        public bool GoodData { get; set; }
        public int LoginID { get; set; }
        public string Organizationname { get; set; }
        public string CustomerName { get; set; }
        public bool AutoPay { get; set; }
        public string SalesRep { get; set; }

        public List<ContractVM> Contracts { get; set; }

        public string NewContractStartDate { get; set; }

        public int SelectedContractPriceLevel { get; set; }
        public int SelectedAddOnPriceLevel { get; set; }
        public SelectList ContractPriceLevels { get; set; }

        public int SelectedAddOnType { get; set; }
        public int SelectedAddOnTrialType { get; set; }
        public SelectList AddOnTypes { get; set; }

        public int SelectedAddOnQty { get; set; }
        public SelectList AddOnQtys { get; set; }

        public int ContractID { get; set; }
        public int InvoiceID { get; set; }
    }    
}