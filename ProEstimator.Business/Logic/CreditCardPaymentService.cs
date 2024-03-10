using ProEstimatorData;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.Model.CreditCardPayment;
using ProEstimatorData.DataModel;
using Stripe;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.UI.WebControls;
using System;

namespace ProEstimator.Business.Logic
{
    public class CreditCardPaymentService : ICreditCardPaymentService
    {
        public CreditCardPaymentService()
        {
        }

        public static bool IsAuthorized(int loginID)
        {
            var creditCardPaymentAllowForLoginIDs = ConfigurationManager.AppSettings.Get("CreditCardPaymentAllowForLoginIDs").ToString();
            return !string.IsNullOrEmpty(creditCardPaymentAllowForLoginIDs) &&
                (creditCardPaymentAllowForLoginIDs == "*" ||
                $",{creditCardPaymentAllowForLoginIDs},".IndexOf($",{loginID},") >= 0);
        }

        public CreditCardPaymentMerchantInfo GetMerchantCreditCardPaymentInfo(int loginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CreditCardPayment_MerchantInfo_Get", parameters);
            if (tableResult.Success)
            {
                return new CreditCardPaymentMerchantInfo(tableResult.DataTable.Rows.Count > 0 ? tableResult.DataTable.Rows[0] : null);
            }

            return null;
        }


        public int InsertCreditCardPaymentSuccessInfo(CreditCardPaymentSuccessVM model)
        {
            var db = new DBAccess();
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("LoginID", model.LoginID),
                new SqlParameter("EstimateID", model.EstimateID),
                new SqlParameter("Status", model.Status),
                new SqlParameter("CustID", model.CustID),
                new SqlParameter("PaymentID", model.PaymentID),
                new SqlParameter("Response", model.Response),
                new SqlParameter("AuthCode", model.AuthCode),
                new SqlParameter("DeclineReason", model.DeclineReason),
                new SqlParameter("Fee", model.Fee),
                new SqlParameter("Invoice", model.Invoice),
                new SqlParameter("Account", model.Account),
                new SqlParameter("Amount", model.Amount),
                new SqlParameter("AmountIncludesFee", model.AmountIncludesFee),
                new SqlParameter("Total", model.Total),
                new SqlParameter("PaymentType", model.PaymentType),
                new SqlParameter("MethodHint", model.MethodHint),
                new SqlParameter("CardBrand", model.CardBrand),
                new SqlParameter("CardNumDisplay", model.CardNumDisplay),
                new SqlParameter("ReceiptToken", model.ReceiptToken),
                new SqlParameter("Call", model.Call),
                new SqlParameter("Nonce", model.Nonce),
                new SqlParameter("Hmac", model.Hmac),
                new SqlParameter("PaymentReferenceID", model.PaymentReferenceID),
                new SqlParameter("CardNum", model.CardNum),
                new SqlParameter("Email", model.Email),
                new SqlParameter("NameOnCard", model.NameOnCard),
                new SqlParameter("CardType", model.CardType),
                new SqlParameter("CreatedDate", DateTime.UtcNow)
            };

            return db.ExecuteWithIntReturn("CreditCardPayment_Insert", parameters)?.Value ?? 0;
        }

    }
}
