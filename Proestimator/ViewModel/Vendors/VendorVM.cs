using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.Vendors
{
    public class VendorVM
    {
        public int ID { get; set; }
        public int LoginsID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string MobilePhone { get; set; }
        public string WorkPhone { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string FaxNumber { get; set; }
        public string Extension { get; set; }
        public string TimeZone { get; set; }
        public int Type { get; set; }

        public bool IsPublic { get; set; }

        public string FederalTaxID { get; set; }
        public string LicenseNumber { get; set; }
        public string BarNumber { get; set; }
        public string RegistrationNumber { get; set; }

    }
}