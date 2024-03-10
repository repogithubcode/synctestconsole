using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Logic.Admin;
using ProEstimatorData;
using ProEstimator.Business.OptOut;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Logic
{
    public static class EmailManager
    {

        private static OptOutService OptOutService
        {
            get
            {
                if (_optOutService == null)
                {
                    _optOutService = new OptOutService();
                }

                return _optOutService;
            }
        }
        private static OptOutService _optOutService;

        /// <summary>
        /// Send an email to the main company contact that their Contract Auto Renew status has changed.
        /// </summary>
        /// <param name="loginID"></param>
        public static void SendContractAutoRenewChange(int loginID, int salesRepID = -1)
        {
            try
            {
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                if (loginInfo != null)
                {
                    Contact contact = Contact.GetContactForLogins(loginID);

                    if (!string.IsNullOrEmpty(contact.Email))
                    {
                        Email email = new Email();
                        email.LoginID = loginID;
                        email.AddToAddress(contact.Email);

                        SalesRep who = null;
                        if (salesRepID > -1)
                        {
                            who = SalesRep.Get(salesRepID);
                        }
                        if (who != null)
                        {
                            email.AddToAddress(who.Email);
                        }

                        SalesRep salesRep = SalesRep.Get(loginInfo.SalesRepID);
                        if (salesRep != null)
                        {
                            email.AddCCAddress(salesRep.Email);
                        }
                        EmailTemplate template;

                        LoginAutoRenew autoRenew = LoginAutoRenew.GetLastForLogin(loginID);
                        if (autoRenew != null && autoRenew.Enabled)
                        {
                            template = EmailTemplate.Get(1);

                            List<string> emailAddresses = SalesRepPermissionManager.GetSalesRepEmailAddressForPermissionTag("IsEmailSentAccountRenewOn");

                            foreach (string eachSalesRepStaffEmailSent in emailAddresses)
                            {
                                email.AddCCAddress(eachSalesRepStaffEmailSent);
                            }
                        }
                        else
                        {
                            template = EmailTemplate.Get(2);

                            List<string> emailAddresses = SalesRepPermissionManager.GetSalesRepEmailAddressForPermissionTag("IsEmailSentAutoRenewOff");

                            foreach (string eachSalesRepStaffEmailSent in emailAddresses)
                            {
                                email.AddCCAddress(eachSalesRepStaffEmailSent);
                            }
                        }

                        email.TemplateID = template.ID;

                        Contract activeContract = Contract.GetActive(loginID);
                        string expirationDate = "'Not Set'";
                        if (activeContract != null)
                        {
                            expirationDate = activeContract.ExpirationDate.ToShortDateString();
                        }

                        List<TagAndValue> tagsSubj = new List<TagAndValue>();
                        tagsSubj.Add(new TagAndValue("LoginId", loginID.ToString()));
                        email.Subject = template.ProcessTemplate(tagsSubj, template.Subject);
                        if (who != null)
                        {
                            email.Subject += " by " + who.FirstName.Trim() + " " + who.LastName.Trim() + " per your request";
                        }

                        List<TagAndValue> tags = new List<TagAndValue>();
                        tags.Add(new TagAndValue("FirstName", contact.FirstName));
                        tags.Add(new TagAndValue("ExpirationDate", expirationDate));
                        tags.Add(new TagAndValue("Id", "#" + loginID.ToString()));

                        email.Body = template.ProcessTemplate(tags);

                        if (template != null && SpamPreventionCheck(loginID, template.ID, 1))
                        {
                            EmailSender.SendEmail(email);
                        }
                        email.Save(0);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "SendContractAutoRenewChange");
            }
        }

        public static void SendContractAutoRenewWarning(int loginID)
        {
            try
            {
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                if (loginInfo != null)
                {
                    Contact contact = Contact.GetContactForLogins(loginID);
                    Contract activeContract = Contract.GetActive(loginID);

                    if (activeContract != null)
                    {
                        List<TagAndValue> tags = new List<TagAndValue>();
                        tags.Add(new TagAndValue("FirstName", contact.FirstName));
                        tags.Add(new TagAndValue("Date", activeContract.ExpirationDate.ToShortDateString()));
                        tags.Add(new TagAndValue("LoginID", loginID.ToString()));

                        List<TagAndValue> tagsSubj = new List<TagAndValue>();
                        tagsSubj.Add(new TagAndValue("LoginId", loginID.ToString()));

                        Email email = new Email();
                        email.LoginID = loginID;
                        email.AddToAddress(contact.Email);

                        SalesRep salesRep = SalesRep.Get(loginInfo.SalesRepID);
                        if (salesRep != null)
                        {
                            email.AddCCAddress(salesRep.Email);
                        }

                        List<string> emailAddresses = SalesRepPermissionManager.GetSalesRepEmailAddressForPermissionTag("IsEmailSentAutoRenewWarning");

                        foreach (string eachSalesRepStaffEmailSent in emailAddresses)
                        {
                            email.AddCCAddress(eachSalesRepStaffEmailSent);
                        }

                        EmailTemplate template = EmailTemplate.Get(3);
                        email.TemplateID = template.ID;
                        email.Subject = template.ProcessTemplate(tagsSubj, template.Subject);
                        email.Body = template.ProcessTemplate(tags);

                        if (template != null && SpamPreventionCheck(loginID, template.ID, 10))
                        {
                            email.Save(0);
                            EmailSender.SendEmail(email);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "SendContractAutoRenewWarning");
            }
        }

        public static void SendContractAutoRenewDone(int loginID)
        {
            try
            {
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                if (loginInfo != null)
                {
                    Contact contact = Contact.GetContactForLogins(loginID);

                    List<TagAndValue> tags = new List<TagAndValue>();
                    tags.Add(new TagAndValue("FirstName", contact.FirstName));

                    List<TagAndValue> tagsSubj = new List<TagAndValue>();
                    tagsSubj.Add(new TagAndValue("LoginId", loginID.ToString()));

                    Email email = new Email();
                    email.LoginID = loginID;
                    email.AddToAddress(contact.Email);

                    SalesRep salesRep = SalesRep.GetAll().FirstOrDefault(o => o.SalesRepID == loginInfo.SalesRepID);

                    if (!string.IsNullOrEmpty(salesRep.Email))
                    {
                        email.AddCCAddress(salesRep.Email);
                    }

                    List<string> emailAddresses = SalesRepPermissionManager.GetSalesRepEmailAddressForPermissionTag("IsEmailSentAutoRenewComplete");

                    foreach (string eachSalesRepStaffEmailSent in emailAddresses)
                    {
                        email.AddCCAddress(eachSalesRepStaffEmailSent);
                    }

                    EmailTemplate template = EmailTemplate.Get(4);
                    email.TemplateID = template.ID;
                    email.Subject = template.ProcessTemplate(tagsSubj, template.Subject);
                    email.Body = template.ProcessTemplate(tags);

                    if (template != null && SpamPreventionCheck(loginID, template.ID, 1))
                    {
                        email.Save(0);
                        EmailSender.SendEmail(email);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "SendContractAutoRenewDone");
            }
        }

        public static void SendContractAutoRenewDone(int loginID, int contractID)
        {
            try
            {
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                if (loginInfo != null)
                {
                    Contact contact = Contact.GetContactForLogins(loginID);

                    List<TagAndValue> tags = new List<TagAndValue>();
                    tags.Add(new TagAndValue("FirstName", contact.FirstName));

                    List<TagAndValue> tagsSubj = new List<TagAndValue>();
                    tagsSubj.Add(new TagAndValue("LoginId", loginID.ToString()));

                    Email email = new Email();
                    email.LoginID = loginID;
                    email.AddToAddress(contact.Email);

                    SalesRep salesRep = SalesRep.GetAll().FirstOrDefault(o => o.SalesRepID == loginInfo.SalesRepID);

                    if (!string.IsNullOrEmpty(salesRep.Email))
                    {
                        email.AddCCAddress(salesRep.Email);
                    }
                    List<string> emailAddresses = SalesRepPermissionManager.GetSalesRepEmailAddressForPermissionTag("IsEmailSentAutoRenewUponAutPay");

                    foreach (string eachSalesRepStaffEmailSent in emailAddresses)
                    {
                        email.AddCCAddress(eachSalesRepStaffEmailSent);
                    }

                    Contract autoRenewed = Contract.Get(contractID);
                    if (autoRenewed != null)
                    {
                        tags.Add(new TagAndValue("Contract", autoRenewed.ContractPriceLevel.ContractTerms.TermDescription));
                        tags.Add(new TagAndValue("EffectiveDate", autoRenewed.EffectiveDate.ToShortDateString()));
                        tags.Add(new TagAndValue("ExpirationDate", autoRenewed.ExpirationDate.ToShortDateString()));

                        StringBuilder addOnBuilder = new StringBuilder();
                        addOnBuilder.Append("");
                        List<ContractAddOn> autoRenewAddOns = ContractAddOn.GetForContract(contractID);
                        foreach (ContractAddOn addOn in autoRenewAddOns)
                        {
                            addOnBuilder.AppendLine("   ");
                            addOnBuilder.Append(addOn.AddOnType.Type);
                            addOnBuilder.Append(" : ");
                            addOnBuilder.Append(addOn.PriceLevel.ContractTerms.TermDescription);
                        }
                        tags.Add(new TagAndValue("AddOn", addOnBuilder.ToString()));
                    }

                    EmailTemplate template = EmailTemplate.Get(8);
                    if (template != null && SpamPreventionCheck(loginID, template.ID, 60))
                    {
                        email.TemplateID = template.ID;
                        email.Subject = template.ProcessTemplate(tagsSubj, template.Subject);
                        email.Body = template.ProcessTemplate(tags);

                        email.Save(0);
                        EmailSender.SendEmail(email);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "SendContractAutoRenewDone");
            }
        }

        public static void SendContractEarlyRenewDone(int loginID, int contractID)
        {
            try
            {
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                if (loginInfo != null)
                {
                    Contact contact = Contact.GetContactForLogins(loginID);

                    List<TagAndValue> tags = new List<TagAndValue>();
                    tags.Add(new TagAndValue("FirstName", string.IsNullOrEmpty(contact.FirstName) ? "Valued Customer" : contact.FirstName));

                    SalesRep salesRep = SalesRep.GetAll().FirstOrDefault(o => o.SalesRepID == loginInfo.SalesRepID);
                    if (salesRep != null)
                    {
                        tags.Add(new TagAndValue("SalesRep", salesRep.FirstName + " " + salesRep.LastName));
                    }
                    else
                    {
                        tags.Add(new TagAndValue("SalesRep", "Web-est"));
                    }

                    List<TagAndValue> tagsSubj = new List<TagAndValue>();
                    tagsSubj.Add(new TagAndValue("LoginId", loginID.ToString()));

                    Email email = new Email();
                    email.LoginID = loginID;
                    email.AddToAddress(contact.Email);

                    if (salesRep != null && !string.IsNullOrEmpty(salesRep.Email))
                    {
                        email.AddCCAddress(salesRep.Email);
                    }

                    List<string> emailAddresses = SalesRepPermissionManager.GetSalesRepEmailAddressForPermissionTag("IsEmailSentContractEarlyRenewDone");

                    foreach (string eachSalesRepStaffEmailSent in emailAddresses)
                    {
                        email.AddCCAddress(eachSalesRepStaffEmailSent);
                    }

                    if (loginInfo.SalesRepID == 1)
                    {
                        email.AddCCAddress("sables@web-est.com");
                    }

                    Contract earlyRenewed = Contract.Get(contractID);
                    if (earlyRenewed != null)
                    {
                        tags.Add(new TagAndValue("Contract", earlyRenewed.ContractPriceLevel.ContractTerms.TermDescription));
                        tags.Add(new TagAndValue("EffectiveDate", earlyRenewed.EffectiveDate.ToShortDateString()));
                        tags.Add(new TagAndValue("ExpirationDate", earlyRenewed.ExpirationDate.ToShortDateString()));
                    }

                    EmailTemplate template = EmailTemplate.Get(9);
                    email.TemplateID = template.ID;
                    //email.Subject = template.Subject;
                    email.Subject = template.ProcessTemplate(tagsSubj, template.Subject);
                    email.Body = template.ProcessTemplate(tags);

                    if (template != null && SpamPreventionCheck(loginID, template.ID, 1))
                    {
                        email.Save(0);
                        EmailSender.SendEmail(email);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "SendContractEarlyRenewDone");
            }
        }

        public static void SendPasswordResetLink(string emailAddress, string link)
        {
            List<TagAndValue> tags = new List<TagAndValue>();
            tags.Add(new TagAndValue("Link", link));

            Email email = new Email();
            email.AddToAddress(emailAddress);

            EmailTemplate template = EmailTemplate.Get(6);
            email.TemplateID = template.ID;
            email.Subject = template.Subject;
            email.Body = template.ProcessTemplate(tags);

            email.Save(0);
            EmailSender.SendEmail(email);
        }

        public static void SendForgotPasswordEmail(string emailAddress, SiteUser siteUser)
        {
            Contact contact = Contact.GetContactForLogins(siteUser.LoginID);

            List<TagAndValue> tags = new List<TagAndValue>();
            tags.Add(new TagAndValue("Email", emailAddress));
            tags.Add(new TagAndValue("Password", siteUser.Password));
            tags.Add(new TagAndValue("NewLine", Environment.NewLine));
            tags.Add(new TagAndValue("FirstName", contact.FirstName));

            Email email = new Email();
            email.AddToAddress(emailAddress);

            EmailTemplate template = EmailTemplate.Get(6);
            email.TemplateID = template.ID;
            email.Subject = template.Subject;
            email.Body = template.ProcessTemplate(tags);

            email.Save(0);
            EmailSender.SendEmail(email);
        }

        public static void SendFirstAddOnPaymentEmail(int addOnID, int loginID, int invoiceID)
        {
            try
            {
                ContractAddOn addOn = ContractAddOn.Get(addOnID);

                if (addOn != null)
                {
                    LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                    SalesRep salesRep = SalesRep.Get(loginInfo.SalesRepID);
                    string addOnType = addOn.AddOnType.Type;
                    if (addOn.AddOnType.ID == 8)
                    {
                        int qty = 0;
                        List<ContractAddOnHistory> history = ContractAddOnHistory.GetForInvoice(invoiceID);
                        history.ForEach(o => { qty += o.EndQuantity - o.StartQuantity; });
                        addOnType += "<br />Quantity: " + qty.ToString();
                    }

                    List<TagAndValue> tags = new List<TagAndValue>();
                    tags.Add(new TagAndValue("AccountID", loginID.ToString()));
                    tags.Add(new TagAndValue("ContractID", addOn.ContractID.ToString()));
                    tags.Add(new TagAndValue("AddOnType", addOnType));
                    tags.Add(new TagAndValue("PaymentTerms", addOn.PriceLevel.ContractTerms.TermDescription));

                    Email email = new Email();
                    email.AddToAddress(salesRep.Email);
                    List<string> emailAddresses = SalesRepPermissionManager.GetSalesRepEmailAddressForPermissionTag("IsEmailSentNewAddOn");

                    foreach (string eachSalesRepStaffEmailSent in emailAddresses)
                    {
                        email.AddCCAddress(eachSalesRepStaffEmailSent);
                    }

                    EmailTemplate template = EmailTemplate.Get(7);
                    email.TemplateID = template.ID;
                    email.Subject = template.Subject;
                    email.Body = template.ProcessTemplate(tags);

                    email.Save(0);
                    EmailSender.SendEmail(email);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "SendFirstAddOnPaymentEmail");
            }
        }

        public static void SendAutoPayFailureEmail(Invoice invoice, int failureNumber)
        {
            try
            {
                LoginInfo loginInfo = LoginInfo.GetByID(invoice.LoginID);
                if (loginInfo != null)
                {
                    if (OptOutService.HasOptedOut(OptOutType.AutoPayFail, loginInfo.ID))
                    {
                        return;
                    }

                    Contact contact = Contact.GetContactForLogins(invoice.LoginID);

                    List<TagAndValue> tagsSubj = new List<TagAndValue>();
                    tagsSubj.Add(new TagAndValue("LoginId", invoice.LoginID.ToString()));

                    SalesRep salesRep = SalesRep.Get(loginInfo.SalesRepID);
                    string rootUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["BaseUrl"];

                    List<TagAndValue> tags = new List<TagAndValue>();
                    tags.Add(new TagAndValue("SalesRepFirstName", salesRep.FirstName));
                    tags.Add(new TagAndValue("PaymentAmount", invoice.InvoiceTotal.ToString("N")));
                    tags.Add(new TagAndValue("UpdateLink", rootUrl + "?link=" + Encryptor.Encrypt(invoice.LoginID + "::/settings/billing")));

                    IOptOutService optOutService = new OptOutService();
                    string optOutLink = optOutService.GetOptOutLink(OptOutType.AutoPayFail, invoice.LoginID);
                    tags.Add(new TagAndValue("OptOutLink", rootUrl + "OptOut/Index?link=" + optOutLink));

                    Email email = new Email();
                    email.LoginID = invoice.LoginID;
                    email.AddToAddress(contact.Email);

                    EmailTemplate template = EmailTemplate.GetByName("Auto Pay Failure " + failureNumber);

                    // To avoid logic bugs spamming the customer, only send these emails once per time frame
                    if (template != null && SpamPreventionCheck(invoice.LoginID, template.ID, 15))
                    {
                        email.TemplateID = template.ID;
                        email.Subject = template.ProcessTemplate(tagsSubj, template.Subject);
                        email.Body = template.ProcessTemplate(tags);

                        if (System.Web.Configuration.WebConfigurationManager.AppSettings["PaymentEmailAddress"] != null)
                        {
                            email.ReplyTo = System.Web.Configuration.WebConfigurationManager.AppSettings["PaymentEmailAddress"];
                        }
                        else
                        {
                            email.ReplyTo = "dcolangelo@web-est.com";
                        }
                        
                        email.Save(0);
                        EmailSender.SendEmail(email);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, invoice.LoginID, "SendAutoPayFailureEmail");
            }
        }

        /// <summary>
        /// Returns False if we've sent an email with the passed TemplateID to the passed LoginID recently and we don't want to do it again to avoid accidentally spamming
        /// </summary>
        private static bool SpamPreventionCheck(int loginID, int templateID, int days)
        {
            List<Email> emails = Email.GetByLoginAndTemplate(loginID, templateID);

            if (emails.Count > 0)
            {
                emails = emails.OrderByDescending(o => o.CreateStamp).ToList();

                DateTime lastEmailStamp = emails.First().CreateStamp;
                TimeSpan timeSinceLastSend = DateTime.Now - lastEmailStamp;

                if (timeSinceLastSend.TotalDays < days)
                {
                    return false;
                }
            }

            return true;
        }

        public static void SendAutoPaySuccessEmail(int loginID)
        {
            try
            {
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                if (loginInfo != null)
                {
                    Contact contact = Contact.GetContactForLogins(loginID);

                    List<TagAndValue> tagsSubj = new List<TagAndValue>();
                    tagsSubj.Add(new TagAndValue("LoginId", loginID.ToString()));

                    List<TagAndValue> tags = new List<TagAndValue>();
                    tags.Add(new TagAndValue("Company", loginInfo.Organization));
                    tags.Add(new TagAndValue("Name", contact.FirstName + " " + contact.LastName));
                    tags.Add(new TagAndValue("Email", contact.Email));

                    Email email = new Email();
                    email.LoginID = loginID;
                    email.AddToAddress(contact.Email);

                    EmailTemplate template = EmailTemplate.GetByName("Auto Pay Success");

                    // To avoid logic bugs spamming the customer, only send these emails once per time frame
                    if (SpamPreventionCheck(loginID, template.ID, 3))
                    {
                        email.TemplateID = template.ID;
                        email.Subject = template.ProcessTemplate(tagsSubj, template.Subject);
                        email.Body = template.ProcessTemplate(tags);

                        email.Save(0);
                        EmailSender.SendEmail(email);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "SendAutoPaySuccessEmail");
            }
        }

        public static void SendSelectAddOnEmail(int loginID, int contractID, string addOnType, string term)
        {
            try
            {              
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                SalesRep salesRep = SalesRep.Get(loginInfo.SalesRepID);

                List<TagAndValue> tagsSubj = new List<TagAndValue>();
                tagsSubj.Add(new TagAndValue("LoginId", loginID.ToString()));
                
                List<TagAndValue> tags = new List<TagAndValue>();
                tags.Add(new TagAndValue("LoginId", loginID.ToString()));
                tags.Add(new TagAndValue("ContractID", contractID.ToString()));
                tags.Add(new TagAndValue("AddOnType", addOnType));
                tags.Add(new TagAndValue("SalesRep", salesRep.FirstName + " " + salesRep.LastName));
                tags.Add(new TagAndValue("PaymentTerms", term));

                Email email = new Email();
                email.AddToAddress(salesRep.Email);
                List<string> emailAddresses = SalesRepPermissionManager.GetSalesRepEmailAddressForPermissionTag("IsEmailSentSelectAddOn");

                foreach (string eachSalesRepStaffEmailSent in emailAddresses)
                {
                    email.AddCCAddress(eachSalesRepStaffEmailSent);
                }

                EmailTemplate template = EmailTemplate.GetByName("Select Add On");
                email.TemplateID = template.ID;
                email.Subject = template.ProcessTemplate(tagsSubj, template.Subject);
                email.Body = template.ProcessTemplate(tags);

                email.Save(0);
                EmailSender.SendEmail(email);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "SendSelectAddOnEmail");
            }
        }
    }
}