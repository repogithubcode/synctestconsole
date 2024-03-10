using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProEstimator.Business.ILogic;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class CompanyProfileVM : ITrackable<CompanyProfileVM>
    {

        public int LoginID { get; set; }
        public string CompanyName { get; set; }
        public string LogoImagePath { get; set; }
        public bool IsLocked { get; set; }

        public string DisabledClass
        {
            get
            {
                return IsLocked ? "disabled" : "";
            }
        }

        public string FirstName {get;set;}
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PhoneTwo { get; set; }
        public string PhoneThree { get; set; }
        public string PhNumberType1 { get; set; }
        public string PhNumberType2 { get; set; }
        public string PhNumberType3 { get; set; }
        public string Fax { get; set; }
        public string HeaderContact { get; set; }

        public string BusinessName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public string CompanyType { get; set; }
        public SelectList CompanyTypes { get; set; }

        public string FederalTaxID { get; set; }
        public bool PrintFederalTaxID { get; set; }
        public bool UseTaxID { get; set; }

        public string LicenseNumber { get; set; }
        public bool PrintLicenseNumber { get; set; }

        public string BarNumber { get; set; }
        public bool PrintBarNumber { get; set; }

        public string RegistrationNumber { get; set; }
        public bool PrintRegistration { get; set; }

        public string ErrorMessage { get; set; }

        public SelectList PhoneTypes { get; set; }

        public SelectList States { get; set; }

        public CompanyProfileVM()
        {
            List<SelectListItem> selections = new List<SelectListItem>();
            selections.Add(new SelectListItem() { Text = "Repair Shop", Value = "Repair" });
            selections.Add(new SelectListItem() { Text = "Adjuster", Value = "Adjuster" });
            selections.Add(new SelectListItem() { Text = "Insurer", Value = "Insurer" });

            CompanyTypes = new SelectList(selections, "Value", "Text");

            PhoneTypes = new SelectList(PhoneType.GetAll(), "Code", "ScreenDisplay");
        }

        public string ToTrackable()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public string Serialized { get; set; }


        public CompanyProfileVM FromTrackable(string item)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<CompanyProfileVM>(item);
        }

        public bool CheckForNulls(CompanyProfileVM obj)
        {
            var result = false;
            if (!string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(obj.FirstName) &&
                !string.IsNullOrEmpty(LastName) && string.IsNullOrEmpty(obj.LastName) &&
                !string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(obj.Email))
            {
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }
    }
}