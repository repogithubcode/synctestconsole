using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel.Customer
{
    public class CustomerSearchVM
    {

        public int UserID { get; set; }
        public int EstimateID { get; set; }

        public string State { get; set; }

        public List<SelectListItem> States { get; set; }

        public SelectList PhoneTypes { get; set; }

        public bool ForCustomerSelection { get; set; }

        public bool ConversionComplete { get; set; }

        public string InvoiceReminder { get; set; }
        public string ContractReminder { get; set; }
        public bool ShowEarlyRenewal { get; set; }

        public CustomerSearchVM()
        {
            UserID = 0;
            EstimateID = 0;
            State = "";
            States = new List<SelectListItem>();
            ForCustomerSelection = false;

            List<PhoneType> phoneTypes = PhoneType.GetAll();
            phoneTypes.Insert(0, new PhoneType("", ""));
            PhoneTypes = new SelectList(phoneTypes, "Code", "ScreenDisplay");
        }
    }
}