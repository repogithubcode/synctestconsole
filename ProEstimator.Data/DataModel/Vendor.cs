using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class Vendor : ProEstEntity, IIDSetter
    {
        public int ID { get; private set; }

        int IIDSetter.ID
        {
            set { ID = value; }
        }

        public int LoginsID { get; set; }
        public int CompanyIDCode { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public string WorkPhone { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string TimeZone { get; set; }
        public bool Universal { get; set; }
        public VendorType Type { get; set; }
        public string FaxNumber { get; set; }
        public string FileName { get; set; }
        public bool Deleted { get; set; }
        public string Extension { get; set; }

        public string FederalTaxID { get; set; }
        public string LicenseNumber { get; set; }
        public string BarNumber { get; set; }
        public string RegistrationNumber { get; set; }

        public bool IsSelected { get; set; }

        public Vendor() { }

    }
}
