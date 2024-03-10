using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class AddressVM
    {
        public int ContactID { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string TimeZone { get; set; }

        public AddressVM() { }

        public AddressVM(ProEstimatorData.DataModel.Address address)
        {
            LoadFromAddress(address);
        }

        public void LoadFromAddress(ProEstimatorData.DataModel.Address address)
        {
            if (address != null)
            {
                ContactID = address.ContactID;
                Line1 = address.Line1;
                Line2 = address.Line2;
                City = address.City;
                State = address.State;
                Zip = address.Zip;
                TimeZone = address.TimeZone;
            }
        }

        public void CopyToAddress(ProEstimatorData.DataModel.Address address)
        {
            if (address != null)
            {
                address.ContactID = ContactID;
                address.Line1 = Line1;
                address.Line2 = Line2;
                address.City = City;
                address.State = State;
                address.Zip = Zip;
                address.TimeZone = TimeZone;
            }
        }

    }
}