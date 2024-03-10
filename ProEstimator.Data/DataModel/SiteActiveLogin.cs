using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData.DataModel
{
    public class SiteActiveLogin : ActiveLogin
    {
        // This is set to True when loaded from the database.  If true, the LoginManager must process contracts before returning.
        public bool NeedsInvoiceCheck { get; set; }

        // These are set when the login is first authorized
        public bool InvoiceNeedsPayment { get; set; }
        public bool HasContract { get; set; }
        public bool IsTrial { get; set; }
        public bool HasFrameDataContract { get; set; }
        public bool HasEMSContract { get; set; }
        public bool HasPDRContract { get; set; }
        public bool HasMultiUserContract { get; set; }
        public bool HasQBExportContract { get; set; }
        public bool HasProAdvisorContract { get; set; }
        public bool HasImagesContract { get; set; }
        public bool HasBundleContract { get; set; }
        public bool ProAdvisorIsTrial { get; set; }
        public bool ImageEditorIsTrial { get; set; }
        public bool HasCustomReportsContract { get; set; }
        public bool HasPartsNow { get; set; }
        public bool ShowEarlyRenewal { get; set; }
        public bool HasUnsignedContract { get; set; }

        /// <summary>
        /// We need to do a new contract check every so often after a user logs in in case their contract expires or is changed by admin.
        /// When this value gets too old, recheck the contract. 
        /// </summary>
        public DateTime NextContractCheck { get; set; }

        public string ContractReminder { get; set; }

        public string LanguagePreference { get; set; }

        public Boolean? ShowChatIconDesktop { get; set; }
        public Boolean? ShowChatIconMobile { get; set; }

        /// <summary>
        /// For ProfitWell, we need the user's email address, which we don't want to load on every page.
        /// </summary>
        public string UserEmailAddress { get; private set; }

        public void RefreshEmailAddress()
        {
            if (string.IsNullOrEmpty(UserEmailAddress))
            {
                SiteUser siteUser = SiteUser.Get(SiteUserID);
                if (siteUser != null)
                {
                    UserEmailAddress = siteUser.EmailAddress;
                }
            }
        }

        public SiteActiveLogin()
        {
            NextContractCheck = DateTime.MinValue;
            NeedsInvoiceCheck = true;
        }

        public SiteActiveLogin(System.Data.DataRow row)
            : base(row)
        {
            NeedsInvoiceCheck = true;
        }
    }
}
