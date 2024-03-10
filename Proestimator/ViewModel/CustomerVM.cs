using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProEstimator.Business.ILogic;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class CustomerVM
    {
        public int UserID { get; set; }
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public ContactVM Contact { get; set; }
        public AddressVM Address { get; set; }

        public SelectList PhoneTypes { get; set; }

        public int SelectedExistingCustomer { get; set; }
        public List<ExistingCustomerVM> ExistingCustomers { get; set; }

        public int EstimateCount { get; set; }

        public bool EstimateIsLocked { get; set; }
        public CustomerVM()
        {
            Contact = new ContactVM();
            Address = new AddressVM();
            ExistingCustomers = new List<ExistingCustomerVM>();
            SelectedExistingCustomer = 0;

            List<PhoneType> phoneTypes = PhoneType.GetAll();
            phoneTypes.Insert(0, new PhoneType("", ""));
            PhoneTypes = new SelectList(phoneTypes, "Code", "ScreenDisplay");            
        }
    }

    public class ExistingCustomerVM
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }

    }
}