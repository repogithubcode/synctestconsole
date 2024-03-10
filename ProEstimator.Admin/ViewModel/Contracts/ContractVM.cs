using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.Business.Logic;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class ContractVM
    {
        public int ContractID { get; set; }
        public int ContractPriceLevel { get; set; }
        public string TermDescription { get; set; }
        public string Notes { get; set; }
        public string PromoCode { get; set; }
        public List<PromoCodeVM> AvailablePromoCodes { get; set; }        

        public string CreatedDate { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpirationDate { get; set; }

        public bool HasEMS { get; set; }
        public bool HasMU { get; set; }
        public bool HasFrameData { get; set; }
        public bool HasQB { get; set; }
        public bool HasProAdvisor { get; set; }
        public bool HasImages { get; set; }
        public bool HasCustomReports { get; set; }
        public bool HasBundle { get; set; }

        public string HasEMSString { get { return HasAddOnString(HasEMS, HasEMS); } }
        public string HasMUString { get { return HasAddOnString(HasMU, HasMU); } }
        public string HasFrameDataString { get { return HasAddOnString(HasFrameData, HasFrameData); } }
        public string HasQBString { get { return HasAddOnString(HasQB, HasBundle); } }
        public string HasProAdvisorString { get { return HasAddOnString(HasProAdvisor, HasBundle); } }
        public string HasImagesString { get { return HasAddOnString(HasImages, HasBundle); } }
        public string HasCustomReportsString { get { return HasAddOnString(HasCustomReports, HasBundle); } }
        public string HasBundleString { get { return HasAddOnString(HasBundle, HasBundle); } }

        private string HasAddOnString(bool hasAddOn, bool hasPermission)
        {
            string result = "";

            if (hasAddOn)
            {
                result = "A";
            }

            if (hasPermission || hasAddOn)
            {
                if (hasAddOn)
                {
                    result += " | ";
                }

                result += "P";
            }

            return result;
        }

        public bool HasPayment { get; set; }

        public bool ShowPickAddonButton { get; set; }

        public bool IsSigned { get; set; }
        public bool IgnoreAutoPay { get; set; }
        public bool EarlyRenewal { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        /// <summary>
        /// This is not usually loaded, it must be set manually by the controller.
        /// </summary>
        public int DigitalSignatureID { get; set; }

        public string ErrorMessage { get; set; }
    }
}