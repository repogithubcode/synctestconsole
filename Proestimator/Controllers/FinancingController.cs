using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using System.Net.Http;
using System.Text;
using System;
using System.Web.Mvc;
using HttpClient = System.Net.Http.HttpClient;
using System.Threading.Tasks;
using ProEstimator.Business.Logic;
using ProEstimator.Business.Model.Financing;
using System.Configuration;
using System.Linq;
using Kendo.Mvc.UI;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Kendo.Mvc.Extensions;
using Proestimator.Resources;

namespace Proestimator.Controllers
{
    public class FinancingController : SiteController
    {
        [HttpGet]
        [Route("{userID}/financing")]
        public ActionResult Financing(int userID)
        {
            if (ViewBag.ShowFinancing != true || userID != ActiveLogin.SiteUserID)
            {
                return new HttpStatusCodeResult(404);
            }

            SuccessBoxFeatureLog.LogFeature(ActiveLogin.LoginID, SuccessBoxModule.Search, "Financing page visited", ActiveLogin.ID);
            ViewBag.NavID = "financing";

            var vm = new WisetackFinancingVM
            {
                Success = true,
                ErrorMessage = ""
            };

            var wisetackMerchantInfo = FinancingService.Instance.GetWisetackMerchantData(ActiveLogin.LoginID);
            if (!string.IsNullOrEmpty(wisetackMerchantInfo?.SignupLink))
            {
                vm.MerchantID = wisetackMerchantInfo.MerchantID;
                vm.SignupLink = wisetackMerchantInfo.SignupLink;
                vm.Status = wisetackMerchantInfo.Status;
                vm.Reasons = wisetackMerchantInfo.Reasons;
                vm.CallbackDate = wisetackMerchantInfo.CallbackDate;
                vm.IsTrial = ActiveLogin.IsTrial;
            }

            return View(vm);
        }

        [HttpGet]
        [Route("{userID}/financingApproved")]
        public ActionResult FinancingApproved(int userID)
        {
            if (ViewBag.ShowFinancing != true || userID != ActiveLogin.SiteUserID)
            {
                return new HttpStatusCodeResult(404);
            }

            // Redirect to the financing page if the user is not approved (and does not have any loan apps)
            var merchantInfo = FinancingService.Instance.GetWisetackMerchantData(ActiveLogin.LoginID);
            var isDeclinedButWasPreviouslyApproved = FinancingService.Instance.IsDeclinedButWasPreviouslyApproved(merchantInfo.MerchantID);
            if (merchantInfo?.Status != "APPLICATION_APPROVED" && !isDeclinedButWasPreviouslyApproved)
            {
                return RedirectToAction("Financing");
            }

            SuccessBoxFeatureLog.LogFeature(ActiveLogin.LoginID, SuccessBoxModule.Search, "Financing Approved page visited", ActiveLogin.ID);
            ViewBag.NavID = "financing";

            var vm = new WisetackFinancingApprovedVM
            {
                Success = true,
                ErrorMessage = "",
                UserID = ActiveLogin.SiteUserID,
                MerchantID = merchantInfo.MerchantID,
                CallbackDate = merchantInfo.CallbackDate,
                Reasons = merchantInfo.Reasons,
                SignupLink = merchantInfo.SignupLink,
                Status = merchantInfo.Status,
                IsDeclinedButWasPreviouslyApproved = isDeclinedButWasPreviouslyApproved,
            };

            return View(vm);
        }

        public ActionResult GetMerchantLoanApplications([DataSourceRequest] DataSourceRequest request, int userID, string merchantID, bool includeAllLoanApplications)
        {
            var loanApps = new List<WisetackLoanApplicationInfoVM>();

            var loginID = _siteLoginManager.GetActiveLogin(userID, GetComputerKey())?.LoginID ?? 0;
            var merchantInfo = FinancingService.Instance.GetWisetackMerchantData(loginID);
            if (merchantInfo?.MerchantID != merchantID
               || !IsUserAuthorized(userID) || merchantInfo?.MerchantID != merchantID)
            {
                return Json(loanApps.ToDataSourceResult(request));
            }

            var db = new DBAccess();
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("LoginID", loginID),
                new SqlParameter("IncludeAllLoanApplications", includeAllLoanApplications ? 1 : 0)
            };

