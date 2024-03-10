using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.SendEstimate
{
    public class ContactVM
    {
        public string Contact { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }

        public ContactVM(string contact, string emailAddress, string phoneNumber)
        {
            Contact = contact;
            EmailAddress = emailAddress;
            PhoneNumber = phoneNumber;
        }
    }
}