using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web.Mvc;

using Proestimator.DataAttributes;
using ProEstimatorData.Models;
using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel.SendEstimate
{
    public class SendEstimateVM
    {
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public bool HasAccountEmailAddress { get; set; }
        public bool AccountIsLocked { get; set; }

        public string DefaultSubject { get; set; }
        public int DefaultBodyTemplateID { get; set; }

        public List<EmailAddressVM> EmailAddresses { get; set; }
        public List<PhoneNumberVM> PhoneNumbers { get; set; }
        public List<TemplateVM> BodyTemplates { get; set; }

        public int SelectedEmailID { get; set; }

        public bool HasEMSContract { get; set; }
        public string EmsRedirectPage { get; set; }

        public bool IsFinancingMerchantApproved { get; set; }

        public ReportCreator ReportCreator { get; set; }

        public SendEstimateVM()
        {
            HasAccountEmailAddress = false;

            EmailAddresses = new List<EmailAddressVM>();
            PhoneNumbers = new List<PhoneNumberVM>();
            SelectedEmailID = 0;
        }

        public SendEstimateVM(int loginID, int estimateID, bool hasEMSContract, bool isTrial)
        {
            HasAccountEmailAddress = false;

            EmailAddresses = new List<EmailAddressVM>();
            PhoneNumbers = new List<PhoneNumberVM>();
            SelectedEmailID = 0;

            ReportCreator = new ReportCreator(loginID, estimateID, hasEMSContract, isTrial, true);
        }
    }

    public class EmailAddressVM
    {
        public string Contact { get; set; }
        public string EmailAddress { get; set; }
        public bool Bounce { get; set; }
        public bool Unsubscribe { get; set; }

        public EmailAddressVM() { }
        public EmailAddressVM(string contact, string emailAddress)
        {
            Contact = contact;
            EmailAddress = emailAddress;
            Bounce = SendGridInfo.GetBounce(emailAddress);
            Unsubscribe = ProEstimatorData.DataModel.Unsubscribe.GetUnsubscribe(emailAddress);
        }
    }

    public class PhoneNumberVM
    {
        public string Contact { get; set; }
        public string PhoneNumber { get; set; }

        public PhoneNumberVM() { }
        public PhoneNumberVM(string contact, string phoneNumber)
        {
            Contact = contact;
            PhoneNumber = phoneNumber;
        }
    }

    public class TemplateVM
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public TemplateVM() { }
        public TemplateVM(int id, string name, string description)
        {
            ID = id;
            Name = name;
            Description = description;
        }
    }

}