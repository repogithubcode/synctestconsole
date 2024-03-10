using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

//using Stripe;

using ProEstimatorData;
using ProEstimatorData.Models;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using Proestimator.DocuSignWeb;

using Proestimator.ViewModel;
using Proestimator.ViewModel.Contracts;

using ProEstimator.Business.Logic;
using ProEstimator.Business.Payments;
using ProEstimator.Business.Model.Account.Commands;
using ProEstimator.Business.Payments.StripeCommands;
using ProEstimator.Business.Payments.InvoiceCommands;
using Proestimator.ViewModelMappers.Payments;
using Proestimator.ViewModelMappers.Contracts;
using System.Web.Http.Tracing;
using System.Web.UI.WebControls;

namespace Proestimator.Controllers
{
    public class InvoiceController : SiteController
    {
        private IPaymentService _paymentService;
        private IStripeService _stripeService;

        public InvoiceController(IPaymentService paymentService, IStripeService stripeService)
        {
            _paymentService = paymentService;
            _stripeService = stripeService;
        }

        #region Pick Contract

        [HttpGet]
        [Route("{userID}/invoice/pick-contract")]
        public ActionResult PickContract(int userID)
        {
            PickContractVMMapper mapper = new PickContractVMMapper();
            PickContractVM vm = mapper.Map(new PickContractVMMapperConfiguration() { LoginID = ActiveLogin.LoginID });

            return View(vm);
        }

        [HttpPost]
        [Route("{userID}/invoice/pick-contract")]
        public ActionResult PickContract(int userID, PickContractVM model)
        {
            // If there is an in progress contract, delete it
            Contract inProgressContract = Contract.GetInProgress(ActiveLogin.LoginID);
            if (inProgressContract != null)
            {
                ContractManager.DeleteUncommitted(inProgressContract, ActiveLogin.LoginID, false);
                inProgressContract = null;
            }

            ContractFunctionResult result = ContractManager.CreateContract(ActiveLogin.LoginID, model.SelectedPaymentID, "", ViewBag.AddOnsPermit);

            if (result.Success)
            {
                // Apply the early renewal promo code if needed
                if (ContractManager.IsInEarlyRenewalPeriod(ActiveLogin.LoginID, ContractManager.MaxRenewalWindow))
                {
                    try
                    {
                        result.Contract.EarlyRenewal = true;
                        result.Contract.Save();
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, ActiveLogin.LoginID, "PickContract SetEarlyRenewalDue");
                    }

                    try
                    {
                        int promoCodeID = InputHelper.GetInteger(ConfigurationManager.AppSettings["EarlyRenewalPromoCode"]);
                        PromoCode promoCode = PromoCode.GetByID(promoCodeID);
                        ContractManager.ApplyPromo(result.Contract, promoCode, ActiveLogin.ID);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, ActiveLogin.LoginID, "PickContract ApplyEarlyRenewal");
                    }
                }

                // If there was already a contract, re-attach any add ons to the new contract
                if (inProgressContract != null)
                {
                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(inProgressContract.ID);
                    foreach (ContractAddOn addOn in addOns)
                    {
                        addOn.ContractID = result.Contract.ID;
                        addOn.Save();

                        List<Invoice> invoices = Invoice.GetForContractAddOn(addOn.ID);
                        foreach (Invoice invoice in invoices)
                        {
                            invoice.ContractID = result.Contract.ID;
                            invoice.Save();
                        }
                    }

                    if (addOns.Count > 0)
                    {
                        return Redirect("/" + userID + "/invoice/subscription-confirm/" + result.Contract.ID);
                    }
                }