            var tableResult = db.ExecuteWithTable("Wisetack_MerchantLoanApplications_Get", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                var vm = new WisetackLoanApplicationInfoVM(row);

                if (!string.IsNullOrEmpty(vm.LoanApplicationStatus))
                {
                    switch (vm.LoanApplicationStatus)
                    {
                        case "INITIATED":
                        case "ACTIONS REQUIRED":
                            vm.LoanApplicationStatus = ProStrings.FinancingLoanApplicationStatusIncomplete;
                            break;
                        case "AUTHORIZED":
                        case "LOAN TERMS ACCEPTED":
                            vm.LoanApplicationStatus = ProStrings.FinancingLoanApplicationStatusApproved;
                            break;
                        case "LOAN CONFIRMED":
                        case "SETTLED":
                            vm.LoanApplicationStatus = ProStrings.FinancingLoanApplicationStatusPaid;
                            break;
                        case "DECLINED":
                            vm.LoanApplicationStatus = ProStrings.FinancingLoanApplicationStatusDeclined;
                            break;
                        case "EXPIRED":
                            vm.LoanApplicationStatus = ProStrings.FinancingLoanApplicationStatusExpired;
                            break;
                        case "CANCELED":
                            vm.LoanApplicationStatus = ProStrings.FinancingLoanApplicationStatusCanceled;
                            break;
                        case "REFUNDED":
                            vm.LoanApplicationStatus = ProStrings.FinancingLoanApplicationStatusRefunded;
                            break;
                    }
                }

                loanApps.Add(vm);
            }

            return Json(loanApps.ToDataSourceResult(request));
        }

        [HttpGet]
        [Route("{userID}/financingRequestLoanApp/{estimateID}")]
        public ActionResult FinancingRequestLoanApp(int userID, int estimateID)
        {
            if (ViewBag.ShowFinancing != true || userID != ActiveLogin.SiteUserID)
            {
                return new HttpStatusCodeResult(404);
            }
            SuccessBoxFeatureLog.LogFeature(ActiveLogin.LoginID, SuccessBoxModule.Search, "Financing Request Financing App page visited", ActiveLogin.ID);

            var estimate = new Estimate(estimateID);
            if (estimate == null)
            {
                return new HttpStatusCodeResult(404);
            }

            var mobileNumber = "";
            if (estimate.CustomerID > 0)
            {
                var customer = Customer.Get(estimate.CustomerID);
                if (customer != null)
                {
                    mobileNumber = GetFormattedPhoneNumber(customer.Contact, "CP", false);
                }
            }

            // NOTE: force user to specify the complete date when requesting a loan app from the Payment Info page
            //      However, when inserting a link into an email, text message or on a PDF, we set this to 25 days
            //      from today w/o the user specifying it.
            var vm = new WisetackFinancingRequestLoanAppVM
            {
                UserID = userID,
                EstimateID = estimateID,
                MobileNumber = mobileNumber,
                TransactionAmount = FinancingService.Instance.GetDefaultAmountTotalToFinance(estimate).ToString(),
                CompletionDate = ""
            };

            return View(vm);
        }

