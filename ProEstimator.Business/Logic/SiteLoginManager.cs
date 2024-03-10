using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Logic
{
    public class SiteLoginManager : LoginManager<SiteActiveLogin>
    {
        private int _contractCheckBufferMinutes = 15;
        
        public SiteLoginManager()
            : base(false)
        {
            
        }

        protected override void ActiveLoginsLoaded()
        {
            try
            {
                _contractCheckBufferMinutes = InputHelper.GetInteger(System.Configuration.ConfigurationManager.AppSettings.Get("ContractCheckMinutes"), 15);
            }
            catch { }

            if (_contractCheckBufferMinutes < 1)
            {
                _contractCheckBufferMinutes = 15;
            }
        }

        protected override SiteActiveLogin InstantiateNewActiveLogin()
        {
            return new SiteActiveLogin();
        }

        protected override SiteActiveLogin LoadActiveLogin(System.Data.DataRow row)
        {
            return new SiteActiveLogin(row);
        }

        protected override void ActiveLoginRetrieved(ActiveLogin activeLogin)
        {
            SiteActiveLogin siteActiveLogin = activeLogin as SiteActiveLogin;

            if (siteActiveLogin.NeedsInvoiceCheck)
            {
                CheckInvoiceInformation(siteActiveLogin);
            }

            siteActiveLogin.RefreshEmailAddress();
        }

        public void RefreshInvoiceInformation(int siteUserID, string computerKey)
        {
            SiteActiveLogin activeLogin = GetActiveLogin(siteUserID, computerKey);
            if (activeLogin != null)
            {
                CheckInvoiceInformation(activeLogin);
            }
        }

        public void RefreshInvoiceInformationForAccount(int loginID)
        {
            List<SiteActiveLogin> logins = ActiveLogins.Where(o => o.LoginID == loginID && o.DeleteKey == ActiveLoginDeleteKey.Active).ToList();

            foreach (SiteActiveLogin login in logins)
            {
                CheckInvoiceInformation(login);
            }
        }

        private void CheckInvoiceInformation(SiteActiveLogin activeLogin)
        {
            const int paymentGracePeriod = -3;

            activeLogin.InvoiceNeedsPayment = false;
            activeLogin.HasContract = false;
            activeLogin.IsTrial = false;
            activeLogin.HasFrameDataContract = false;
            activeLogin.HasEMSContract = false;
            activeLogin.HasPDRContract = true;
            activeLogin.HasPartsNow = false;
            activeLogin.HasMultiUserContract = false;
            activeLogin.HasQBExportContract = false;
            activeLogin.ShowEarlyRenewal = false;
            activeLogin.HasUnsignedContract = false;

            activeLogin.ContractReminder = "";

            activeLogin.HasPartsNow = new PartsNowService().GetPartsNowByLogin(activeLogin.LoginID);

            List<Invoice> invoices = ContractManager.GetInvoicesToBePaid(activeLogin.LoginID);

            // Check for an active contract.
            Contract activeContract = Contract.GetActive(activeLogin.LoginID);
            if (activeContract != null)
            {
                activeLogin.HasContract = true;
                activeLogin.IsTrial = false;
                activeLogin.HasUnsignedContract = !activeContract.IsSigned;

                // Check add on contracts
                List<ContractAddOn> addOns = ContractAddOn.GetForContract(activeContract.ID);
                List<ContractAddOnTrial> addOnTrials = ContractAddOnTrial.GetForContract(activeContract.ID).Where(o => o.StartDate.Date <= DateTime.Now.Date && o.EndDate.Date >= DateTime.Now.Date).ToList();

                ContractAddOn emsAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 5 && o.Active);
                ContractAddOn frameAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 2 && o.Active);
                ContractAddOn qbExportAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 9 && o.Active);
                ContractAddOn proAdvisorAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 10 && o.Active);
                ContractAddOn imagesAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 11 && o.Active);
                ContractAddOn customReportAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 13 && o.Active);
                ContractAddOn bundleAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 12 && o.Active);

                // See if the customer has a bundle add on
                bool hasBundle = false;

                if (bundleAddOn != null)
                {
                    Invoice dueAddOnInvoice = invoices.FirstOrDefault(o => o.AddOnID == bundleAddOn.ID && o.DaysUntilDue < paymentGracePeriod);
                    if (dueAddOnInvoice == null)
                    {
                        hasBundle = true;
                    }
                }

                if (!activeLogin.HasBundleContract && addOnTrials.FirstOrDefault(o => o.AddOnType.ID == 12) != null)
                {
                    hasBundle = true;
                }

                // EMS Add On
                if (emsAddOn != null)
                {
                    Invoice dueEmsInvoice = invoices.FirstOrDefault(o => o.AddOnID == emsAddOn.ID && o.DaysUntilDue < paymentGracePeriod);
                    if (dueEmsInvoice == null)
                    {
                        activeLogin.HasEMSContract = true;
                    }
                }

                if (!activeLogin.HasEMSContract && addOnTrials.FirstOrDefault(o => o.AddOnType.ID == 5) != null)
                {
                    activeLogin.HasEMSContract = true;
                }

                // Frame Add On
                if (frameAddOn != null)
                {
                    Invoice dueFrameInvoice = invoices.FirstOrDefault(o => o.AddOnID == frameAddOn.ID && o.DaysUntilDue < paymentGracePeriod);
                    if (dueFrameInvoice == null)
                    {
                        activeLogin.HasFrameDataContract = true;
                    }
                }

                if (!activeLogin.HasFrameDataContract && addOnTrials.FirstOrDefault(o => o.AddOnType.ID == 2) != null)
                {
                    activeLogin.HasFrameDataContract = true;
                }

                // Multi User Add On
                activeLogin.HasMultiUserContract = addOns.FirstOrDefault(o => o.AddOnType.ID == 8 && o.Active && o.HasPayment) != null;
                if (!activeLogin.HasMultiUserContract && addOnTrials.FirstOrDefault(o => o.AddOnType.ID == 8) != null)
                {
                    activeLogin.HasMultiUserContract = true;
                }

                // QB Add On
                if (qbExportAddOn != null)
                {
                    Invoice dueQBInvoice = invoices.FirstOrDefault(o => o.AddOnID == qbExportAddOn.ID && o.DaysUntilDue < paymentGracePeriod);
                    if (dueQBInvoice == null)
                    {
                        activeLogin.HasQBExportContract = true;
                    }
                }

                if (!activeLogin.HasQBExportContract && addOnTrials.FirstOrDefault(o => o.AddOnType.ID == 9) != null)
                {
                    activeLogin.HasQBExportContract = true;
                }

                // ProAdvisor add on
                if (proAdvisorAddOn != null)
                {
                    Invoice dueAddOnInvoice = invoices.FirstOrDefault(o => o.AddOnID == proAdvisorAddOn.ID && o.DaysUntilDue < paymentGracePeriod);
                    if (dueAddOnInvoice == null)
                    {
                        activeLogin.HasProAdvisorContract = true;
                    }
                }

                if (!activeLogin.HasProAdvisorContract && addOnTrials.FirstOrDefault(o => o.AddOnType.ID == 10) != null)
                {
                    activeLogin.HasProAdvisorContract = true;
                    activeLogin.ProAdvisorIsTrial = true;
                }

                // Images Add On
                if (imagesAddOn != null)
                {
                    Invoice dueImageInvoice = invoices.FirstOrDefault(o => o.AddOnID == imagesAddOn.ID && o.DaysUntilDue < paymentGracePeriod);
                    if (dueImageInvoice == null)
                    {
                        activeLogin.HasImagesContract = true;
                    }
                }

                if (!activeLogin.HasImagesContract && addOnTrials.FirstOrDefault(o => o.AddOnType.ID == 11) != null)
                {
                    activeLogin.HasImagesContract = true;
                    activeLogin.ImageEditorIsTrial = true;
                } 

                if (hasBundle)
                {
                    activeLogin.HasBundleContract = true;
                    activeLogin.HasQBExportContract = true;
                    activeLogin.HasProAdvisorContract = true;
                    activeLogin.HasImagesContract = true;
                    activeLogin.HasCustomReportsContract = true;
                }

                // Custom Reports Add On
                if (customReportAddOn != null)
                {
                    Invoice dueCustomReportsInvoice = invoices.FirstOrDefault(o => o.AddOnID == customReportAddOn.ID && o.DaysUntilDue < paymentGracePeriod);
                    if (dueCustomReportsInvoice == null)
                    {
                        activeLogin.HasCustomReportsContract = true;
                    }
                }

                if (!activeLogin.HasCustomReportsContract && addOnTrials.FirstOrDefault(o => o.AddOnType.ID == 13) != null)
                {
                    activeLogin.HasCustomReportsContract = true;
                }

                LoginAutoRenew autoRenew = LoginAutoRenew.GetLastForLogin(activeLogin.LoginID);
                if (autoRenew == null || !autoRenew.Enabled)
                {
                    activeLogin.ShowEarlyRenewal = ContractManager.IsInEarlyRenewalPeriod(activeContract);
                }

                // If the contract is about to expire and there isn't another one show a message
                if (activeContract.DaysUntilExpiration <= ContractManager.RenewalWindow)
                {
                    List<Contract> allContracts = Contract.GetAllForLogin(activeLogin.LoginID);
                    StripeInfo stripeInfo = StripeInfo.GetForLogin(activeLogin.LoginID);

                    bool hasAutoPay = stripeInfo != null && stripeInfo.AutoPay;

                    Contract renewalContract = allContracts.FirstOrDefault(o => o.EffectiveDate.Date >= activeContract.ExpirationDate.Date && (o.HasPayment || hasAutoPay));
                    if (renewalContract == null)
                    {
                        activeLogin.ContractReminder = "Your contract expires on " + activeContract.ExpirationDate.ToShortDateString();
                    }
                }
            }
            else
            {
                // Check if the user has a trial contract
                Trial trial = Trial.GetActive(activeLogin.LoginID);
                if (trial != null)
                {
                    activeLogin.HasContract = true;
                    activeLogin.IsTrial = true;
                    activeLogin.HasPDRContract = trial.HasPDR;
                    activeLogin.HasEMSContract = trial.HasEMS;
                    activeLogin.HasFrameDataContract = trial.HasFrameData;
                    activeLogin.HasQBExportContract = trial.HasQBExport;
                    activeLogin.HasProAdvisorContract = trial.HasProAdvisor;
                    activeLogin.ProAdvisorIsTrial = trial.HasProAdvisor;
                    activeLogin.HasImagesContract = trial.HasImages;

                    activeLogin.ContractReminder = "Your trial Web-Est subscription will expire on " + trial.EndDate.ToShortDateString() + ".";
                }
            }

            //Checking for OverDue main contract Invoices
            try
            {
                int daysUntilDue = 100;
                decimal total = 0;
                foreach (Invoice invoice in invoices)
                {
                    if (invoice.AddOnID == 0)   // Only check main contract invoices, not add ons
                    {
                        total += invoice.InvoiceTotal;

                        if (invoice.DaysUntilDue < daysUntilDue)
                        {
                            daysUntilDue = invoice.DaysUntilDue;
                        }
                    }
                }

                if (total > 0 && daysUntilDue < paymentGracePeriod)
                {
                    if (activeLogin.IsImpersonated != true)
                    {
                        activeLogin.InvoiceNeedsPayment = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, activeLogin.LoginID, "SiteSession CheckInvoiceInformation CheckInvoices");
            }

            activeLogin.NeedsInvoiceCheck = false;
            activeLogin.NextContractCheck = DateTime.Now.AddMinutes(_contractCheckBufferMinutes);
        }
    }
}
