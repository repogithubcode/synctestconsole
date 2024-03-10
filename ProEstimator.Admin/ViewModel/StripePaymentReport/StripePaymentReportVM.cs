using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;
using ProEstimatorData;
using ProEstimator.Business.Model.Admin;
using ProEstimator.Business.Model.Admin.Reports;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel.Payment
{
    public class StripePaymentReportVM
    {
        public int LoginID { get; set; }
        public string LoginIDString { get; set; }
        public string Organization { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }

        public int PaymentNumber { get; set; }

        public string TermDescription { get; set; }

        public StripePaymentReportVM() { }

        public StripePaymentReportVM(StripePayment stripePayment)
        {
            LoginID = InputHelper.GetInteger(stripePayment.LoginID.ToString());
            Organization = InputHelper.GetString(stripePayment.Organization.ToString());
            Address = InputHelper.GetString(stripePayment.Address.ToString());
            City = InputHelper.GetString(stripePayment.City.ToString());
            State = InputHelper.GetString(stripePayment.State.ToString());
            Zip = InputHelper.GetString(stripePayment.Zip.ToString());
            Date = InputHelper.GetDateTime(stripePayment.Date.ToString());
            Amount = InputHelper.GetDecimal(stripePayment.Amount);
            PaymentNumber = InputHelper.GetInteger(stripePayment.PaymentNumber.ToString());
            TermDescription = InputHelper.GetString(stripePayment.TermDescription);

            LoginIDString = LoginID.ToString();
            PaymentNumber = InputHelper.GetInteger(stripePayment.PaymentNumber.ToString());
            TermDescription = InputHelper.GetString(stripePayment.TermDescription);
        }
    }
}