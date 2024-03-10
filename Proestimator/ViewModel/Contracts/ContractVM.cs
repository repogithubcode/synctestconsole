using System;
using System.Collections.Generic;
using System.Linq;

using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.ViewModel.Contracts
{
    public class ContractVM
    {
        public int ContractID { get; set; }
        public string TermDescription { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int DigitalSignatureID { get; set; }
        public bool IsActive { get; set; }
        public List<AddOnDetailsVM> AddOnDetails { get; set; } = new List<AddOnDetailsVM>();
    }
}