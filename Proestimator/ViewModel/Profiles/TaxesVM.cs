using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.Profiles
{
    public class TaxesVM : RateProfileVMBase 
    {
        public int ID { get; set; }
        public string TaxRate { get; set; }
        public string SecondTaxRateStart { get; set; }
        public string SecondTaxRate { get; set; }
        
        public bool TaxLaborAndPartsSeparately { get; set; }
        public string PartsTaxRate { get; set; }

        public decimal ACChargeAmount { get; set; }
        public string LKQText { get; set; }
        public string TotalLossPerc { get; set; }
        public bool IncludeStructureInBody { get; set; }
        public bool ChargeForAimingHeadlights { get; set; }
        public bool ChargeForPowerUnits { get; set; }
        public bool ChargeForRefrigRecovery { get; set; }
        public bool SuppressAddRelatedPrompt { get; set; }
        public int SuppLevel { get; set; }
        public bool DoNotMarkChanges { get; set; }

        public bool UseDefaultProfile { get; set; }
    }
}