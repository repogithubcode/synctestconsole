using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProEstimator.Admin.ViewModel.ContractReport
{
    public class ContractGridVM
    {
        public int ID { get; set; }
        public int LoginID { get; set; }
        public string ContractTermDescription { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string SalesRep { get; set; }
        public string Notes { get; set; }
        public bool Active { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsRenewal { get; set; }
    }
}