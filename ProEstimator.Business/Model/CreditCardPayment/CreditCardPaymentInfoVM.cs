using ProEstimatorData;
using System;
using System.Data;

namespace ProEstimator.Business.Model.CreditCardPayment
{
    // Data returned from our databse, in table CreditCardPayment, and displayed on the Intellipay approved page
    public class CreditCardPaymentInfoVM
    {
        public int CreditCardPaymentID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CustomerName { get; set; }
        public int LoginID { get; set; }
        public int EstimateID { get; set; }
        public int Status { get; set; }
        public int CustID { get; set; }
        public int PaymentID { get; set; }
        public string Response { get; set; }
        public string AuthCode { get; set; }
        public string DeclineReason { get; set; }
        public double Fee { get; set; }
        public int Invoice { get; set; }
        public string Account { get; set; }
        public double Amount { get; set; }
        public bool AmountIncludesFee { get; set; }
        public double Total { get; set; }
        public string PaymentType { get; set; }
        public string MethodHint { get; set; }
        public string CardBrand { get; set; }
        public string CardNumDisplay { get; set; }
        public string ReceiptToken { get; set; }
        public string Call { get; set; }
        public string Nonce { get; set; }
        public string Hmac { get; set; }
        public string PaymentReferenceID { get; set; }
        public string CardNum { get; set; }
        public string Email { get; set; }
        public string NameOnCard { get; set; }
        public string CardType { get; set; }

        public CreditCardPaymentInfoVM()
        {
        }

        public CreditCardPaymentInfoVM(DataRow row)
        {
            CreditCardPaymentID = InputHelper.GetInteger(row["CreditCardPaymentID"].ToString());
            CustomerName = row["CustomerName"].ToString();
            if (!string.IsNullOrEmpty(row["CreatedDate"]?.ToString()))
                CreatedDate = InputHelper.GetDateTime(row["CreatedDate"].ToString());
            EstimateID = InputHelper.GetInteger(row["EstimateID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            Status = InputHelper.GetInteger(row["Status"].ToString());
            CustID = InputHelper.GetInteger(row["CustID"].ToString());
            PaymentID = InputHelper.GetInteger(row["PaymentID"].ToString());
            Response = row["Response"].ToString();
            AuthCode = row["AuthCode"].ToString();
            DeclineReason = row["DeclineReason"].ToString();
            Fee = InputHelper.GetDouble(row["Fee"].ToString());
            Invoice = InputHelper.GetInteger(row["Invoice"].ToString());
            Account = row["Account"].ToString();
            Amount = InputHelper.GetDouble(row["Amount"].ToString());
            AmountIncludesFee = InputHelper.GetBoolean(row["AmountIncludesFee"].ToString());
            Total = InputHelper.GetDouble(row["Total"].ToString());
            PaymentType = row["PaymentType"].ToString();
            MethodHint = row["MethodHint"].ToString();
            CardBrand = row["CardBrand"].ToString();
            CardNumDisplay = row["CardNumDisplay"].ToString();
            ReceiptToken = row["ReceiptToken"].ToString();
            Call = row["Call"].ToString();
            Nonce = row["Nonce"].ToString();
            Hmac = row["Hmac"].ToString();
            PaymentReferenceID = row["PaymentReferenceID"].ToString();
            CardNum = row["CardNum"].ToString();
            Email = row["Email"].ToString();
            NameOnCard = row["NameOnCard"].ToString();
            CardType = row["CardType"].ToString();
        }

    }
}