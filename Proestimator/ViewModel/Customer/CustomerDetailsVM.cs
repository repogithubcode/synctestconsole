using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.Customer
{
    public class CustomerDetailsVM
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public string Phone1 { get; set; }
        public string Extension1 { get; set; }
        public string Phone1Type { get; set; }

        public string Phone2 { get; set; }
        public string Extension2{ get; set; }
        public string Phone2Type { get; set; }

        public string Phone3 { get; set; }
        public string Extension3 { get; set; }
        public string Phone3Type { get; set; }

        public string BusinessName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string TimeZone { get; set; }
    }
}