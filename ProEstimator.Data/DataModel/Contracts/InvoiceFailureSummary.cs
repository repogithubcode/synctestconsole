using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData.DataModel.Contracts
{
    public class InvoiceFailureSummary
    {

        public int InvoiceID { get; set; }
        public int LoginID { get; set; }
        public decimal InvoiceTotal { get; set; }
        public DateTime DueDate { get; set; }
        public string InvoiceNotes { get; set; }
        public string InvoiceSummary { get; set; }
        public DateTime LastFailStamp { get; set; }
        public string FailNote { get; set; }
        public string LastFour { get; set; }
        public string Expiration { get; set; }
        public string StripeCardID { get; set; }

    }
}