                return Redirect("/" + userID + "/invoice/pick-addon/" + result.Contract.ID);
            }
            else
            {
                model.ErrorMessage = result.ErrorMessage;
                return View(model);
            }
        }

        public ActionResult GetMainContractOptions([DataSourceRequest] DataSourceRequest request, int userID, int loginID)
        {
            List<ContractTermsVM> contractList = new List<ContractTermsVM>();

            if (IsUserAuthorized(userID))
            {
                try
                {
                    int priceLevel = 0;

                    ProEstimatorData.DataModel.Contracts.Contract contract = ContractManager.GetLatestContract(loginID);

                    // Figure out the price level of the last contract, or default to 0
                    if (contract != null)
                    {
                        if (contract.ExpirationDate > DateTime.Now.AddDays(-90))
                        {
                            priceLevel = contract.ContractPriceLevel.PriceLevel;

                            if (priceLevel < 0)
                            {
                                priceLevel = 0;
                            }
                        }
                    }

                    if (UseNewCustomerPrice(loginID))
                    {
                        priceLevel = 8;
                    }

                    contractList = GetContractTersmDetailsForContractType(0, 1, 1, priceLevel);
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, loginID, "GetMainContractOptions");
                }
            }

            return Json(contractList.ToDataSourceResult(request));
        }

        private bool UseNewCustomerPrice(int loginID)
        {
            bool newCustomer = false;

            List<Invoice> invoices = Invoice.GetForLogin(loginID);
            invoices = invoices.Where(o => o.Paid).ToList();
            if (invoices.Count == 0)
            {
                newCustomer = true;
            }

            Contract latestContract = ContractManager.GetLatestContract(loginID);
            if (latestContract != null && latestContract.ExpirationDate < DateTime.Now.AddDays(-90))
            {
                newCustomer = true;
            }

            Trial activeTrial = Trial.GetActive(loginID);
            DateTime cutoffDate = new DateTime(2024, 2, 11);
            if (activeTrial != null && activeTrial.StartDate <= cutoffDate)
            {
                newCustomer = false;
            }

            return newCustomer;
        }

        /// <summary>
        /// For all contract terms records, return a VM with the terms and the price for the passed price level.
        /// If there is no price for the passed price level, don't add the contract terms.
        /// If the price level > 0 (0 is the default), we will show inactive contract terms records.  The price levels are used for old customers.
        /// </summary>
        private List<ContractTermsVM> GetContractTersmDetailsForContractType(int contractID, int contractType, int qty, int priceLevel = 0)
        {
            List<ContractTermsVM> result = new List<ContractTermsVM>();

            try
            {
                List<ContractTerms> allContractTerms = ContractManager.GetContractTermsForAddOn(contractID, contractType);
                foreach (ContractTerms contractTerms in allContractTerms)
                {
                    ContractPriceLevel contractPriceLevel = ContractPriceLevel.GetAll().FirstOrDefault(o => o.Active && (o.PriceLevel == priceLevel || contractType != 1) && o.ContractTerms.ID == contractTerms.ID);

                    if (contractPriceLevel != null)
                    {
                        ContractTermsVM vm = new ContractTermsVM();
                        vm.ContractPriceLevelID = contractPriceLevel.ID;
                        vm.DepositAmount = qty * contractTerms.DepositAmount;
                        vm.ForceAutoPay = contractTerms.ForceAutoPay;
                        vm.NumberOfPayments = contractTerms.NumberOfPayments;
                        vm.PaymentDescription = (contractTerms.DepositAmount > 0 ? "Deposit of " + contractTerms.DepositAmount.ToString("C") + " then " : "") + contractTerms.NumberOfPayments.ToString() + " payment" + (contractTerms.NumberOfPayments > 0 ? "s" : "") + " of " + contractPriceLevel.PaymentAmount.ToString("C");
                        vm.TermDescription = contractTerms.TermDescription;
                        vm.PaymentAmount = contractPriceLevel.PaymentAmount;
                        vm.TermTotal = qty * contractTerms.DepositAmount + (qty * contractTerms.NumberOfPayments * contractPriceLevel.PaymentAmount);
                        string qtyString = "";
                        if (qty > 1)
                        {
                            qtyString = qty.ToString() + " x ";
                        }

                        vm.Summary = (contractTerms.DepositAmount > 0 ? qtyString + contractTerms.DepositAmount.ToString("C") + " + " : "") + qtyString + contractTerms.NumberOfPayments.ToString() + " x " + contractPriceLevel.PaymentAmount.ToString("C");

                        result.Add(vm);
                    }
                }

                // If we didn't find any results for the passed price level, try again with the default price level.
                if (priceLevel > 0 && result.Count == 0)
                {
                    result = GetContractTersmDetailsForContractType(contractID, contractType, qty, 0);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "GetContractTersmDetailsForContractType");
            }

            return result;
        }

        #endregion

        #region Pick Addons

        [HttpGet]
        [Route("{userID}/invoice/pick-addon/{contractID}")]
        public ActionResult AddOnContract(int userID, int? contractID = 0)
        {
            PickAddOnsVM vm = MakePickAddOnsVM(contractID);
            return View(vm);
        }

        private PickAddOnsVM MakePickAddOnsVM(int? contractID)
        {
            new PickAddOnsVM();

            Contract contract;
            if (contractID.HasValue)
            {
                contract = Contract.Get(contractID.Value);
            }
            else
            {
                contract = Contract.GetActive(ActiveLogin.LoginID);
            }

            if (contract == null || contract.LoginID != ActiveLogin.LoginID)
            {
                return new PickAddOnsVM() { LoginID = ActiveLogin.ID };
            }
            else
            {
                PickAddOnsVMMapper mapper = new PickAddOnsVMMapper();
                PickAddOnsVM vm = mapper.Map(new PickAddOnsVMMapperConfiguration() { ContractID = contract.ID });

                vm.ContractMessage = string.Format(Proestimator.Resources.ProStrings.PickContractRunsBetween, contract.EffectiveDate.ToShortDateString(), contract.ExpirationDate.ToShortDateString());
                vm.LoginID = ActiveLogin.LoginID;

                return vm;
            }
        }

        [HttpPost]
        [Route("{userID}/invoice/pick-addon/{contractID}")]
        public ActionResult AddOnContract(int userID, int contractID, PickAddOnsVM vm)
        {
            StringBuilder errorBuilder = new StringBuilder();
            bool fail = true;

            // If there is an in progress contract use that, othrewise use the active contract
            Contract contract = Contract.Get(vm.ContractID);

            if (contract == null)
            {
                errorBuilder.AppendLine("Contract " + vm.ContractID + " not found.");
            }
            else
            {
                List<ContractAddOn> addOns = ContractAddOn.GetForContract(contractID);
                Contract nContract = Contract.Get(contractID);
                Contract activeContract = Contract.GetActive(nContract.LoginID);
                bool isDelete = activeContract == null || nContract.ID != activeContract.ID;

                foreach (AddOnDetailsVM detailVM in vm.AddOnDetails)
                {
                    if (detailVM.IsMultiAdd)
                    {
                        bool success = true;
                        if (isDelete)
                        {
                            List<ContractAddOn> existingAddOns = addOns.Where(o => o.AddOnType.ID == detailVM.AddOnTypeID).ToList();
                            foreach (ContractAddOn existingAddOn in existingAddOns)
                            {
                                FunctionResult resultDelete = existingAddOn.DeletePermanent();
                                if (!resultDelete.Success)
                                {
                                    success = false;
                                    errorBuilder.AppendLine(resultDelete.ErrorMessage);
                                    if (detailVM.SelectedID > 0)
                                    {
                                        string type = ContractType.Get(detailVM.AddOnTypeID).Type;
                                        string term = ContractPriceLevel.Get(detailVM.SelectedID).ContractTerms.TermDescription;
                                        errorBuilder.AppendLine("Failed to create add on " + type + " with " + term);
                                    }
                                }
                                else
                                {
                                    fail = false;
                                }
                            }
                        }
                        if (success && detailVM.SelectedID > 0 && detailVM.SelectedAddOnQty > 0)
                        {
                            FunctionResult result = ContractManager.CreateContractAddOn(contract, detailVM.SelectedID, detailVM.AddOnTypeID, DateTime.Now, detailVM.SelectedAddOnQty);
                            if (!result.Success)
                            {
                                errorBuilder.AppendLine(result.ErrorMessage);
                            }
                            else
                            {
                                fail = false;
                            }
                        }
                    }
                    else
                    {
                        ContractAddOn existingAddOn = addOns.FirstOrDefault(o => o.AddOnType == ContractType.Get(detailVM.AddOnTypeID));

                        if (detailVM.SelectedID > 0)
                        {
                            if (existingAddOn == null)
                            {
                                FunctionResult result = ContractManager.CreateContractAddOn(contract, detailVM.SelectedID, detailVM.AddOnTypeID, DateTime.Now);
                                if (!result.Success)
                                {
                                    errorBuilder.AppendLine(result.ErrorMessage);
                                }
                                else
                                {
                                    fail = false;
                                }
                            }
                            else if (existingAddOn.PriceLevel != ContractPriceLevel.Get(detailVM.SelectedID))
                            {
                                FunctionResult resultDelete = existingAddOn.DeletePermanent();
                                if (resultDelete.Success)
                                {
                                    FunctionResult resultAddOn = ContractManager.CreateContractAddOn(contract, detailVM.SelectedID, detailVM.AddOnTypeID, DateTime.Now);
                                    if (!resultAddOn.Success)
                                    {
                                        errorBuilder.AppendLine(resultAddOn.ErrorMessage);
                                    }
                                    fail = false;
                                }
                                else
                                {
                                    string type = ContractType.Get(detailVM.AddOnTypeID).Type;
                                    string term = ContractPriceLevel.Get(detailVM.SelectedID).ContractTerms.TermDescription;
                                    errorBuilder.AppendLine(resultDelete.ErrorMessage);
                                    errorBuilder.AppendLine("Failed to create add on " + type + " with " + term);
                                }
                            }
                        }
                        else
                        {
                            if (existingAddOn != null)
                            {
                                FunctionResult result = existingAddOn.DeletePermanent();
                                if (!result.Success)
                                {
                                    errorBuilder.AppendLine(result.ErrorMessage);
                                    errorBuilder.AppendLine("Failed to delete add on " + existingAddOn.AddOnType.Type + " with " + existingAddOn.PriceLevel.ContractTerms.TermDescription);
                                }
                                else
                                {
                                    fail = false;
                                }
                            }
                        }
                    }
                }
            }

            string errors = errorBuilder.ToString();
            if (!string.IsNullOrEmpty(errors) && fail)
            {
                vm = MakePickAddOnsVM(contractID);
                vm.Errors = errors;
                ModelState.Clear();
                return View(vm);
            }
            else
            {
                return Redirect("/" + userID + "/invoice/subscription-confirm/" + vm.ContractID);
            }
        }

        public ActionResult GetAddOnContractOptions(
              [DataSourceRequest] DataSourceRequest request
            , int userID
            , int loginID
            , int contractID
            , int contractTypeID
            , int qty = 1
        )
        {
            List<ContractTermsVM> contractList = new List<ContractTermsVM>();

            if (IsUserAuthorized(userID))
            {
                try
                {
                    contractList = GetContractTersmDetailsForContractType(contractID, contractTypeID, qty);
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, loginID, "GetAddOnContractOptions");
                }
            }

            return Json(contractList.ToDataSourceResult(request));
        }


        #endregion

        #region Subscription Confirm

        [HttpGet]
        [Route("{userID}/invoice/subscription-confirm/{contractID}")]
        public ActionResult SubscriptionConfirm(int userID, int contractID)
        {
            Contract contract = Contract.Get(contractID);

            if (contract == null || contract.LoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID + "/invoice/pick-contract");
            }
            else
            {
                SubscriptionConfirmVM vm = new SubscriptionConfirmVM();
                vm.UserID = userID;
                vm.ErrorMessage = Session["ErrorMessage"] != null ? Session["ErrorMessage"].ToString() : "";
                Session["ErrorMessage"] = null;
                FillSubscriptionConfirmVM(vm, contract);

                return View(vm);
            }
        }

        [HttpPost]
        [Route("{userID}/invoice/add-promo")]
        public ActionResult SubmitCode(SubscriptionConfirmVM vm)
        {
            Contract contract = Contract.Get(vm.ContractID);
            if (contract != null)
            {
                FunctionResult result = ContractManager.ApplyPromo(contract, vm.PromoCode, ActiveLogin.ID);
                if (!result.Success)
                {
                    Session["ErrorMessage"] = result.ErrorMessage;
                }
            }
            else
            {
                Session["ErrorMessage"] = Proestimator.Resources.ProStrings.NoInProgressContractFound;
            }

            return Redirect("/" + vm.UserID + "/invoice/subscription-confirm/" + contract.ID);
        }

        [HttpPost]
        [Route("{userID}/invoice/remove-promo")]
        public ActionResult RemovePromo(SubscriptionConfirmVM vm)
        {
            Contract contract = Contract.Get(vm.ContractID);

            if (contract != null)
            {
                FunctionResult result = ContractManager.RemovePromo(contract, ActiveLogin.ID);
                if (!result.Success)
                {
                    Session["ErrorMessage"] = result.ErrorMessage;
                }
            }
            else
            {
                Session["ErrorMessage"] = Proestimator.Resources.ProStrings.NoInProgressContractFound;
            }

            return Redirect("/" + vm.UserID + "/invoice/subscription-confirm/" + contract.ID);
        }

        [HttpPost]
        [Route("{userID}/invoice/delete-contract")]
        public ActionResult DeleteContract(SubscriptionConfirmVM vm)
        {
            Contract inProgressContract = Contract.GetInProgress(vm.LoginID);
            if (inProgressContract != null)
            {
                FunctionResult result = ContractManager.DeleteUncommitted(inProgressContract, vm.LoginID, true);
                if (!result.Success)
                {
                    Session["ErrorMessage"] = string.Format(Proestimator.Resources.ProStrings.ErrorDeletingContract, result.ErrorMessage);
                    return Redirect("/" + vm.UserID + "/invoice/subscription-confirm/" + inProgressContract.ID);
                }
                else
                {
                    return Redirect("/" + vm.UserID);
                }
            }
            else
            {
                Session["ErrorMessage"] = Proestimator.Resources.ProStrings.NoInProgressContractFound;
                return Redirect("/" + vm.UserID + "/invoice/subscription-confirm/" + inProgressContract.ID);
            }
        }

        private void FillSubscriptionConfirmVM(SubscriptionConfirmVM vm, Contract contract)
        {
            // Get details about the main contract.
            if (!contract.HasPayment)
            {
                vm.HasMainContract = true;
                vm.TermDescription = contract.ContractPriceLevel.ContractTerms.TermDescription;
                vm.StartDate = contract.EffectiveDate.Date;
                vm.ExpirationDate = contract.ExpirationDate.Date;

                InvoiceVMMapper invoiceMapper = new InvoiceVMMapper();

                List<Invoice> invoices = Invoice.GetForContract(contract.ID);
                foreach (Invoice invoice in invoices)
                {
                    vm.EstimaticsInvoices.Add(invoiceMapper.Map(new InvoiceVMMapperConfiguration() { Invoice = invoice }));
                }

                // Get details about Promo
                if (contract.PromoID > 0)
                {
                    vm.HasPromo = true;

                    PromoCode promo = PromoCode.GetByID(contract.PromoID);
                    if (promo != null)
                    {
                        vm.CurrentPromoCode = promo.Code;
                        vm.CurrentPromoAmount = promo.PromoAmount;
                        int promoCodeID = InputHelper.GetInteger(ConfigurationManager.AppSettings["EarlyRenewalPromoCode"]);
                        vm.IsEarlyRenewalPromoCodeApplied = promoCodeID == contract.PromoID ? true : false;
                    }
                }
            }

            // Get details about add ons
            List<ContractAddOn> addOns = ContractAddOn.GetForContract(contract.ID);

            bool hasFreeAddOn = false;

            AddOnConfirmVMMapper mapper = new AddOnConfirmVMMapper();

            foreach (ContractAddOn addOn in addOns)
            {
                if (!addOn.HasPayment || addOn.PriceLevel.PaymentAmount == 0 || (addOn.AddOnType.ID == 8 && ContractManager.InvoiceDeletable(addOn.ID).Count > 0))
                {
                    vm.AddOns.Add(mapper.Map(new AddOnConfirmVMMapperConfiguration() { AddOn = addOn }));
                }

                if (addOn.PriceLevel.PaymentAmount == 0)
                {
                    hasFreeAddOn = true;
                }
            }

            if (hasFreeAddOn)
            {
                _siteLoginManager.RefreshInvoiceInformation(vm.UserID, GetComputerKey());
            }

            vm.LoginID = contract.LoginID;
            vm.ContractID = contract.ID;
        }

        public JsonResult RemoveAddOn(int addOnID, int userID)
        {
            CacheActiveLoginID(userID);

            string errorMessage = "";

            if (IsUserAuthorized(userID))
            {
                ContractAddOn addOn = ContractAddOn.Get(addOnID);
                if (addOn != null)
                {
                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                    FunctionResult deleteResult = ContractManager.RemoveContractAddOn(activeLogin.LoginID, addOn.ID, addOn.Quantity, activeLogin.ID);
                    if (!deleteResult.Success)
                    {
                        errorMessage = deleteResult.ErrorMessage;
                    }
                }
            }
            else
            {
                errorMessage = "Invalid login";
            }

            return Json(errorMessage, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Customer Invoice

        [HttpGet]
        [Route("{userID}/invoice/customer-invoice")]
        public ActionResult CustomerInvoice(int userID)
        {
            CustomerInvoiceVM vm = new CustomerInvoiceVM();
            CustomerInvoiceMapper.Fill(ActiveLogin.LoginID, vm, Session);
            return View(vm);
        }

        [HttpPost]
        [Route("{userID}/invoice/customer-invoice")]
        public ActionResult CustomerInvoice(CustomerInvoiceVM vm)
        {
            try
            {
                // Check if the Stripe form posted this
                string token = Request.Form["stripeToken"];
                string emailAddress = Request.Form["stripeEmail"];

                // If a stripe token was created, save credit card info and charge the invoice
                if (!string.IsNullOrEmpty(token))
                {
                    ErrorLogger.LogError("Token: " + token + "  EmailAddress: " + emailAddress, ActiveLogin.LoginID, 0, "InvoiceController Stripe");

                    // First save the credit card info
                    FunctionResultObj<StripeInfo> stripeInfoResults = _stripeService.ProcessStripeToken(token, emailAddress, ActiveLogin.LoginID);
                    if (stripeInfoResults.Success)
                    {
                        string autoPayString = Request.Form["AutoPaySelectedString"];
                        if (InputHelper.GetBoolean(autoPayString))
                        {
                            new TurnOnAutoPay(ActiveLogin.LoginID, "ForcedWhenSaved").Execute();
                        }

                        vm.SelectedInvoices = Request.Form["SelectedInvoices"];

                        if (string.IsNullOrEmpty(vm.SelectedInvoices))
                        {
                            return Redirect("/" + ActiveLogin.SiteUserID + "/settings/billing");
                        }

                        if (!string.IsNullOrEmpty(vm.SelectedEarlyRenew))
                        {
                            DateTime now = DateTime.Now;
                            List<Invoice> invoiceList = GetInovicesByIDList(ActiveLogin.LoginID, vm.SelectedEarlyRenew);
                            foreach (Invoice invoice in invoiceList)
                            {
                                invoice.EarlyRenewalStamp = now;
                                invoice.Save();
                            }
                        }

                        // Pay the invoices
                        List<Invoice> invoices = GetInovicesByIDList(ActiveLogin.LoginID, vm.SelectedInvoices);
                        string stripeCustomerID = _stripeService.GetStripeCustomerID(ActiveLogin.LoginID, true);
                        FunctionResult result = _paymentService.PayInvoices(invoices, ActiveLogin.LoginID, stripeCustomerID);

                        _siteLoginManager.RefreshInvoiceInformationForAccount(ActiveLogin.LoginID);
                    }
                    else
                    {
                        vm.Message = stripeInfoResults.ErrorMessage;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, ActiveLogin.LoginID, "InvoiceController StripeProcessing");
                vm.Message = string.Format(Proestimator.Resources.ProStrings.ErrorProcessingPayment, ex.Message);
            }

            CustomerInvoiceMapper.Fill(ActiveLogin.LoginID, vm, Session);
            return View(vm);
        }

        /// <summary>
        /// Return a list of Invoice objects that belong to the passed LoginID and who's IDs are in the selectedInvoiceIDList
        /// </summary>
        /// <param name="selectedInvoiceIDsList">A comma separated list of Invoice IDs.</param>
        private List<Invoice> GetInovicesByIDList(int loginID, string selectedInvoiceIDsList)
        {
            List<string> invoiceIDs = selectedInvoiceIDsList.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            List<Invoice> allInvoices = Invoice.GetForLogin(loginID);

            List<Invoice> selectedInvoices = allInvoices.Where(o => invoiceIDs.Contains(o.ID.ToString())).ToList();
            return selectedInvoices;
        }

        [HttpPost]
        [Route("{userID}/invoice/delete-cc")]
        public ActionResult DeleteCC(CustomerInvoiceVM vm)
        {
            _stripeService.DeleteStripeCreditCard(vm.LoginID);

            return Redirect("/" + ActiveLogin.SiteUserID + "/invoice/customer-invoice");
        }

        #endregion

        [HttpPost]
        [Route("{userID}/invoice/clear-cc-error")]
        public ActionResult ClearCCError(CustomerInvoiceVM vm)
        {
            StripeInfo stripeInfo = StripeInfo.GetForLogin(vm.LoginID);
            stripeInfo.ErrorMessage = "";
            stripeInfo.CardError = false;

            stripeInfo.Save(ActiveLogin.ID);

            return RedirectToAction("CustomerInvoice");
        }


        [HttpGet]
        [Route("{userID}/invoice/{estimateID}/contact-webest")]
        public ActionResult ContactWebEst(int userID, int estimateID)
        {
            ViewBag.Message = Proestimator.Resources.ProStrings.ContactWebEstToAddFeatureTrialAccount;
            ViewBag.EstimateID = estimateID;

            return View();
        }
    }
}