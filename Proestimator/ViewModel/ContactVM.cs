using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData;

namespace Proestimator.ViewModel
{
    public class ContactVM
    {
        public int ContactID { get; set; }

        public string Email { get; set; }
        public string SecondaryEmail { get; set; }
        public string Fax { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Phone1Type { get; set; }
        public string Phone1 { get; set; }
        public string Extension1 { get; set; }

        public string Phone2Type { get; set; }
        public string Phone2 { get; set; }
        public string Extension2 { get; set; }

        public string Phone3Type { get; set; }
        public string Phone3 { get; set; }
        public string Extension3 { get; set; }

        public string BusinessName { get; set; }

        [AllowHtml]
        public string Notes { get; set; }
        public bool CustomerSaved { get; set; }

        public ContactVM() { }

        public ContactVM(ProEstimatorData.DataModel.Contact contact)
        {
            LoadFromContact(contact);
        }

        public void LoadFromContact(ProEstimatorData.DataModel.Contact contact)
        {
            if (contact != null)
            {
                ContactID = contact.ContactID;
                Email = contact.Email;
                SecondaryEmail = contact.SecondaryEmail;
                Fax = contact.Fax;
                FirstName = contact.FirstName;
                LastName = contact.LastName;

                Phone1Type = contact.PhoneNumberType1;
                Phone1 = contact.Phone;
                Extension1 = contact.Extension1;

                Phone2Type = contact.PhoneNumberType2;
                Phone2 = contact.Phone2;
                Extension2 = contact.Extension2;

                Phone3Type = contact.PhoneNumberType3;
                Phone3 = contact.Phone3;
                Extension3 = contact.Extension3;

                BusinessName = contact.BusinessName;
                Notes = contact.Notes;
                CustomerSaved = contact.CustomerSaved;
            }
        }

        public void CopyToContact(ProEstimatorData.DataModel.Contact contact)
        {
            if (contact != null)
            {
                contact.ContactID = ContactID;
                contact.Email = InputHelper.GetString(Email);
                contact.SecondaryEmail = InputHelper.GetString(SecondaryEmail);
                contact.Fax = InputHelper.GetString(Fax);
                contact.FirstName = InputHelper.GetString(FirstName);
                contact.LastName = InputHelper.GetString(LastName);

                contact.PhoneNumberType1 = InputHelper.GetString(Phone1Type);
                contact.Phone = InputHelper.GetString(Phone1);
                contact.Extension1 = InputHelper.GetString(Extension1);

                contact.PhoneNumberType2 = InputHelper.GetString(Phone2Type);
                contact.Phone2 = InputHelper.GetString(Phone2);
                contact.Extension2 = InputHelper.GetString(Extension2);

                contact.PhoneNumberType3 = InputHelper.GetString(Phone3Type);
                contact.Phone3 = InputHelper.GetString(Phone3);
                contact.Extension3 = InputHelper.GetString(Extension3);
                
                contact.BusinessName = InputHelper.GetString(BusinessName);
                contact.Notes = InputHelper.GetString(Notes);
                contact.CustomerSaved = CustomerSaved;
            }
        }
    }
}