        [HttpGet]
        [Route("{userID}/GetFinancingLoanAppInfo/{estimateID}")]
        public JsonResult GetFinancingLoanAppInfo(int userID, int estimateID)
        {
            if (ViewBag.ShowFinancing != true || ActiveLogin.SiteUserID != userID)
            {
                return Json(new WisetackLoanApplicationForEstimateInfoVM
                {
                    Success = false,
                    ErrorMessage = "Unauthorized"
                }, JsonRequestBehavior.AllowGet);
            }

            var estimate = new Estimate(estimateID);
            if (estimate == null)
            {
                return Json(new WisetackLoanApplicationForEstimateInfoVM
                {
                    Success = false,
                    ErrorMessage = "Invalid Estimate ID"
                }, JsonRequestBehavior.AllowGet);
            }

            var vm = FinancingService.Instance.GetWisetackLoanApplicationForEstimateInfo(ActiveLogin.LoginID, estimateID);
            if (vm != null)
            {
                vm.GrandTotal = FinancingService.Instance.GetDefaultAmountTotalToFinance(estimate).ToString();
                vm.Success = true;

                return Json(vm, JsonRequestBehavior.AllowGet);
            }

            return Json(new WisetackLoanApplicationForEstimateInfoVM
            {
                Success = false,
                ErrorMessage = "Unexpected issue encountered when attempting to get Financing Application information for this estimate"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("{userID}/financing/GetWisetackMerchantInfo")]
        public JsonResult GetWisetackMerchantInfo(int userID)
        {
            if (ViewBag.ShowFinancing != true || ActiveLogin.SiteUserID != userID)
            {
                return Json(new WisetackFinancingVM
                {
                    Success = false,
                    ErrorMessage = "Unauthorized"
                }, JsonRequestBehavior.AllowGet);
            }

            var merchantInfo = FinancingService.Instance.GetWisetackMerchantData(ActiveLogin.LoginID);
            if (string.IsNullOrEmpty(merchantInfo?.MerchantID))
            {
                return Json(new WisetackFinancingVM
                {
                    Success = false,
                    ErrorMessage = "There is no wisetack financing data associated with this user"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new WisetackFinancingVM
            {
                Success = true,
                MerchantID = merchantInfo.MerchantID,
                SignupLink = merchantInfo.SignupLink,
                Status = merchantInfo.Status,
                Reasons = merchantInfo.Reasons,
                CallbackDate = merchantInfo.CallbackDate,
                IsTrial = ActiveLogin.IsTrial
            }, JsonRequestBehavior.AllowGet);
        }

        private string GetFormattedPhoneNumber(ProEstimatorData.DataModel.Contact contact, string phoneType, bool prefixPlus1)
        {
            if (contact == null)
                return null;

            var prefix = "";
            if (prefixPlus1)
            {
                prefix = "+1";
            }

            if (contact.PhoneNumberType1 == phoneType && InputHelper.GetNumbersOnly(contact.Phone).Length == 10)
            {
                return $"{prefix}{InputHelper.GetNumbersOnly(contact.Phone)}";
            }
            else if (contact.PhoneNumberType2 == phoneType && InputHelper.GetNumbersOnly(contact.Phone2).Length == 10)
            {
                return $"{prefix}{InputHelper.GetNumbersOnly(contact.Phone2)}";
            }
            else if (InputHelper.GetNumbersOnly(contact.Phone).Length == 10)
            {
                return $"{prefix}{InputHelper.GetNumbersOnly(contact.Phone)}";
            }
            else if (InputHelper.GetNumbersOnly(contact.Phone2).Length == 10)
            {
                return $"{prefix}{InputHelper.GetNumbersOnly(contact.Phone2)}";
            }

            return null;
        }

        [HttpGet]
        [Route("{userID}/financing/SendLoanApplication/{estimateID}")]
        public async Task<JsonResult> SendLoanApplication(int userID, int estimateID, string mobileNumber, string completionDate, string transactionAmount)
        {
            if (ViewBag.ShowFinancing != true || ActiveLogin.SiteUserID != userID)
            {
                return Json(new WisetackSendLoanApplicationVM
                {
                    Success = false,
                    ErrorMessage = "Unauthorized"
                }, JsonRequestBehavior.AllowGet);
            }

            var merchantInfo = FinancingService.Instance.GetWisetackMerchantData(ActiveLogin.LoginID);
            if (string.IsNullOrEmpty(merchantInfo?.MerchantID))
            {
                return Json(new WisetackSendLoanApplicationVM
                {
                    Success = false,
                    ErrorMessage = ProStrings.FinancingLoanApplicationErrorMessageNoMerchant
                }, JsonRequestBehavior.AllowGet);
            }

            if (!double.TryParse(transactionAmount, out double transactionAmountConverted)
                || transactionAmountConverted < 500 || transactionAmountConverted > 15000)
            {
                return Json(new WisetackSendLoanApplicationVM
                {
                    Success = false,
                    ErrorMessage = ProStrings.FinancingLoanApplicationErrorMessageInvalidAmount
                }, JsonRequestBehavior.AllowGet);
            }

            // Ensure the amount to finance is not more than the amount remaining
            var amtRemaining = FinancingService.Instance.GetDefaultAmountTotalToFinance(new Estimate(estimateID));
            if (transactionAmountConverted > amtRemaining)
            {
                return Json(new WisetackSendLoanApplicationVM
                {
                    Success = false,
                    ErrorMessage = $"The amount to finance cannot be greater than the amount remaining: {amtRemaining:C}."
                }, JsonRequestBehavior.AllowGet);
            }


            if (!DateTime.TryParse(completionDate, out DateTime competionDateConverted))
            {
                return Json(new WisetackSendLoanApplicationVM
                {
                    Success = false,
                    ErrorMessage = ProStrings.FinancingLoanApplicationErrorMessageInvalidDate
                }, JsonRequestBehavior.AllowGet);
            }

            if (competionDateConverted > DateTime.UtcNow.AddDays(30))
            {
                return Json(new WisetackSendLoanApplicationVM
                {
                    Success = false,
                    ErrorMessage = ProStrings.FinancingLoanApplicationErrorMessageDateTooFar
                }, JsonRequestBehavior.AllowGet);
            }

            mobileNumber = mobileNumber?.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
            if (mobileNumber == null || mobileNumber.Length != 10)
            {
                return Json(new WisetackSendLoanApplicationVM
                {
                    Success = false,
                    ErrorMessage = ProStrings.FinancingLoanApplicationErrorMessageInvalidMobileNumber
                }, JsonRequestBehavior.AllowGet);
            }

            mobileNumber = $"+1{mobileNumber}";
            completionDate = competionDateConverted.ToString("yyyy-MM-dd");

            var purchaseId = $"web-est-{estimateID}";

            var loanApplicationRequest = new WisetackLoanApplicationRequest
            {
                TransactionAmount = transactionAmountConverted.ToString(),
                ServiceCompletedOn = completionDate,
                TransactionPurpose = "Web-Est: Initiate Financing App",
                MobileNumber = mobileNumber,
                PurchaseId = purchaseId,
                CallbackURL = ConfigurationManager.AppSettings.Get("FinancingCallbackUrlLoanApplication").ToString(),
                AppSource = "invoice",
                SettlementDelay = false
            };

            var loanApplicationResponse = await FinancingService.Instance.GetWisetackLoanApplicationResponse(merchantInfo.MerchantID, loanApplicationRequest);
            if (!string.IsNullOrEmpty(loanApplicationResponse?.TransactionID) &&
                !string.IsNullOrEmpty(loanApplicationResponse?.PaymentLink))
            {
                // Check if there is an existing loan app. If so, call the Wisetack API to cancel it
                var existingLoanApp = FinancingService.Instance.GetWisetackLoanApplicationForEstimateInfo(ActiveLogin.LoginID, estimateID);
                if (!string.IsNullOrEmpty(existingLoanApp?.TransactionID) && !string.IsNullOrEmpty(existingLoanApp?.MerchantID))
                {
                    await FinancingService.Instance.CancelLoanApplication(existingLoanApp.MerchantID, existingLoanApp.TransactionID);
                }

                FinancingService.Instance.UpdateEstimateLoanApplicationInfo(estimateID, loanApplicationResponse.TransactionID,
                    loanApplicationResponse.PaymentLink, transactionAmountConverted.ToString(), competionDateConverted,
                    mobileNumber);

                return Json(new WisetackSendLoanApplicationVM
                {
                    Success = true,
                    TransactionID = loanApplicationResponse.TransactionID,
                    PaymentLink = loanApplicationResponse.PaymentLink
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new WisetackSendLoanApplicationVM
            {
                Success = false,
                ErrorMessage = $"There was an unexpected issue. {loanApplicationResponse?.Message}"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("{userID}/financing/GetLoanApplicationInsertLinkInfo/{estimateID}")]
        public async Task<JsonResult> GetLoanApplicationInsertLinkInfo(int userID, int estimateID)
        {
            if (ViewBag.ShowFinancing != true || ActiveLogin.SiteUserID != userID)
            {
                return Json(new WisetackLoanApplicaitonInsertLinkInfoVM
                {
                    Success = false,
                    ErrorMessage = "Unauthorized"
                }, JsonRequestBehavior.AllowGet);
            }

            var estimate = new ProEstimatorData.DataModel.Estimate(estimateID);
            if (estimate == null)
            {
                return Json(new WisetackLoanApplicaitonInsertLinkInfoVM
                {
                    Success = false,
                    ErrorMessage = "Invalid Estimate ID"
                }, JsonRequestBehavior.AllowGet);
            }

            var merchantInfo = FinancingService.Instance.GetWisetackMerchantData(ActiveLogin.LoginID);
            if (string.IsNullOrEmpty(merchantInfo?.MerchantID))
            {
                return Json(new WisetackSendLoanApplicationVM
                {
                    Success = false,
                    ErrorMessage = "There is no wisetack financing data associated with this user"
                }, JsonRequestBehavior.AllowGet);
            }

            var amountToFinance = FinancingService.Instance.GetDefaultAmountTotalToFinance(estimate);
            if (amountToFinance < 500 || amountToFinance > 15000)
            {
                return Json(new WisetackSendLoanApplicationVM
                {
                    Success = false,
                    ErrorMessage = ProStrings.FinancingLoanApplicationErrorMessageInvalidAmount
                }, JsonRequestBehavior.AllowGet);
            }

            // Check if there is an existing loan app. If so, return the link w/o creating a new one
            var existingLoanApp = FinancingService.Instance.GetWisetackLoanApplicationForEstimateInfo(ActiveLogin.LoginID, estimateID);
            if (!string.IsNullOrEmpty(existingLoanApp?.TransactionID) && !string.IsNullOrEmpty(existingLoanApp?.MerchantID))
            {
                return Json(new WisetackLoanApplicaitonInsertLinkInfoVM
                {
                    Success = true,
                    AmountToFinance = amountToFinance.ToString(),
                    TransactionID = existingLoanApp.TransactionID,
                    PaymentLink = existingLoanApp.LoanApplicationLink
                }, JsonRequestBehavior.AllowGet);
            }

            var purchaseId = $"web-est-{estimateID}";
            var completionDate = DateTime.Parse(DateTime.UtcNow.AddDays(25).ToString("MM/dd/yyyy"));

            var loanApplicationLinkRequest = new WisetackLoanApplicationLinkRequest
            {
                TransactionAmount = amountToFinance.ToString(),
                ServiceCompletedOn = completionDate.ToString("yyyy-MM-dd"),
                TransactionPurpose = "Web-Est: Generate Link for Financing App",
                PurchaseId = purchaseId,
                CallbackURL = ConfigurationManager.AppSettings.Get("FinancingCallbackUrlLoanApplication").ToString(),
                AppSource = "invoice",
                SettlementDelay = false
            };

            var loanApplicationLinkResponse = await FinancingService.Instance.GetWisetackLoanApplicationLinkResponse(merchantInfo.MerchantID, loanApplicationLinkRequest);

            if (!string.IsNullOrEmpty(loanApplicationLinkResponse?.TransactionID) &&
                !string.IsNullOrEmpty(loanApplicationLinkResponse?.PaymentLink))
            {
                // Setting the Mobile Number field to an empty string rather than null to show the field was updated from the
                //  default NULL value in this instance.
                FinancingService.Instance.UpdateEstimateLoanApplicationInfo(estimateID, loanApplicationLinkResponse.TransactionID,
                    loanApplicationLinkResponse.PaymentLink, amountToFinance.ToString(), completionDate, string.Empty);

                return Json(new WisetackLoanApplicaitonInsertLinkInfoVM
                {
                    Success = true,
                    TransactionID = loanApplicationLinkResponse.TransactionID,
                    PaymentLink = loanApplicationLinkResponse.PaymentLink,
                    AmountToFinance = amountToFinance.ToString()
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new WisetackLoanApplicaitonInsertLinkInfoVM
            {
                Success = false,
                ErrorMessage = $"There was an unexpected issue. {loanApplicationLinkResponse?.Message}"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("{userID}/financing/GetMerchantSignupLink")]
        public async Task<JsonResult> GetMerchantSignupLink(int userID)
        {
            SuccessBoxFeatureLog.LogFeature(ActiveLogin.LoginID, SuccessBoxModule.Search, "Financing page: Merchant Signup Requested", ActiveLogin.ID);

            if (ViewBag.ShowFinancing != true || ActiveLogin.SiteUserID != userID || ActiveLogin.IsTrial)
            {
                return Json(new WisetackMerchantSignupLinkVM
                {
                    Success = false,
                    ErrorMessage = "Unauthorized"
                }, JsonRequestBehavior.AllowGet);
            }

            // Check if this user already has a Merchant ID / Signup Link. If so, just return it.
            var merchantInfo = FinancingService.Instance.GetWisetackMerchantData(ActiveLogin.LoginID);
            if (!string.IsNullOrEmpty(merchantInfo?.MerchantID))
            {
                return Json(new WisetackMerchantSignupLinkVM
                {
                    Success = true,
                    MerchantID = merchantInfo.MerchantID,
                    SignupLink = merchantInfo.SignupLink
                }, JsonRequestBehavior.AllowGet);
            }

            var externalID = $"WEBEST-LOGIN-{ActiveLogin.LoginID}";
            var loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
            var contact = ProEstimatorData.DataModel.Contact.GetContact(loginInfo.ContactID);
            var address = ProEstimatorData.DataModel.Address.GetForContact(contact.ContactID);
            var stateOnCode = State.StatesList.Where(eachState => (string.Compare(eachState.Code, address.State, true) == 0)).FirstOrDefault<State>();
            var stateOnDescription = State.StatesList.Where(eachState => (string.Compare(eachState.Description, address.State, true) == 0)).FirstOrDefault<State>();
            var stateCode = stateOnCode != null ? stateOnCode.Code : stateOnDescription?.Code;


            var merchantSalesInfo = FinancingService.Instance.GetWisetackMerchantSalesInfo(ActiveLogin.LoginID);
            int ttmSales = Math.Max(0, (int)Math.Ceiling(merchantSalesInfo.TtmSales));
            int ttmSalesUnits = Math.Max(0, merchantSalesInfo.TtmSalesUnits);

            // Annualize the TTM Sales & Sales Units if the Merchant registered in the past year
            if (loginInfo.CreationDate > DateTime.Now.AddDays(-365))
            {
                var days = Math.Max(1, (DateTime.Now - loginInfo.CreationDate).Days);
                ttmSales *= (365 / days);
                ttmSalesUnits *= (365 / days);
            }

            var merchantSignupRequest = new WisetackMerchantSignupRequest
            {
                CallbackURL = ConfigurationManager.AppSettings.Get("FinancingCallbackUrlMerchantSignup").ToString(),
                SignupPurpose = "Web-Est merchant signup for Wisetack services",
                Business = new WisetackMerchantSignupRequestBusiness
                {
                    BusinessLegalName = string.IsNullOrWhiteSpace(loginInfo.CompanyName) ? null : loginInfo.CompanyName,
                    DoingBusinessAs = null,
                    BusinessAddress = string.IsNullOrWhiteSpace(address.Line1) ? null : address.Line1,
                    AddressSecondaryNumber = string.IsNullOrWhiteSpace(address.Line2) ? null : address.Line2,
                    Email = string.IsNullOrWhiteSpace(contact?.Email) ? null : contact?.Email,
                    City = string.IsNullOrWhiteSpace(address.City) ? null : address.City,
                    State = string.IsNullOrWhiteSpace(stateCode) ? null : stateCode,
                    Zip = string.IsNullOrWhiteSpace(address.Zip) ? null : address.Zip,
                    PhoneNumber = GetFormattedPhoneNumber(contact, "WP", true),
                    FederalEIN = InputHelper.GetNumbersOnly(loginInfo.FederalTaxID).Length == 9 ? InputHelper.GetNumbersOnly(loginInfo.FederalTaxID) : null,
                    TtmSales = ttmSales,
                    TtmSalesUnits = ttmSalesUnits,
                    TtmDisputes = 0,
                    TtmDisputesUnits = 0,
                    EnrollmentDate = loginInfo.CreationDate.ToString("yyyy-MM-dd"),
                    CustomerTier = null,
                    BusinessWebsite = null,
                    Industry = "Automotive",
                    ExternalId = externalID
                },
            };

            if (!string.IsNullOrWhiteSpace(contact?.Email))
            {
                merchantSignupRequest.Initiator = new WisetackMerchantSignupRequestInitiator
                {
                    FirstName = contact?.FirstName,
                    LastName = contact?.LastName,
                    Email = contact?.Email,
                    MobileNumber = null     // Note: setting to null. Else the merchant will receive a text message before filling out the form
                };
            }

            var wisetackMerchantSignupResponse = await FinancingService.Instance.GetWisetackMerchantSignupResponse(merchantSignupRequest);

            if (!string.IsNullOrEmpty(wisetackMerchantSignupResponse?.MerchantID) &&
                !string.IsNullOrEmpty(wisetackMerchantSignupResponse?.SignupLink))
            {
                // Update the Logins table, setting the Merchant ID & Signup Link
                // NOTE: As a result of the API call above to Wisetack, there should be records inserted
                //      into the WisetackCallbackMerchantSignup table as the user proceeds through the 
                //      signup process using the returned link.
                FinancingService.Instance.UpdateLoginMerchantInfo(ActiveLogin.LoginID, wisetackMerchantSignupResponse.MerchantID, wisetackMerchantSignupResponse.SignupLink);

                return Json(new WisetackMerchantSignupLinkVM
                {
                    Success = true,
                    MerchantID = wisetackMerchantSignupResponse.MerchantID,
                    SignupLink = wisetackMerchantSignupResponse.SignupLink
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new WisetackMerchantSignupLinkVM
            {
                Success = false,
                ErrorMessage = wisetackMerchantSignupResponse?.Message
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("{userID}/financing/GetMerchantPromoInfo/{estimateID}")]
        public async Task<JsonResult> GetMerchantPromoInfo(int userID, int estimateID)
        {
            if (ViewBag.ShowFinancing != true || ActiveLogin.SiteUserID != userID)
            {
                return Json(new WisetackLoanApplicationPromoVM
                {
                    Success = false,
                    ErrorMessage = "Unauthorized"
                }, JsonRequestBehavior.AllowGet);
            }

            var merchantInfo = FinancingService.Instance.GetWisetackMerchantData(ActiveLogin.LoginID);
            if (string.IsNullOrEmpty(merchantInfo?.MerchantID))
            {
                return Json(new WisetackLoanApplicationPromoVM
                {
                    Success = false,
                    ErrorMessage = "Cannot request Wisetack promotional information. There is no merchant ID for this user."
                }, JsonRequestBehavior.AllowGet);
            }

            var estimate = new ProEstimatorData.DataModel.Estimate(estimateID);
            if (estimate == null)
            {
                return Json(new WisetackLoanApplicationPromoVM
                {
                    Success = false,
                    ErrorMessage = "Invalid Estimate ID"
                }, JsonRequestBehavior.AllowGet);
            }


            var amount = FinancingService.Instance.GetDefaultAmountTotalToFinance(estimate);
            if (amount == 0)
                amount = 1000;
            var promoRequest = new WisetackLoanApplicationPromoRequest
            {
                Amount = amount.ToString(),
                MerchantId = merchantInfo.MerchantID
            };

            var promoResponse = await FinancingService.Instance.GetWisetackLoanApplicationPromoResponse(promoRequest);

            if (!string.IsNullOrEmpty(promoResponse?.Promo?.Tagline))
            {
                return Json(new WisetackLoanApplicationPromoVM
                {
                    Success = true,
                    MerchantID = promoResponse.MerchantID,
                    Headline = promoResponse.Promo.Headline,
                    Tagline = promoResponse.Promo.Tagline,
                    ValueProp1 = promoResponse.Promo.ValueProp1,
                    ValueProp2 = promoResponse.Promo.ValueProp2,
                    ValueProp3 = promoResponse.Promo.ValueProp3,
                    Button = promoResponse.Promo.Button,
                    Disclosure = promoResponse.Promo.Disclosure,
                    AsLowAsMonthlyPayment = promoResponse.Promo.AsLowAs?.MonthlyPayment,
                    AsLowAsTermLength = promoResponse.Promo.AsLowAs?.TermLength,
                    AsLowAsApr = promoResponse.Promo.AsLowAs?.Apr,
                    AsLowAsMinApr = promoResponse.Promo.AsLowAs?.MinApr,
                    AsLowAsMaxApr = promoResponse.Promo.AsLowAs?.MaxApr
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new WisetackLoanApplicationPromoVM
            {
                Success = false,
                ErrorMessage = $"There was an unexpected issue. No promo information returned from Wisetack."
            }, JsonRequestBehavior.AllowGet);
        }
    }
}