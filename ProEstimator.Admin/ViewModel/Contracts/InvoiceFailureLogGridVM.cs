using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class InvoiceFailureLogGridVM
    {
        public DateTime TimeStamp { get; set; }
        public string Note { get; set; }
        public string LastFour { get; set; }
        public string Expiration { get; set; }
        public string StripeCardID { get; set; }
    }
}