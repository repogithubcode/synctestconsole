using ProEstimatorData.Models.SubModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.Profiles
{
    public class RateVM : RateProfileVMBase 
    {

        public List<RateLabor> LaborRates { get; set; }
        public List<RateLabor> SuppliesRates { get; set; }
        public List<RatePart> PartsRates { get; set; }
        public List<RatePart> OtherRates { get; set; }

        public string ErrorMessage { get; set; }

        public bool UseDefaultProfile { get; set; }

        public bool CapRatesAfterInclude { get; set; }
        public bool CapSuppliesAfterInclude { get; set; }

        public bool IsFullEstimateDiscountMarkup { get; set; }
        public double FullEstimateDiscountMarkupValue { get; set; }

        public bool RateCapSelectionCancelled { get; set; }


        public RateVM()
        {
            LaborRates = new List<RateLabor>();
            SuppliesRates = new List<RateLabor>();
            PartsRates = new List<RatePart>();
            OtherRates = new List<RatePart>();
        }
    }
}