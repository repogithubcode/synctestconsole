using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.Contracts
{
    public class SignContractVM
    {
        public bool ShowForm { get; set; } = true;
        public int LoginID { get; set; }
        public int ContractID { get; set; }
        public string ContractText { get; set; }
        public string ErrorMessage { get; set; }
        public string InputValue { get; set; }
        public string SignatureDate { get; set; }
    }
}