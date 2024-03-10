using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;

namespace Proestimator.ViewModel.Customer
{
    public class CustomerSearchResultVM
    {
        public int CustomerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string BusinessName { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }

        public CustomerSearchResultVM()
        {

        }

        public CustomerSearchResultVM(ProEstimatorData.DataModel.Customer customer, List<string> searchWords)
        {
            CustomerID = customer.ID;
            FirstName = HilightSearch(customer.Contact.FirstName, searchWords);
            LastName = HilightSearch(customer.Contact.LastName, searchWords);
            PhoneNumber = HilightSearch(InputHelper.FormatPhone(customer.Contact.Phone), searchWords);
            EmailAddress = HilightSearch(customer.Contact.Email, searchWords);
            BusinessName = HilightSearch(customer.Contact.BusinessName, searchWords);
            Address = HilightSearch(customer.Address.Line1, searchWords);
            ZipCode = HilightSearch(customer.Address.Zip, searchWords);
        }

        private string HilightSearch(string input, List<string> searchWords)
        {
            string inputLower = input.ToLower();

            foreach (string word in searchWords)
            {
                int index = inputLower.IndexOf(word);
                if (index > -1)
                {
                    string pre = input.Substring(0, index);
                    string hilight = input.Substring(index, word.Length);
                    string post = input.Substring(index + word.Length, input.Length - (index + word.Length));

                    input = pre + "<span class='search-match'>" + hilight + "</span>" + post;
                    return input;
                }
            }

            return input;
        }
    }
}