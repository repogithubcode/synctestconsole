using System;
using System.Web;

namespace Proestimator.ViewModel.Customer
{
    public class CustomerSelectVM
    {
        public int CustomerID { get; set; }
        public bool Selected { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public CustomerSelectVM(int id, string fName, string lName)
        {
            CustomerID = id;
            Selected = false;
            FirstName = fName;
            LastName = lName;
        }

        public CustomerSelectVM(ProEstimatorData.DataModel.Customer customer)
        {
            CustomerID = customer.ID;
            Selected = false;
            FirstName = customer.Contact.FirstName;
            LastName = customer.Contact.LastName;
        }
    }
}
