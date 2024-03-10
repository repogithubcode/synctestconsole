using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

using Proestimator.ViewModel;
using ProEstimator.Business.Logic;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.ViewModelMappers.Payments
{
    public class CustomerInvoiceMapper
    {
        public static void Fill(int loginID, CustomerInvoiceVM vm, HttpSessionStateBase session)
        {
            vm.LoginID = loginID;

            // Load all due invoices
            List<Invoice> invoices = ContractManager.GetInvoicesToBePaid(loginID);
            vm.LoadInvoices(invoices);

            // Load saved payment info
            try
            {
                StripeInfo stripeInfo = StripeInfo.GetForLogin(loginID);
                if (stripeInfo != null && !string.IsNullOrEmpty(stripeInfo.StripeCardID))
                {
                    vm.HasSavedPaymentInfo = true;
                    vm.CardExpiration = stripeInfo.CardExpiration;
                    vm.Last4 = stripeInfo.CardLast4;
                    vm.AutoPaySelected = stripeInfo.AutoPay;

                    if (stripeInfo.CardError)
                    {
                        vm.CardHasError = true;
                        vm.CardErrorMessage = stripeInfo.ErrorMessage;
                    }
                }
                else
                {
                    vm.HasSavedPaymentInfo = false;
                    vm.StripeKey = ConfigurationManager.AppSettings.Get("StripePublishableKey").ToString();
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "InvoiceController FIllCustomerInvoiceVM StripeInfo");
                //vm.Message += "Error getting payment info." + Environment.NewLine;
            }

            // If there is an in progress contract the user can pick a new contract
            Contract inProgressContract = Contract.GetInProgress(loginID);

            if (inProgressContract != null)
            {
                vm.UserCanChangeContract = true;
            }


            // If there is a promo code attached to the contract load that data
            if (inProgressContract != null && inProgressContract.PromoID > 0)
            {
                PromoCode promoCode = PromoCode.GetByID(inProgressContract.PromoID);
                if (promoCode != null)
                {
                    vm.PromoID = promoCode.ID;
                    vm.PromoAmount = (double)promoCode.PromoAmount;
                }
            }

            // See if auto pay must be on
            if (inProgressContract != null && inProgressContract.ContractPriceLevel.ContractTerms.ForceAutoPay)
            {
                vm.ForceAutoPay = true;
            }

            if (session["ErrorMessage"] != null)
            {
                vm.Message = session["ErrorMessage"].ToString();
                session["ErrorMessage"] = null;
            }
        }

    }
}