using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class AddOnVM
    {
        public int ID { get; set; }
        public int ContractPriceLevelID { get; set; }
        public string ContractType { get; set; }
        public string Description { get; set; }
        public string StartDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int Quantity { get; set; }
        public bool HasPayment { get; set; }
        public string TermDescription { get; set; }

        public string ErrorMessage { get; set; }
    }
}