using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimator.Business.Model.Admin;

using ProEstimator.Admin.ViewModel;
using ProEstimator.Admin.ViewModel.Logins;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.Business.Logic;
using ProEstimator.Admin.ViewModel.Contracts;
using ProEstimator.Admin.ViewModel.EmailReport;
using ProEstimatorData.DataModel.Admin;
using ProEstimator.Business.Model.Account;
using ProEstimator.DataRepositories.Contracts;
using ProEstimator.DataRepositories.ProAdvisor;
using ProEstimatorData.DataModel.ProAdvisor;
using ProEstimator.Business.ProAdvisor;

namespace ProEstimator.Admin.Controllers
{
    public class LoginsController : AdminController
    {
        private IInvoiceFailureSummaryRepository _failureSummaryService;
        private IProAdvisorProfileService _proAdvisorProfileService;
        private IProAdvisorProfileRepository _proAdvisorProfileRepository;

        public LoginsController(IInvoiceFailureSummaryRepository failureSummaryService, IProAdvisorProfileService proAdvisorProfileService, IProAdvisorProfileRepository proAdvisorProfileRepository)
        {
            _failureSummaryService = failureSummaryService;
            _proAdvisorProfileService = proAdvisorProfileService;
            _proAdvisorProfileRepository = proAdvisorProfileRepository;
        }

        [HttpGet]
        [Route("Logins/New")]
        public ActionResult NewLogin()
        {
            Contact contact = new Contact();
            contact.ContactType = ContactType.Person;
            contact.ContactSubType = ContactSubType.FocuswriteOrganization;
            contact.Save(ActiveLogin.ID, ActiveLogin.LoginID);

            Address address = new Address();
            address.ContactID = contact.ContactID;
            address.Save(ActiveLogin.ID, ActiveLogin.LoginID);

            LoginInfo newLogin = new LoginInfo();
            newLogin.ContactID = contact.ContactID;
            SaveResult saveResult = newLogin.Save(ActiveLogin.ID);

            if (saveResult.Success)
            {
                ProEstimatorData.Models.RateProfileManager.CreateBlankRateProfile(newLogin.ID);
                ContractManager.ExtendOrCreateTrial(newLogin.ID, 14, GetSalesRepID());

                return Redirect("/Logins/List/" + newLogin.ID);
            }

            return View(saveResult.ErrorMessage);
        }

