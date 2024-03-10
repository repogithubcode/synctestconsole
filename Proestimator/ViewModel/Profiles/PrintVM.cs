using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proestimator.ViewModel.Profiles
{
    public class PrintVM : RateProfileVMBase 
    {
        public int ID { get; set; }
        public int GraphicsQuality { get; set; }
        public bool NoHeaderLogo { get; set; }
        public bool NoInsuranceSection { get; set; }
        public bool NoPhotos { get; set; }
        [AllowHtml]
        public string FooterText { get; set; }
        public bool PrintPrivateNotes { get; set; }
        public bool PrintPublicNotes { get; set; }
        public string ContactOption { get; set; }
        public string SupplementOption { get; set; }
        public string OrderBy { get; set; }
        public bool SeparateLabor { get; set; }
        public bool EstimateNumber { get; set; }
        public bool AdminInfoID { get; set; }
        public bool Dollars { get; set; }
        public bool GreyBars { get; set; }
        public bool NoVehicleAccessories { get; set; }
        public bool PrintPaymentInfo { get; set; }
        public bool PrintEstimator { get; set; }
        public bool PrintVendors { get; set; }

        public bool PrintTechnicians { get; set; }
        public bool NoFooterDateTimeStamp { get; set; }

        public bool UseDefaultProfile { get; set; }
    }
}