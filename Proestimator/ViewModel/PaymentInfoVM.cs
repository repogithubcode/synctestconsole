using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class PaymentInfoVM
    {
        public int UserID { get; set; }
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public List<PaymentInfoSummaryVM> PaymentList { get; set; }

        public int PaymentId { get; set; }
        public int ContactsID { get; set; }
        public string PayeeName { get; set; }
        public string PaymentType { get; set; }
        public string PaymentDate { get; set; }
        public string CheckNumber { get; set; }
        public string Amount { get; set; }
        public string Memo { get; set; }
        public string WhoPays { get; set; }
        public int CreditCardPaymentID { get; set; }

        public SelectList WhoPaysList { get; set; }
        public SelectList PaymentTypeList { get; set; }

        public List<KeyValuePair<string, string>> PayeePresets { get; set; }

        public string SaveMessage { get; set; }

        public string TotalPaid { get; set; }
        public string EstimateTotal { get; set; }
        public string TotalRemaining { get; set; }

        public string CustomerEmailAddress { get; set; }

        public bool EstimateIsLocked { get; set; }

        public string CreditCardPaymentLightBoxTerminalApi { get; set; }
        public string CreditCardPaymentUriParams { get; set; }
        public bool CreditCardPaymentShow => !string.IsNullOrEmpty(CreditCardPaymentLightBoxTerminalApi) && !string.IsNullOrEmpty(CreditCardPaymentUriParams);

        public PaymentInfoVM()
        {
            PaymentList = new List<PaymentInfoSummaryVM>();

            List<SelectListItem> whoPaysList = new List<SelectListItem>();
            whoPaysList.Add(new SelectListItem() { Text = "--Select--", Value = "" });
            whoPaysList.Add(new SelectListItem() { Text = "Insured", Value = "I" });
            whoPaysList.Add(new SelectListItem() { Text = "Customer", Value = "CP" });
            whoPaysList.Add(new SelectListItem() { Text = "Claimant", Value = "C" });
            whoPaysList.Add(new SelectListItem() { Text = "Fleet / Self-insured Company", Value = "F" });
            whoPaysList.Add(new SelectListItem() { Text = "Internal (shop)", Value = "IN" });
            whoPaysList.Add(new SelectListItem() { Text = "Warranty", Value = "W" });
            whoPaysList.Add(new SelectListItem() { Text = "Other", Value = "O" });
            whoPaysList.Add(new SelectListItem() { Text = "Insurance Company", Value = "IC" });
            WhoPaysList = new SelectList(whoPaysList, "Value", "Text");

            List<SelectListItem> paymentTypeList = new List<SelectListItem>();
            paymentTypeList.Add(new SelectListItem() { Text = "--Select--", Value = "" });
            paymentTypeList.Add(new SelectListItem() { Text = "Cash", Value = "CS" });
            paymentTypeList.Add(new SelectListItem() { Text = "Check", Value = "CK" });
            paymentTypeList.Add(new SelectListItem() { Text = "Credit Card", Value = "CC" });
            paymentTypeList.Add(new SelectListItem() { Text = "Debit Card", Value = "DC" });
            paymentTypeList.Add(new SelectListItem() { Text = "Other", Value = "OT" });
            PaymentTypeList = new SelectList(paymentTypeList, "Value", "Text");

            PayeePresets = new List<KeyValuePair<string, string>>();
        }
    }

    public class PaymentInfoSummaryVM
    {
        public int PaymentId { get; set; }
        public string PayeeName { get; set; }
        public string PaymentDate { get; set; }
        public string Amount { get; set; }
        public string Memo { get; set; }
        public int CreditCardPaymentID { get; set; }
    }
}