        [HttpGet]
        [Route("Logins/List/{loginID}")]
        public ActionResult Index(int loginID)
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/Logins/List/" + loginID;
                return Redirect("/LogOut");
            }
            else
            {
                LoginsPageVM vm = new LoginsPageVM();

                if (loginID == -1)
                {
                    vm.CreateNewLogin = true;
                    vm.GoodData = true;
                    vm.LoginID = 0;
                }
                else if (loginID == 0)
                {
                    vm.GoodData = true;
                    vm.LoginID = 0;
                }
                else
                {
                    // Load organization info
                    LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                    if (loginInfo != null)
                    {
                        vm.GoodData = true;
                        vm.LoginID = loginID;
                        vm.SessionSalesRepID = GetSalesRepID();

                        vm.CreationDate = loginInfo.CreationDate;
                        vm.LoginName = loginInfo.LoginName;
                        vm.Organization = loginInfo.Organization;
                        vm.Password = loginInfo.Password;
                        vm.TechSupportPassword = loginInfo.TechSupportPassword;

                        vm.Disabled = loginInfo.Disabled;
                        vm.DoubtfulAccount = loginInfo.DoubtfulAccount;
                        vm.StaffAccount = loginInfo.StaffAccount;
                        vm.Appraiser = loginInfo.Appraiser;

                        vm.CompanyOrigin = loginInfo.CompanyOrigin;

                        vm.CompanyType = loginInfo.CompanyType;

                        if (loginInfo.CarfaxExcludeDate != null && loginInfo.CarfaxExcludeDate != DateTime.MinValue)
                        {
                            vm.CarfaxExcludeDateStr = Convert.ToDateTime(loginInfo.CarfaxExcludeDate).ToString("MM/dd/yyyy hh:mm tt");
                            vm.CarfaxExclude = true;
                        }
                        else
                        {
                            vm.CarfaxExclude = false;
                        }

                        vm.LanguageID = loginInfo.LanguageID;

                        vm.IntelliPayMerchantKey = loginInfo.IntelliPayMerchantKey;
                        vm.IntelliPayAPIKey = loginInfo.IntelliPayAPIKey;
                        vm.IntelliPayUseCardReader = loginInfo.IntelliPayUseCardReader;

                        Contact contact = Contact.GetContact(loginInfo.ContactID);
                        vm.FirstName = contact.FirstName;
                        vm.LastName = contact.LastName;
                        vm.HeaderContact = loginInfo.HeaderContact;
                        vm.JobTitle = contact.Title;
                        vm.EmailAddress = contact.Email;
                        vm.PhoneType1 = string.IsNullOrEmpty(contact.PhoneNumberType1) ? "WP" : contact.PhoneNumberType1;
                        vm.PhoneNumber1 = contact.Phone;
                        vm.PhoneType2 = string.IsNullOrEmpty(contact.PhoneNumberType2) ? "CP" : contact.PhoneNumberType2;
                        vm.PhoneNumber2 = contact.Phone2;
                        vm.FaxNumber = contact.Fax;

                        Address address = Address.GetForContact(loginInfo.ContactID);
                        vm.CompanyName = loginInfo.CompanyName;
                        vm.Address1 = address.Line1;
                        vm.Address2 = address.Line2;
                        vm.City = address.City;
                        vm.State = address.State;
                        vm.Zip = address.Zip;

                        LoginAutoRenew autoRenew = LoginAutoRenew.GetLastForLogin(loginID);
                        if (autoRenew != null)
                        {
                            vm.IsAutoRenew = autoRenew.Enabled;
                        }

                        JsonResult jsonResult = CheckIfRemoteSupportEnabled(loginID);
                        vm.IsRemoteSupportOn = Convert.ToString(jsonResult.Data) == "true" ? true : false;

                        ActiveLogin lastLogin = ActiveLogin.GetLastLoginActivity(loginID);
                        if (lastLogin != null)
                        {
                            vm.LastActivityTime = lastLogin.LastActivity.ToString();
                        }
                        else
                        {
                            vm.LastActivityTime = "N/A";
                        }

                        vm.SalesRepID = loginInfo.SalesRepID;

                        vm.RegistrationNumber = loginInfo.RegistrationNumber;
                        vm.ShowRepairShopProfiles = loginInfo.ShowRepairShopProfiles;
                        vm.AllowAlternateIdentities = loginInfo.AllowAlternateIdentities;
                        vm.ShowLaborTimeWO = loginInfo.ShowLaborTimeWO;
                        vm.LicenseNumber = loginInfo.LicenseNumber;
                        vm.BarNumber = loginInfo.BarNumber;
                        vm.FederalTaxID = loginInfo.FederalTaxID;
                        vm.UseDefaultRateProfile = loginInfo.UseDefaultRateProfile;
                        vm.UseDefaultPDRRateProfile = loginInfo.UseDefaultPDRRateProfile;
                        vm.ProfileLocked = loginInfo.ProfileLocked;
                        vm.PartsNow = loginInfo.PartsNow;
                        vm.OverlapAdmin = loginInfo.OverlapAdmin;
                        vm.LastEstimateNumber = loginInfo.LastEstimateNumber.ToString();
                        vm.LastWorkOrderNumber = loginInfo.LastWorkOrderNumber.ToString();

                        // Get summary data
                        vm.NumberOfEstimates = Estimate.GetCountForLogin(loginID);
                        vm.ContractDetails = GetContractSummary(loginID, out bool active);
                        vm.ContractActive = active;

                        Contract activeContract = Contract.GetActive(loginID);
                        if (activeContract != null)
                        {
                            vm.HasActiveContract = true;
                        }

                        int logins = 1;
                        if (vm.HasActiveContract)
                        {
                            List<ContractAddOn> muAddOns = ContractAddOn.GetForContract(activeContract.ID).Where(o => o.AddOnType.ID == 8 && o.Active && o.HasPayment).ToList();
                            muAddOns.ForEach(o => { logins += o.Quantity; });
                            List<ContractAddOnTrial> muTrialAddOns = ContractAddOnTrial.GetForContract(activeContract.ID).Where(o => o.AddOnType.ID == 8 && o.StartDate < DateTime.Now && o.EndDate > DateTime.Now).ToList();
                            logins += muTrialAddOns.Count;
                        }

                        vm.NoOfLogins = logins.ToString();

                        // Find the total amount due 
                        decimal totalDue = 0;
                        decimal totalTax = 0;
                        decimal totalInv = 0;

                        List<Invoice> dueInvoices = ContractManager.GetInvoicesToBePaid(loginID).Where(o => o.DueDate.Date <= DateTime.Now.Date).ToList();

                        dueInvoices.ForEach(o =>
                        {
                            totalInv += o.InvoiceAmount;
                            totalDue += o.InvoiceTotal;
                            totalTax += o.SalesTax;
                        });
                        string totalInvAndTax = totalTax == 0 ? "" : " : " + totalInv.ToString("C") + " + " + totalTax.ToString("C") + " taxes";

                        vm.TotalDueDetails = totalDue.ToString("C") + totalInvAndTax;

                        AdminService adminService = new AdminService();
                        vm.ImpersonateLink = "https://" + System.Configuration.ConfigurationManager.AppSettings["server"] + "?impersonate=" + adminService.Encrypt(loginID);

                        List<ProadvisorTotal> proadvisorTotals = ProadvisorTotal.GetForFilter(loginID, "", 0);

                        if (proadvisorTotals.Count() > 0)
                        {
                            vm.ProAdvisorEstimateTotal = proadvisorTotals[0].ProAdvisorEstimateTotal;
                        }
                    }
                }

                return View(vm);
            }
        }

        public JsonResult AutoRenewStatusChange(int loginID, int salesRepID, bool isEnabled)
        {
            try
            {
                if (isEnabled)
                {
                    LoginAutoRenew.Insert(loginID, true);
                }
                else
                {
                    LoginAutoRenew.Insert(loginID, false);
                }
            }
            catch (Exception ex)
            {
                return Json(new FunctionResult(ex.Message), JsonRequestBehavior.AllowGet);
            }

            try
            {
                EmailManager.SendContractAutoRenewChange(loginID, salesRepID);
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e, loginID, salesRepID, "LoginsController.cs AutoRenewStatusChange");
            }

            return Json(new FunctionResult(), JsonRequestBehavior.AllowGet);
        }

        private string GetContractSummary(int loginID, out bool active)
        {
            Contract activeContract = Contract.GetActive(loginID);
            Contract inProgressContract = Contract.GetInProgress(loginID);
            string contractSummary = "";
            active = false;

            if (activeContract != null)
            {
                contractSummary = activeContract.ContractPriceLevel.ContractTerms.TermDescription + " contract: " + activeContract.EffectiveDate.ToShortDateString() + " - " + activeContract.ExpirationDate.ToShortDateString();
                active = true;
            }
            else if (inProgressContract != null)
            {
                contractSummary = inProgressContract.ContractPriceLevel.ContractTerms.TermDescription + " contract: " + inProgressContract.EffectiveDate.ToShortDateString() + " - " + inProgressContract.ExpirationDate.ToShortDateString();
            }
            else
            {
                Trial activeTrial = Trial.GetActive(loginID);
                if (activeTrial != null)
                {
                    contractSummary = "Trial: " + activeTrial.StartDate.ToShortDateString() + " - " + activeTrial.EndDate.ToShortDateString();
                    active = true;
                }
                else
                {
                    // No active contract OR trial.  Find the last expiration date
                    List<Contract> allContracts = Contract.GetAllForLogin(loginID);
                    List<Trial> allTrials = Trial.GetForLogin(loginID);

                    DateTime mostRecent = DateTime.MinValue;

                    foreach (Contract contract in allContracts)
                    {
                        if (contract.ExpirationDate > mostRecent)
                        {
                            mostRecent = contract.ExpirationDate;
                            contractSummary = contract.ContractPriceLevel.ContractTerms.TermDescription + " contract: " + contract.EffectiveDate.ToShortDateString() + " - " + contract.ExpirationDate.ToShortDateString();
                            active = contract.Active && !contract.IsDeleted ? true : false;
                        }
                    }

                    foreach (Trial trial in allTrials)
                    {
                        if (trial.EndDate > mostRecent)
                        {
                            mostRecent = trial.EndDate;
                            contractSummary = "Trial: " + trial.StartDate.ToShortDateString() + " - " + trial.EndDate.ToShortDateString();
                            active = trial.Active && !trial.IsDeleted ? true : false;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(contractSummary))
            {
                contractSummary = "--None--";
            }

            return contractSummary;
        }

        [HttpPost]
        [Route("Logins/List/{loginID}")]
        public ActionResult Index(LoginsPageVM vm)
        {
            StringBuilder errorBuilder = new StringBuilder();

            try
            {
                // Validate the form
                bool isValid = true;

                if (string.IsNullOrEmpty(vm.State))
                {
                    errorBuilder.AppendLine("You must select a State.");
                    isValid = false;
                }

                if (string.IsNullOrEmpty(vm.LoginName))
                {
                    errorBuilder.AppendLine("You must enter a Login Name.");
                    isValid = false;
                }

                if (string.IsNullOrEmpty(vm.Organization))
                {
                    errorBuilder.AppendLine("You must enter an Organization.");
                    isValid = false;
                }

                if (vm.LoginID > 0 && string.IsNullOrEmpty(vm.Password))
                {
                    errorBuilder.AppendLine("You must enter a Password.");
                    isValid = false;
                }

                if (string.IsNullOrEmpty(vm.FirstName))
                {
                    errorBuilder.AppendLine("You must enter a First Name.");
                    isValid = false;
                }

                if (string.IsNullOrEmpty(vm.LastName))
                {
                    errorBuilder.AppendLine("You must enter a Last Name.");
                    isValid = false;
                }

                //if (string.IsNullOrEmpty(vm.EmailAddress))
                //{
                //    errorBuilder.AppendLine("You must enter an Email Address.  This will be used for the login user name.");
                //    isValid = false;
                //}

                if (vm.LoginID == 0 && SiteUser.IsEmailAddressTaken(vm.EmailAddress))
                {
                    errorBuilder.AppendLine("That Email Address is already taken.");
                    isValid = false;
                }

                if (string.IsNullOrEmpty(vm.JobTitle))
                {
                    errorBuilder.AppendLine("You must enter a Job Title.");
                    isValid = false;
                }

                if ((vm.LoginID == 0 || vm.LoginID == -1) && vm.NewSalesRepID == -1)
                {
                    errorBuilder.AppendLine("You must select a Sales Rep.");
                    isValid = false;
                }

                List<LoginInfo> loginMatches = LoginInfo.GetByCredentials(vm.LoginName, vm.Organization, vm.Password);
                if (loginMatches != null)
                {
                    if (loginMatches.Count > 1)
                    {
                        StringBuilder builder = new StringBuilder();
                        string seperator = "";

                        foreach (LoginInfo loginInfo in loginMatches)
                        {
                            builder.Append(seperator + loginInfo.ID.ToString());
                            seperator = ", ";
                        }

                        errorBuilder.AppendLine("Accounts " + builder.ToString() + " have the same Login Name, Organization name and password.  These must be unique, this account will not be saved until these values are unique.");
                        isValid = false;
                    }
                    else if (loginMatches.Count == 1 && loginMatches[0].ID != vm.LoginID)
                    {
                        errorBuilder.AppendLine("Account " + loginMatches[0].ID.ToString() + " has the same Login Name, Organization name and password.  These must be unique, this account will not be saved until these values are unique.");
                        isValid = false;
                    }
                }

                List<LoginInfo> accountsDupEmail = GetAccountsEmailInUse(vm.EmailAddress, vm.LoginID);
                if (accountsDupEmail != null && accountsDupEmail.Count > 0)
                {
                    string delimiter = "";

                    errorBuilder.Append("Accounts ");
                    foreach (LoginInfo account in accountsDupEmail)
                    {
                        errorBuilder.Append(delimiter + account.ID.ToString() + " (" + account.CompanyName + ")");
                        delimiter = ", ";
                    }

                    errorBuilder.Append(accountsDupEmail.Count == 1 ? " has " : " have ");
                    errorBuilder.AppendLine("the same Email Address already in use.&nbsp;&nbsp;A different email address is required to create a new account.");
                    isValid = false;
                }

                if (isValid)
                {
                    LoginInfo loginInfo;

                    if (vm.LoginID <= 0)
                    {
                        Contact contact = new Contact();
                        contact.ContactType = ContactType.Person;
                        contact.ContactSubType = ContactSubType.FocuswriteOrganization;
                        contact.Save(ActiveLogin.ID, ActiveLogin.LoginID);

                        Address address = new Address();
                        address.ContactID = contact.ContactID;
                        address.Save(ActiveLogin.ID, ActiveLogin.LoginID);

                        loginInfo = new LoginInfo();
                        loginInfo.ContactID = contact.ContactID;
                        loginInfo.SalesRepID = vm.NewSalesRepID;
                        SaveResult saveResult = loginInfo.Save(ActiveLogin.ID);

                        vm.SalesRepID = vm.NewSalesRepID;
                        vm.UseDefaultPDRRateProfile = true;
                        vm.UseDefaultRateProfile = true;

                        if (saveResult.Success)
                        {
                            // Create the first admin user
                            FunctionResultInt newUserResult = ProEstimator.Business.Logic.LoginManager<SiteActiveLogin>.CreateUser(loginInfo.ID, vm.EmailAddress, loginInfo.ID.ToString(), true);

                            // Set up the default rate profile
                            int rateProfileID = ProEstimatorData.Models.RateProfileManager.CreateBlankRateProfile(loginInfo.ID);
                            ProEstimatorData.DataModel.Profiles.RateProfile newProfile = ProEstimatorData.DataModel.Profiles.RateProfile.Get(rateProfileID);
                            newProfile.SetAsDefaultProfile();

                            PDR_Manager manager = new PDR_Manager();
                            PDR_RateProfileFunctionResult result = manager.DuplicateRateProfile(1, loginInfo.ID, ActiveLogin.ID);
                            if (result.Success)
                            {
                                result.RateProfile.IsDefault = true;
                                result.RateProfile.Save(ActiveLogin.ID);
                            }

                            FunctionResultInt profileResult = _proAdvisorProfileService.CopyProfile(1, loginInfo.ID, ActiveLogin.ID);
                            if (profileResult.Success)
                            {
                                _proAdvisorProfileService.SetUseDefaultProfile(ActiveLogin.ID, loginInfo.ID, true);
                                ProAdvisorPresetProfile presetProfile = _proAdvisorProfileRepository.GetProfile(profileResult.Value);
                                presetProfile.DefaultFlag = true;
                                _proAdvisorProfileRepository.SaveProfile(ActiveLogin.ID, presetProfile);
                            }

                            ContractManager.ExtendOrCreateTrial(loginInfo.ID, 14, GetSalesRepID());

                            loginInfo.Password = loginInfo.ID.ToString();

                            vm.Password = loginInfo.ID.ToString();
                        }
                    }
                    else
                    {
                        loginInfo = LoginInfo.GetByID(vm.LoginID);
                    }

                    if (loginInfo != null)
                    {
                        bool wasDisabled = loginInfo.Disabled;

                        // Save the address 
                        Address address = Address.GetForContact(loginInfo.ContactID);
                        address.Line1 = vm.Address1;
                        address.Line2 = vm.Address2;
                        address.City = vm.City;
                        address.State = vm.State;
                        address.Zip = vm.Zip;

                        SaveResult addressSave = address.Save(ActiveLogin.ID);
                        if (!addressSave.Success)
                        {
                            errorBuilder.AppendLine("Error saving address: " + addressSave.ErrorMessage);
                        }

                        // Save the Contact
                        Contact contact = Contact.GetContact(loginInfo.ContactID);
                        contact.FirstName = vm.FirstName;
                        contact.LastName = vm.LastName;
                        contact.Title = vm.JobTitle;
                        contact.Email = vm.EmailAddress;
                        contact.PhoneNumberType1 = vm.PhoneType1;
                        contact.Phone = vm.PhoneNumber1;
                        contact.PhoneNumberType2 = vm.PhoneType2;
                        contact.Phone2 = vm.PhoneNumber2;
                        contact.Fax = vm.FaxNumber;

                        SaveResult contactSave = contact.Save(ActiveLogin.ID, ActiveLogin.LoginID);
                        if (!contactSave.Success)
                        {
                            errorBuilder.AppendLine("Error saving contact: " + contactSave.ErrorMessage);
                        }

                        Boolean savedCarfaxExclude = false;
                        if (loginInfo.CarfaxExcludeDate != null && loginInfo.CarfaxExcludeDate != DateTime.MinValue)
                        {
                            savedCarfaxExclude = true;
                        }

                        // Save login info
                        loginInfo.CompanyName = vm.CompanyName;
                        loginInfo.LoginName = vm.LoginName;
                        loginInfo.Organization = vm.Organization;
                        loginInfo.Password = vm.Password;
                        loginInfo.TechSupportPassword = vm.TechSupportPassword;
                        loginInfo.Disabled = vm.Disabled;
                        loginInfo.DoubtfulAccount = vm.DoubtfulAccount;
                        loginInfo.StaffAccount = vm.StaffAccount;
                        loginInfo.Appraiser = vm.Appraiser;
                        loginInfo.NoOfLogins = InputHelper.GetInteger(vm.NoOfLogins);
                        loginInfo.CompanyOrigin = vm.CompanyOrigin;
                        loginInfo.CompanyType = vm.CompanyType;
                        loginInfo.HeaderContact = vm.HeaderContact;
                        loginInfo.SalesRepID = vm.SalesRepID;
                        loginInfo.RegistrationNumber = vm.RegistrationNumber;
                        loginInfo.ShowRepairShopProfiles = vm.ShowRepairShopProfiles;
                        loginInfo.AllowAlternateIdentities = vm.AllowAlternateIdentities;
                        loginInfo.ShowLaborTimeWO = vm.ShowLaborTimeWO;
                        loginInfo.LicenseNumber = vm.LicenseNumber;
                        loginInfo.BarNumber = vm.BarNumber;
                        loginInfo.FederalTaxID = vm.FederalTaxID;
                        loginInfo.UseDefaultRateProfile = vm.UseDefaultRateProfile;
                        loginInfo.UseDefaultPDRRateProfile = vm.UseDefaultPDRRateProfile;
                        loginInfo.ProfileLocked = vm.ProfileLocked;
                        loginInfo.PartsNow = vm.PartsNow;
                        loginInfo.OverlapAdmin = vm.OverlapAdmin;
                        loginInfo.LastEstimateNumber = InputHelper.GetInteger(vm.LastEstimateNumber);
                        loginInfo.LastWorkOrderNumber = InputHelper.GetInteger(vm.LastWorkOrderNumber);
                        loginInfo.LanguageID = vm.LanguageID;
                        loginInfo.IntelliPayMerchantKey = vm.IntelliPayMerchantKey;
                        loginInfo.IntelliPayAPIKey = vm.IntelliPayAPIKey;
                        loginInfo.IntelliPayUseCardReader = vm.IntelliPayUseCardReader;

                        if (ProEstimator.Business.Logic.Admin.SalesRepPermissionManager.HasPermission(GetSalesRepID(), "CarFax"))
                        {
                            if (vm.CarfaxExclude == true)
                            {
                                if (!savedCarfaxExclude)
                                {
                                    loginInfo.CarfaxExcludeDate = DateTime.Now;

                                    MiscTracking.Insert(loginInfo.ID, 0, "CarFaxRemove", GetSalesRepID().ToString());

                                    vm.CarfaxExcludeDateStr = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt");

                                    SalesRep salesRep = SalesRep.Get(GetSalesRepID());
                                    if (salesRep != null)
                                    {
                                        vm.ModifiedBySalesRepName = salesRep.FirstName + " " + salesRep.LastName;
                                    }
                                }
                            }
                            else
                            {
                                loginInfo.CarfaxExcludeDate = null;

                                if (savedCarfaxExclude)
                                {
                                    MiscTracking.Insert(loginInfo.ID, 0, "CarFaxAdd", GetSalesRepID().ToString());

                                    vm.CarfaxExcludeDateStr = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt");

                                    SalesRep salesRep = SalesRep.Get(GetSalesRepID());
                                    if (salesRep != null)
                                    {
                                        vm.ModifiedBySalesRepName = salesRep.FirstName + " " + salesRep.LastName;
                                    }
                                }
                            }
                        }

                        if (vm.CarfaxExclude == true && (loginInfo.CarfaxExcludeDate == null || loginInfo.CarfaxExcludeDate == DateTime.MinValue))
                        {
                            loginInfo.CarfaxExcludeDate = DateTime.Today;
                        }

                        SaveResult loginInfoSave = loginInfo.Save(ActiveLogin.ID);
                        if (!loginInfoSave.Success)
                        {
                            errorBuilder.AppendLine("Error saving account info: " + loginInfoSave.ErrorMessage);
                        }
                        else
                        {
                            // Synce the contract with Success box.
                            //Contract contract = Contract.GetActive(vm.LoginID);
                            //if (contract != null)
                            //{
                            //    SuccessBoxDataSyncer.SuccessBoxContractLogUpdate(contract.ID, 0, false);
                            //}

                            if (ProEstimator.Business.Logic.Admin.SalesRepPermissionManager.HasPermission(GetSalesRepID(), "DeleteCCInfo") && vm.SelectedStripeInfoID > 0)
                            {
                                StripeInfo stripeInfo = StripeInfo.GetStripeInfo(vm.SelectedStripeInfoID);
                                if (stripeInfo != null && stripeInfo.DeleteFlag != vm.CcInfoDeleteFlag)
                                {
                                    stripeInfo.DeleteFlag = vm.CcInfoDeleteFlag;
                                    stripeInfo.Save(ActiveLogin.ID);
                                    string tag = stripeInfo.DeleteFlag ? "CCInfoDeleted" : "CCInfoUn-Deleted";
                                    string salesRepName = "";
                                    SalesRep salesRep = SalesRep.Get(GetSalesRepID());
                                    if (salesRep != null)
                                    {
                                        salesRepName = salesRep.FirstName + " " + salesRep.LastName;
                                    }
                                    MiscTracking.Insert(loginInfo.ID, stripeInfo.ID, tag, salesRepName);
                                }
                            }


                            // If the account was disabled, tell the front end site about it.
                            if (!wasDisabled && loginInfo.Disabled)
                            {
                                string frontEndRoot = "https://proestimator.web-est.com/";

                                try
                                {
                                    frontEndRoot = System.Configuration.ConfigurationManager.AppSettings.Get("FrontEndRootUrl").ToString();
                                }
                                catch
                                {
                                    ErrorLogger.LogError("Admin, FrontEndRootUrl config not set.", "Admin FrontEndRootUrl");
                                }

                                try
                                {
                                    System.Net.WebClient client = new System.Net.WebClient();
                                    string kickLink = frontEndRoot + "Login/KickLogin?loginID=" + loginInfo.ID.ToString() + "&code=" + InputHelper.GetHash(loginInfo.ID.ToString(), 10);
                                    vm.ExtraSaveMessage = client.DownloadString(kickLink).Replace("\"", "");
                                }
                                catch (Exception ex)
                                {
                                    ErrorLogger.LogError(ex, 0, "SalesRepController KickLogin");
                                }
                            }

                            // If there are no Estimators attached to the account, create one using the contact's first and last name
                            List<Estimator> estimators = Estimator.GetByLogin(vm.LoginID);
                            if (estimators == null || estimators.Count == 0)
                            {
                                Estimator newEstimator = new Estimator();
                                newEstimator.FirstName = vm.FirstName;
                                newEstimator.LastName = vm.LastName;
                                newEstimator.LoginID = vm.LoginID;
                                newEstimator.DefaultEstimator = true;

                                SaveResult estimatorSave = newEstimator.Save(0);
                                if (estimatorSave.Success)
                                {
                                    ErrorLogger.LogError("Created first estimator " + newEstimator.FirstName + " " + newEstimator.LastName + " for account " + newEstimator.LoginID, "CreateFirstEstimator");
                                }
                                else
                                {
                                    ErrorLogger.LogError("Account " + newEstimator.LoginID + " error: " + estimatorSave.ErrorMessage, "CreateFirstEstimator Error");
                                }
                            }

                            vm.GoodSave = true;

                            if (vm.LoginID <= 0)
                            {
                                return Redirect("/Logins/List/" + loginInfo.ID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorBuilder.AppendLine(ex.Message);
            }

            vm.GoodData = true;
            vm.ErrorMessage = errorBuilder.ToString().Replace(Environment.NewLine, "<br />");
            if (vm.LoginID == -1)
            {
                vm.LoginID = 0;
            }

            return View(vm);
        }

        public JsonResult GetImpersonateLinks(int loginID)
        {
            AdminService adminService = new AdminService();
            List<ImpersonateLink> impersonateLinks = new List<ImpersonateLink>();

            string server = System.Configuration.ConfigurationManager.AppSettings["server"];
            string http = "https";

            if (server.ToLower().Contains("dev") || server.ToLower().Contains("localhost"))
            {
                http = "http";
            }

            if (AdminIsValidated())
            {
                List<SiteUser> users = SiteUser.GetForLogin(loginID);

                foreach (SiteUser user in users)
                {
                    impersonateLinks.Add(new ImpersonateLink() { Link = http + "://" + server + "?impersonate=" + adminService.Encrypt(user.ID), UserName = user.EmailAddress });
                }
            }

            return Json(impersonateLinks, JsonRequestBehavior.AllowGet);
        }

        public class ImpersonateLink
        {
            public string Link { get; set; }
            public string UserName { get; set; }
        }

        public JsonResult ExtendTrial(int loginID, int days, int salesRepID)
        {
            TrialFunctionResult trialResult;

            FunctionResult result = ContractManager.ExtendOrCreateTrial(loginID, days, salesRepID);
            if (result.Success)
            {
                string response = GetContractSummary(loginID, out bool active);
                trialResult = new TrialFunctionResult(active, response);
            }
            else
            {
                trialResult = new TrialFunctionResult(result.ErrorMessage);
            }

            return Json(trialResult, JsonRequestBehavior.AllowGet);
        }

        public class TrialFunctionResult : FunctionResult
        {
            public string Response { get; set; }
            public bool TrialActive { get; set; }
            public TrialFunctionResult()
                : base()
            {
            }

            public TrialFunctionResult(string errorMessage)
                : base(errorMessage)
            {
            }

            public TrialFunctionResult(bool active, string detail)
                : base()
            {
                TrialActive = active;
                Response = detail;
            }
        }

        public ActionResult GetExtensionHistory([DataSourceRequest] DataSourceRequest request, int loginID)
        {
            List<ExtensionHistoryVM> history = new List<ExtensionHistoryVM>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("[Admin].[Getextensionhistory]", new System.Data.SqlClient.SqlParameter("id", loginID));

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    history.Add(new ExtensionHistoryVM(row));
                }
            }

            return Json(history.ToDataSourceResult(request));
        }

        public ActionResult GetLoginContractAddOnList([DataSourceRequest] DataSourceRequest request, int loginID)
        {
            List<LoginContractAddOnVM> loginContractAddOnVMs = new List<LoginContractAddOnVM>();

            Contract activeContract = Contract.GetActive(loginID);

            if (activeContract != null)
            {
                if (AdminIsValidated())
                {
                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(activeContract.ID).Where(o => o.HasPayment).ToList();
                    List<ContractAddOnTrial> trialAddOns = ContractAddOnTrial.GetForContract(activeContract.ID).Where(o => o.StartDate <= DateTime.Now && o.EndDate >= DateTime.Now).ToList();
                    loginContractAddOnVMs = LoginContractAddOnVM.GetLoginContractAddOnVMList(addOns, trialAddOns, true);
                }
            }
            else
            {
                Trial activeTrial = Trial.GetActive(loginID);
                if (activeTrial != null)
                {
                    if (AdminIsValidated())
                    {
                        loginContractAddOnVMs = LoginContractAddOnVM.GetLoginContractAddOnVMList(activeTrial, true);
                    }
                }
                else
                {
                    // No active contract OR trial.  Find the last expiration date
                    List<Contract> allContracts = Contract.GetAllForLogin(loginID).OrderByDescending(o => o.ExpirationDate).ToList();
                    List<Trial> allTrials = Trial.GetForLogin(loginID).OrderByDescending(o => o.EndDate).ToList();

                    DateTime mostRecent = DateTime.MinValue;

                    foreach (Contract contract in allContracts)
                    {
                        if (contract.ExpirationDate > mostRecent)
                        {
                            mostRecent = contract.ExpirationDate;

                            List<ContractAddOn> addOns = ContractAddOn.GetForContract(contract.ID).Where(o => o.HasPayment).ToList();
                            List<ContractAddOnTrial> trialAddOns = ContractAddOnTrial.GetForContract(contract.ID);

                            loginContractAddOnVMs = LoginContractAddOnVM.GetLoginContractAddOnVMList(addOns, trialAddOns, false);
                        }
                    }

                    foreach (Trial trial in allTrials)
                    {
                        if (trial.EndDate > mostRecent)
                        {
                            mostRecent = trial.EndDate;
                            loginContractAddOnVMs = LoginContractAddOnVM.GetLoginContractAddOnVMList(trial, false);
                        }
                    }
                }
            }

            return Json(loginContractAddOnVMs.ToDataSourceResult(request));
        }

        public ActionResult GetSavedPaymentInfoHistory([DataSourceRequest] DataSourceRequest request, int loginID, Boolean deleteFlag)
        {
            List<PaymentInfoHistoryVM> paymentInfoHistory = new List<PaymentInfoHistoryVM>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("loginID", loginID));

            if (deleteFlag == false)
            {
                parameters.Add(new SqlParameter("deleteFlag", deleteFlag));
            }
            else
            {
                parameters.Add(new SqlParameter("deleteFlag", DBNull.Value));
            }

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("[dbo].[StripeInfoGetByLogin]", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    paymentInfoHistory.Add(new PaymentInfoHistoryVM(row));
                }
            }

            return Json(paymentInfoHistory.ToDataSourceResult(request));
        }

        public JsonResult GetSavedPaymentInfoDetails(int stripeInfoID, int salesRepID)
        {
            if (AdminIsValidated())
            {
                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("[dbo].[StripeInfoGet]", new SqlParameter("id", stripeInfoID));

                if (tableResult.Success)
                {
                    PaymentInfoHistoryVM paymentInfoHistoryVM = new PaymentInfoHistoryVM(tableResult.DataTable.Rows[0]);
                    List<DeletePaymentInfoStatusVM> status = new List<DeletePaymentInfoStatusVM>();
                    if (ProEstimator.Business.Logic.Admin.SalesRepPermissionManager.HasPermission(salesRepID, "DeleteCCInfo"))
                    {
                        List<MiscTracking> deletes = GetDeleteHistory(stripeInfoID);
                        foreach (MiscTracking delHistory in deletes)
                        {
                            status.Add(new DeletePaymentInfoStatusVM(delHistory.TimeStamp, delHistory.Tag, delHistory.OtherData));
                        }
                    }
                    paymentInfoHistoryVM.DeleteStatus = status;
                    return Json(paymentInfoHistoryVM, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Admin is not validated", JsonRequestBehavior.AllowGet);
            }
        }

        private List<MiscTracking> GetDeleteHistory(int stripeInfoID)
        {
            List<MiscTracking> delete = MiscTracking.Get(0, stripeInfoID, "CCInfoDeleted");
            List<MiscTracking> undelete = MiscTracking.Get(0, stripeInfoID, "CCInfoUn-Deleted");

            List<MiscTracking> returnList = new List<MiscTracking>();
            returnList.AddRange(delete);
            returnList.AddRange(undelete);

            return returnList.OrderByDescending(o => o.TimeStamp).ToList();
        }

        public ActionResult GetAutoRenewHistory([DataSourceRequest] DataSourceRequest request, int loginID)
        {
            List<AutoRenewStatusVM> autoRenewList = new List<AutoRenewStatusVM>();
            IEnumerable<Email> emailList = Email.GetForFilter(loginID.ToString(), null, null, null, "Auto Renew turned", null, null, null).OrderByDescending(o => o.CreateStamp);
            foreach (Email email in emailList)
            {
                string[] words = email.Subject.Split(' ');
                int i = 0;
                if (Array.Exists(words, x => x == loginID.ToString())) i = 1;
                string action = words.Length >= 5 + i ? words[4 + i].ToUpper() : "N/A";
                string repName = words.Length >= 8 + i ? words[6 + i] + " " + words[7 + i] : "Customer";

                autoRenewList.Add(new AutoRenewStatusVM(email.LoginID, email.CreateStamp, action, repName));
            }

            return Json(autoRenewList.ToDataSourceResult(request));
        }

        public ActionResult GetAutoPayHistory([DataSourceRequest] DataSourceRequest request, int loginID)
        {
            List<AutoPayChangeVM> rows = new List<AutoPayChangeVM>();

            List<MiscTracking> miscTrackingList = GetAutoPayHistoryService.GetAutoPayHistory(loginID);

            foreach (MiscTracking miscTracking in miscTrackingList)
            {
                rows.Add(new AutoPayChangeVM(miscTracking));
            }

            return Json(rows.ToDataSourceResult(request));
        }

        public ActionResult GetInvoiceFails([DataSourceRequest] DataSourceRequest request, int loginID)
        {
            return Json(_failureSummaryService.GetInvoiceFails(loginID).ToDataSourceResult(request));
        }

        public ActionResult GetUsers([DataSourceRequest] DataSourceRequest request, int loginID, Boolean deleteFlag)
        {
            List<SiteUserVM> siteUserVMs = new List<SiteUserVM>();

            if (AdminIsValidated())
            {
                List<SiteUser> users = SiteUser.GetForLogin(loginID, deleteFlag);
                foreach (SiteUser user in users)
                {
                    siteUserVMs.Add(new SiteUserVM(user));
                }
            }

            return Json(siteUserVMs.ToDataSourceResult(request));
        }

        public JsonResult GetUserData(int userID)
        {
            if (AdminIsValidated())
            {
                SiteUser user = SiteUser.Get(userID);
                if (user != null)
                {
                    SiteUserVM userVM = new SiteUserVM(user);

                    List<SitePermission> permissions = SitePermission.GetAll();
                    List<SitePermissionLink> permissionLinks = SitePermissionLink.GetForUser(userID);

                    foreach (SitePermissionLink link in permissionLinks)
                    {
                        SitePermission permission = permissions.FirstOrDefault(o => o.ID == link.PermissionID);
                        if (permission != null)
                        {
                            userVM.HasPermissions.Add(new SiteUserHasPermissionVM(permission.Tag, link.HasPermission));
                        }
                    }

                    return Json(userVM, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveUserData(int userID, int loginID, string name, string emailAddress, string password, string permissions)
        {
            if (AdminIsValidated())
            {
                if (loginID <= 0)
                {
                    return Json(new FunctionResult("No Login ID Passed."), JsonRequestBehavior.AllowGet);
                }

                SiteUser siteUser;

                if (userID > 0)
                {
                    siteUser = SiteUser.Get(userID);
                }
                else
                {
                    siteUser = new SiteUser();
                    siteUser.LoginID = loginID;
                }

                siteUser.Name = name;
                siteUser.EmailAddress = emailAddress;

                if (!string.IsNullOrEmpty(password))
                {
                    siteUser.Password = password;
                }

                SaveResult saveResult = siteUser.Save(ActiveLogin.ID);

                NewIDFunctionResult functionResult = new NewIDFunctionResult();
                if (saveResult.Success)
                {
                    List<SitePermission> permissionsList = SitePermission.GetAll();
                    List<SitePermissionLink> permissionLinks = SitePermissionLink.GetForUser(siteUser.ID);

                    string[] permissionsPieces = permissions.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string permissionsPiece in permissionsPieces)
                    {
                        string[] pieces = permissionsPiece.Split("--".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string tag = pieces[0];
                        bool hasPermission = InputHelper.GetBoolean(pieces[1]);

                        SitePermission permission = permissionsList.FirstOrDefault(o => o.Tag == tag);
                        if (permission != null)
                        {
                            SitePermissionLink link = permissionLinks.FirstOrDefault(o => o.PermissionID == permission.ID);
                            if (link == null)
                            {
                                link = new SitePermissionLink();
                                link.PermissionID = permission.ID;
                                link.SiteUserID = siteUser.ID;
                                link.HasPermission = true;
                            }

                            if (link.HasPermission != hasPermission)
                            {
                                link.HasPermission = hasPermission;
                                link.Save();
                            }
                        }
                    }

                    functionResult.NewID = siteUser.ID;
                    functionResult.Success = true;
                }
                else
                {
                    functionResult.ErrorMessage = saveResult.ErrorMessage;
                    functionResult.Success = false;
                }

                return Json(functionResult, JsonRequestBehavior.AllowGet);
            }

            return Json(new FunctionResult("Admin not validated."), JsonRequestBehavior.AllowGet);
        }

        public class NewIDFunctionResult : FunctionResult
        {
            public int NewID { get; set; }
        }

        public JsonResult DeleteUser(int userID)
        {
            return Json(ToggleUserDeleted(userID), JsonRequestBehavior.AllowGet);
        }

        private FunctionResult ToggleUserDeleted(int userID)
        {
            if (AdminIsValidated())
            {
                SiteUser user = SiteUser.Get(userID);
                if (user != null)
                {
                    user.IsDeleted = !user.IsDeleted;
                    SaveResult saveResult = user.Save(ActiveLogin.ID);
                    return new FunctionResult(saveResult.ErrorMessage);
                }
            }

            return new FunctionResult("Invalid admin or user.");
        }

        public ActionResult GetLoginsSearch([DataSourceRequest] DataSourceRequest request, string loginID, string loginName, string estimateID, string emailAddress, string vin)
        {
            List<LoginSearchVM> logins = new List<LoginSearchVM>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("SearchLoginID", loginID));
            parameters.Add(new SqlParameter("EstimateID", estimateID));
            parameters.Add(new SqlParameter("SearchName", loginName));
            parameters.Add(new SqlParameter("EmailAddress", emailAddress));
            parameters.Add(new SqlParameter("Vin", vin));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("AdminUserSearch", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    logins.Add(new LoginSearchVM(row));
                }
            }

            return Json(logins.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private List<LoginInfo> GetAccountsEmailInUse(string email, int ignoreLoginID)
        {
            List<LoginInfo> accounts = new List<LoginInfo>();

            List<LoginInfo> emailMatches = LoginInfo.GetByEmailAddress(email);
            foreach (LoginInfo emailMatch in emailMatches.Where(o => o.ID != ignoreLoginID))
            {
                accounts.Add(emailMatch);
            }

            return accounts;
        }

        public ActionResult GetSuccessBoxFeatureCount([DataSourceRequest] DataSourceRequest request, int loginID, string startDate, string endDate, bool incImperson)
        {
            return Json(SuccessBoxFeatureLog.GetCountByFeatureForLogin(loginID, startDate, endDate, incImperson).ToDataSourceResult(request));
        }

        public ActionResult GetSuccessBoxFeatureLogs([DataSourceRequest] DataSourceRequest request, int loginID, string featureTag, string startDate, string endDate, bool incImperson)
        {
            return Json(SuccessBoxFeatureLog.GetFeatureLogs(loginID, featureTag, startDate, endDate, incImperson).ToDataSourceResult(request));
        }

        public ActionResult GetUserActivity([DataSourceRequest] DataSourceRequest request, int loginID, int userID)
        {
            return Json(ActiveLogin.GetUserLoginActivity(loginID, userID).ToDataSourceResult(request));
        }

        public ActionResult GetCarFaxHistory([DataSourceRequest] DataSourceRequest request, int loginID)
        {
            List<CarFaxDetail> details = new List<CarFaxDetail>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CarFax_GetHistory", new SqlParameter("LoginID", loginID));

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                details.Add(new CarFaxDetail(row));
            }

            return Json(details.ToDataSourceResult(request));
        }

        public class CarFaxDetail
        {
            public DateTime TimeStamp { get; set; }
            public int SalesRepID { get; set; }
            public string UserName { get; set; }
            public string FullName { get; set; }
            public string Action { get; set; }

            public CarFaxDetail(DataRow row)
            {
                TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
                SalesRepID = InputHelper.GetInteger(row["SalesRepID"].ToString());
                UserName = InputHelper.GetString(row["UserName"].ToString());
                FullName = InputHelper.GetString(row["FullName"].ToString());
                Action = InputHelper.GetString(row["Action"].ToString());
            }
        }

        public JsonResult ToggleRemoteSupport(int loginID, Boolean isRemoteSupportEnable)
        {
            string frontEndRoot = "https://proestimator.web-est.com/";
            string result = string.Empty;

            string combinedEncoded = ProEstimatorData.Encryptor.Encrypt(loginID.ToString());
            combinedEncoded = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(combinedEncoded);

            try
            {
                frontEndRoot = System.Configuration.ConfigurationManager.AppSettings.Get("FrontEndRootUrl").ToString();
            }
            catch
            {
                ErrorLogger.LogError("Admin, FrontEndRootUrl config not set.", "Admin FrontEndRootUrl");
            }

            try
            {
                System.Net.WebClient client = new System.Net.WebClient();

                string remoteSupportActionURL = isRemoteSupportEnable ? "Hooks/TurnOnRemoteSupportLink" : "Hooks/TurnOffRemoteSupportLink";

                string link = frontEndRoot + remoteSupportActionURL + "?one=" + combinedEncoded;

                ErrorLogger.LogError(link, "CacheLink");

                result = client.DownloadString(link);
            }
            catch (Exception ex)
            {
                result = "error";
                ErrorLogger.LogError(ex, 0, "AdminController ToggleRemoteSupport");
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckIfRemoteSupportEnabled(int loginID)
        {
            string frontEndRoot = "https://proestimator.web-est.com/";
            string result = string.Empty;

            string combinedEncoded = ProEstimatorData.Encryptor.Encrypt(loginID.ToString());
            combinedEncoded = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(combinedEncoded);

            try
            {
                frontEndRoot = System.Configuration.ConfigurationManager.AppSettings.Get("FrontEndRootUrl").ToString();
            }
            catch
            {
                ErrorLogger.LogError("Admin, FrontEndRootUrl config not set.", "Admin FrontEndRootUrl");
            }

            try
            {
                System.Net.WebClient client = new System.Net.WebClient();

                string link = frontEndRoot + "Hooks/IsTurnOnRemoteSupportLink?one=" + combinedEncoded;

                ErrorLogger.LogError(link, "CacheLink");

                result = client.DownloadString(link);
            }
            catch (Exception ex)
            {
                result = "error";
                ErrorLogger.LogError(ex, 0, "AdminController CheckIfRemoteSupportEnabled");
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}