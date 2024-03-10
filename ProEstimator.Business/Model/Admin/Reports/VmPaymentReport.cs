using System.Collections.Generic;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin.Reports
{
    public class VmPaymentReport : IModelMap<VmPaymentReport>
    {
        public VmPaymentReport()
        {
            CreditCardPayments = new List<VmCreditCardPayment>();
            CheckPayments = new List<VmCheckPayment>();
            AutoPayments = new List<VmAutoPayment>();
            StripePayments = new List<VmStripePayment>();
        }

        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public List<VmCreditCardPayment> CreditCardPayments { get; set; }
        public List<VmCheckPayment> CheckPayments { get; set; }
        public List<VmAutoPayment> AutoPayments { get; set; }
        public List<VmStripePayment> StripePayments { get; set; }

        public VmPaymentReport ToModel(System.Data.DataRow row)
        {
            var model = new VmPaymentReport();

            return model;
        }
    }
}