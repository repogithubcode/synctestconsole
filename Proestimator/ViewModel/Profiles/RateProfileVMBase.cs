using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.Profiles
{
    public class RateProfileVMBase
    {
        public int UserID { get; set; }
        public int LoginID { get; set; }
        public bool ProfileIsValid { get; set; }
        public int ProfileID { get; set; }
        public string ProfileName { get; set; }
        public string Description { get; set; }
        public bool ProfileDefault { get; set; }
        public bool PresetsDefault { get; set; }
        public int EstimateID { get; set; }
        public bool EstimateIsLocked { get; set; }
        public bool IsGlobalProfile { get; set; }
        public decimal CreditCardFeePercentage { get; set; }
        public bool ApplyCreditCardFee { get; set; }
        public bool TaxedCreditCardFee { get; set; }
    }
}