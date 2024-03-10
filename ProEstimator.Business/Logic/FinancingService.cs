using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using ProEstimator.Business.Model.Financing;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Financing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using HttpClient = System.Net.Http.HttpClient;
using System.Net.Http;
using System.Text;
using System.Data;
using System.Web.Mvc;

namespace ProEstimator.Business.Logic
{
    public sealed class FinancingService
    {
        private FinancingService()
        {
        }

        private static readonly Lazy<FinancingService> lazyFinancingService = new Lazy<FinancingService>(() => new FinancingService());

        public static FinancingService Instance
        {
            get
            {
                return lazyFinancingService.Value;
            }
        }

        public WisetackLoanApplicationForEstimateInfoVM GetWisetackLoanApplicationForEstimateInfo(int loginID, int estimateID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Wisetack_LoanApplicationInfo_Get", parameters);
            if (tableResult.Success)
            {
                return new WisetackLoanApplicationForEstimateInfoVM(tableResult.DataTable.Rows.Count > 0 ? tableResult.DataTable.Rows[0] : null);
            }

            return null;
        }

        public async Task CancelLoanApplication(string merchantID, string transactionID)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {ConfigurationManager.AppSettings.Get("FinancingApiAuthKey")}");

                    var apiEndpoint = ConfigurationManager.AppSettings.Get("FinancingApiUrlLoanApplicationCancel").ToString()
                                        .Replace("[MERCHANT_ID]", merchantID)
                                        .Replace("[TRANSACTION_ID]", transactionID);
                    await client.PostAsync(apiEndpoint, null);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "FinancingService CancelLoanApplication PerformHttpRequest");
                throw ex;
            }
        }

        public async Task<WisetackMerchantSignupResponse> GetWisetackMerchantSignupResponse(WisetackMerchantSignupRequest merchantSignupRequest)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    var strJson = JsonConvert.SerializeObject(merchantSignupRequest, jsonSerializerSettings);
                    var content = new StringContent(strJson, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {ConfigurationManager.AppSettings.Get("FinancingApiAuthKey")}");

                    var response = await client.PostAsync(ConfigurationManager.AppSettings.Get("FinancingApiUrlMerchantSignup").ToString(), content);

                    var strData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<WisetackMerchantSignupResponse>(strData);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "FinancingService GetWisetackMerchantSignupResponse PerformHttpRequest");
            }

            return null;
        }

        public async Task<WisetackLoanApplicationResponse> GetWisetackLoanApplicationResponse(string merchantID, WisetackLoanApplicationRequest loanApplicationRequest)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    var strJson = JsonConvert.SerializeObject(loanApplicationRequest, jsonSerializerSettings);
                    var content = new StringContent(strJson, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {ConfigurationManager.AppSettings.Get("FinancingApiAuthKey")}");

                    var apiEndpoint = ConfigurationManager.AppSettings.Get("FinancingApiUrlLoanApplication").ToString()
                                        .Replace("[MERCHANT_ID]", merchantID);
                    var response = await client.PostAsync(apiEndpoint, content);

                    var strData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<WisetackLoanApplicationResponse>(strData);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "FinancingService GetWisetackLoanApplicationResponse PerformHttpRequest");
            }

            return null;
        }

        public async Task<WisetackLoanApplicationLinkResponse> GetWisetackLoanApplicationLinkResponse(string merchantID, WisetackLoanApplicationLinkRequest loanApplicationLinkRequest)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    var strJson = JsonConvert.SerializeObject(loanApplicationLinkRequest, jsonSerializerSettings);
                    var content = new StringContent(strJson, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {ConfigurationManager.AppSettings.Get("FinancingApiAuthKey")}");

                    var apiEndpoint = ConfigurationManager.AppSettings.Get("FinancingApiUrlLoanApplicationPaymentLink").ToString()
                                        .Replace("[MERCHANT_ID]", merchantID);
                    var response = await client.PostAsync(apiEndpoint, content);

                    var strData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<WisetackLoanApplicationLinkResponse>(strData);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "FinancingService GetWisetackLoanApplicationLinkResponse PerformHttpRequest");
            }

            return null;
        }

        public async Task<WisetackLoanApplicationPromoResponse> GetWisetackLoanApplicationPromoResponse(WisetackLoanApplicationPromoRequest promoRequest)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    var strJson = JsonConvert.SerializeObject(promoRequest, jsonSerializerSettings);
                    var content = new StringContent(strJson, Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {ConfigurationManager.AppSettings.Get("FinancingApiAuthKey")}");

                    var response = await client.PostAsync(ConfigurationManager.AppSettings.Get("FinancingApiUrlLoanApplicationPromo").ToString(), content);

                    var strData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<WisetackLoanApplicationPromoResponse>(strData);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "FinancingService GetWisetackLoanApplicationPromoResponse PerformHttpRequest");
            }

            return null;
        }

        public double GetDefaultAmountTotalToFinance(Estimate estimate)
        {
            var grandTotal = InputHelper.GetDouble(estimate?.GrandTotalString);

            if (grandTotal == 0)
                return 0;

            var allPayments = PaymentInfoData.GetAllPaymentInfo(estimate.EstimateID);
            if (allPayments != null)
            {
                grandTotal -= (double)allPayments.Sum(x => x.Amount);
            }

            return Math.Max(grandTotal, 0);
        }

        public bool InsertWisetackMerchantSignupCallbackInfo(WisetackMerchantSignupCallbackInfo callbackData)
        {
            DBAccess db = new DBAccess();
            List<SqlParameter> parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("CreatedDate", DateTime.UtcNow));
            parameters.Add(new SqlParameter("CallbackDate", callbackData.Date));
            parameters.Add(new SqlParameter("MerchantID", callbackData.MerchantId));
            parameters.Add(new SqlParameter("SignupLink", callbackData.SignupLink));
            parameters.Add(new SqlParameter("Status", callbackData.Status));
            parameters.Add(new SqlParameter("EventType", callbackData.EventType));
            parameters.Add(new SqlParameter("Reasons", callbackData.Reasons));
            parameters.Add(new SqlParameter("ExternalID", callbackData.ExternalId));
            parameters.Add(new SqlParameter("OnboardingType", callbackData.OnboardingType));
            parameters.Add(new SqlParameter("TransactionsEnabled", callbackData.TransactionsEnabled ? 1 : 0));
            parameters.Add(new SqlParameter("TransactionRange", callbackData.TransactionRange));

            var result = db.ExecuteNonQuery("WisetackCallbackMerchantSignup_Insert", parameters);

            return result.Success;
        }

        public bool InsertWisetackLoanApplicationCallbackInfo(WisetackLoanApplicationCallbackInfo callbackData)
        {
            DBAccess db = new DBAccess();
            List<SqlParameter> parameters = new List<SqlParameter>();

            string actionsRequired = null;

            parameters.Add(new SqlParameter("CreatedDate", DateTime.UtcNow));
            parameters.Add(new SqlParameter("TransactionID", callbackData.TransactionId));
            parameters.Add(new SqlParameter("CallbackDate", callbackData.Date));
            parameters.Add(new SqlParameter("MessageID", callbackData.MessageId));
            parameters.Add(new SqlParameter("ChangedStatus", callbackData.ChangedStatus));
            parameters.Add(new SqlParameter("ActionsRequired", actionsRequired));
            parameters.Add(new SqlParameter("RequestedLoanAmount", callbackData.RequestedLoanAmount));
            parameters.Add(new SqlParameter("ApprovedLoanAmount", callbackData.ApprovedLoanAmount));
            parameters.Add(new SqlParameter("SettledLoanAmount", callbackData.SettledLoanAmount));
            parameters.Add(new SqlParameter("ProcessingFee", callbackData.ProcessingFee));
            parameters.Add(new SqlParameter("MaximumAmount", callbackData.MaximumAmount));
            parameters.Add(new SqlParameter("TransactionPurpose", callbackData.TransactionPurpose));
            parameters.Add(new SqlParameter("ServiceCompletedOn", callbackData.ServiceCompletedOn));
            parameters.Add(new SqlParameter("TilaAcceptedOn", callbackData.TilaAcceptedOn));
            parameters.Add(new SqlParameter("LoanCreatedAt", callbackData.CreatedAt));
            parameters.Add(new SqlParameter("EventType", callbackData.EventType));
            parameters.Add(new SqlParameter("ExpirationDate", callbackData.ExpirationDate));
            parameters.Add(new SqlParameter("PrequalId", callbackData.PrequalId));
            parameters.Add(new SqlParameter("CustomerId", callbackData.CustomerId));

            var result = db.ExecuteNonQuery("WisetackCallbackLoanApplication_Insert", parameters);

            return result.Success;
        }

        public void UpdateLoginMerchantInfo(int loginID, string merchantID, string signupLink)
        {
            DBAccess db = new DBAccess();
            List<SqlParameter> parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("LoginID ", loginID));
            parameters.Add(new SqlParameter("MerchantID ", merchantID));
            parameters.Add(new SqlParameter("SignupLink ", signupLink));

            db.ExecuteNonQuery("Wisetack_LoginsMerchantID_Update", parameters);
        }

        public WisetackMerchantInfo GetWisetackMerchantData(int loginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Wisetack_MerchantInfo_Get", parameters);
            if (tableResult.Success)
            {
                return new WisetackMerchantInfo(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public WisetackMerchantSalesInfo GetWisetackMerchantSalesInfo(int loginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Wisetack_MerchantSalesInfo_Get", parameters);
            if (tableResult.Success)
            {
                return new WisetackMerchantSalesInfo(tableResult.DataTable.Rows[0]);
            }

            return new WisetackMerchantSalesInfo
            {
                TtmSales = 0,
                TtmSalesUnits = 0,
            };
        }

        public void UpdateEstimateLoanApplicationInfo(int estimateID, string transactionID, string paymentLink,
            string financeAmount, DateTime completionDate, string mobileNumber)
        {
            DBAccess db = new DBAccess();
            List<SqlParameter> parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("AdminInfoID", estimateID));
            parameters.Add(new SqlParameter("TransactionID", transactionID));
            parameters.Add(new SqlParameter("PaymentLink", paymentLink));
            parameters.Add(new SqlParameter("AmountToFinance", financeAmount));
            parameters.Add(new SqlParameter("EstimatedDateOfCompletion", completionDate));
            parameters.Add(new SqlParameter("LoanAppMobileNumber", mobileNumber));

            db.ExecuteNonQuery("Wisetack_AdminInfoTransactionID_Update", parameters);
        }

        public async Task<Tuple<string, string>> GetFinancingMonthlyAmountAndLoanAppLink(int loginID, ProEstimatorData.DataModel.Estimate estimate)
        {
            var monthlyAmount = "";
            var loanAppLink = "";

            var wisetackMerchantInfo = FinancingService.Instance.GetWisetackMerchantData(loginID);

            if (!string.IsNullOrEmpty(wisetackMerchantInfo?.MerchantID))
            {
                var existingLoanApp = FinancingService.Instance.GetWisetackLoanApplicationForEstimateInfo(loginID, estimate.EstimateID);
                if (!string.IsNullOrEmpty(existingLoanApp?.TransactionID) && !string.IsNullOrEmpty(existingLoanApp?.MerchantID))
                {
                    loanAppLink = existingLoanApp.LoanApplicationLink;
                }

                if (string.IsNullOrEmpty(loanAppLink))
                {
                    var purchaseId = $"web-est-{estimate.EstimateID}";
                    var completionDate = DateTime.Parse(DateTime.UtcNow.AddDays(25).ToString("MM/dd/yyyy"));

                    var loanApplicationLinkRequest = new WisetackLoanApplicationLinkRequest
                    {
                        TransactionAmount = estimate.GrandTotalString.Replace(",", ""),
                        ServiceCompletedOn = completionDate.ToString("yyyy-MM-dd"),
                        TransactionPurpose = "Web-Est: Generate Link for Loan App",
                        PurchaseId = purchaseId,
                        CallbackURL = ConfigurationManager.AppSettings.Get("FinancingCallbackUrlLoanApplication").ToString(),
                        AppSource = "invoice",
                        SettlementDelay = false
                    };

                    var loanApplicationLinkResponse = await FinancingService.Instance.GetWisetackLoanApplicationLinkResponse(wisetackMerchantInfo?.MerchantID, loanApplicationLinkRequest);

                    if (!string.IsNullOrEmpty(loanApplicationLinkResponse?.TransactionID) &&
                        !string.IsNullOrEmpty(loanApplicationLinkResponse?.PaymentLink))
                    {
                        // Setting the Mobile Number field to an empty string rather than null to show the field was updated from the
                        //  default NULL value in this instance.
                        FinancingService.Instance.UpdateEstimateLoanApplicationInfo(estimate.EstimateID, loanApplicationLinkResponse.TransactionID,
                            loanApplicationLinkResponse.PaymentLink, estimate.GrandTotalString.Replace(",", ""), completionDate, string.Empty);

                        loanAppLink = loanApplicationLinkResponse.PaymentLink;
                    }
                }

                var promoRequest = new WisetackLoanApplicationPromoRequest
                {
                    Amount = FinancingService.Instance.GetDefaultAmountTotalToFinance(estimate).ToString(),
                    MerchantId = wisetackMerchantInfo.MerchantID
                };

                var promoResponse = await FinancingService.Instance.GetWisetackLoanApplicationPromoResponse(promoRequest);

                monthlyAmount = promoResponse?.Promo?.AsLowAs?.MonthlyPayment ?? "";
            }

            return new Tuple<string, string>(monthlyAmount, loanAppLink);
        }

        public List<FinancingReportMerchantSignup> GetFinancingReportMerchantSignupData(string startDate, string endDate, 
            string merchantName, string loginID, string status, string minLoanApps)
        {
            int convertedLoginID = 0;
            int.TryParse(loginID, out convertedLoginID);
            int convertedMinLoanApps = 0;
            int.TryParse(minLoanApps, out convertedMinLoanApps);

            var results = new List<FinancingReportMerchantSignup>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("StartDate", startDate));
            parameters.Add(new SqlParameter("EndDate", endDate));
            parameters.Add(new SqlParameter("MerchantName", merchantName));
            parameters.Add(new SqlParameter("LoginID", convertedLoginID));
            parameters.Add(new SqlParameter("SignupStatus", status));
            parameters.Add(new SqlParameter("MinLoanAppCount", convertedMinLoanApps));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Wisetack_FinancingReportMerchantSignups_Get", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                var signupRow = new FinancingReportMerchantSignup(row);
                signupRow.SignupStatus = FinancingService.Instance.GetFinancingReportStatus(signupRow.SignupStatus);
                results.Add(signupRow);
            }

            return results;
        }

        public List<FinancingReportMerchantSignupLog> GetFinancingReportMerchantSignupLogData(string merchantID)
        {
            if (string.IsNullOrEmpty(merchantID))
                return new List<FinancingReportMerchantSignupLog>();

            var results = new List<FinancingReportMerchantSignupLog>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("MerchantID", merchantID));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Wisetack_FinancingReportMerchantSignupsLog_Get", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                var signupRow = new FinancingReportMerchantSignupLog(row);
                results.Add(signupRow);
            }

            return results;
        }

        public bool IsDeclinedButWasPreviouslyApproved(string merchantID)
        {
            var statuses = GetFinancingReportMerchantSignupLogData(merchantID);
            return statuses.LastOrDefault()?.SignupStatus == "APPLICATION_DECLINED" && statuses.Exists(x => x.SignupStatus == "APPLICATION_APPROVED");
        }

        public List<FinancingReportLoanApplicationLog> GetFinancingReportLoanApplicationLogData(string transactionID)
        {
            if (string.IsNullOrEmpty(transactionID))
                return new List<FinancingReportLoanApplicationLog>();

            var results = new List<FinancingReportLoanApplicationLog>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("TransactionID", transactionID));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Wisetack_FinancingReportLoanApplicationsLog_Get", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                var signupRow = new FinancingReportLoanApplicationLog(row);
                results.Add(signupRow);
            }

            return results;
        }

        public List<FinancingReportLoanApplication> GetFinancingReportMerchantLoanApplicationsData(string merchantID, bool includeAllLoanApps)
        {
            if (string.IsNullOrEmpty(merchantID))
                return new List<FinancingReportLoanApplication>();

            var results = new List<FinancingReportLoanApplication>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("MerchantID", merchantID));
            parameters.Add(new SqlParameter("IncludeAllLoanApplications", includeAllLoanApps));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Wisetack_FinancingReportMerchantLoanApplications_Get", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                var loanAppRow = new FinancingReportLoanApplication(row);
                loanAppRow.LoanApplicationStatus = FinancingService.Instance.GetFinancingLoanAppStatus(loanAppRow.LoanApplicationStatus);
                results.Add(loanAppRow);
            }

            return results;
        }

        public List<FinancingReportLoanApplication> GetFinancingReportLoanApplicationsData(string rangeStart, string rangeEnd,
                                                string loginID, string merchantName, string loanAppStatus, string estimateID, string customerName)
        {
            int convertedLoginID = 0;
            int.TryParse(loginID, out convertedLoginID);
            int convertedEstimateID = 0;
            int.TryParse(estimateID, out convertedEstimateID);

            var results = new List<FinancingReportLoanApplication>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("RangeStart", rangeStart));
            parameters.Add(new SqlParameter("RangeEnd", rangeEnd));
            parameters.Add(new SqlParameter("LoginID", convertedLoginID));
            parameters.Add(new SqlParameter("MerchantName", merchantName));
            parameters.Add(new SqlParameter("LoanAppStatus", loanAppStatus));
            parameters.Add(new SqlParameter("EstimateID", convertedEstimateID));
            parameters.Add(new SqlParameter("CustomerName", customerName));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Wisetack_FinancingReportLoanApplications_Get", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                var loanAppRow = new FinancingReportLoanApplication(row);
                loanAppRow.LoanApplicationStatus = FinancingService.Instance.GetFinancingLoanAppStatus(loanAppRow.LoanApplicationStatus);
                results.Add(loanAppRow);
            }

            return results;
        }

        public IEnumerable<SelectListItem> GetFinancingReportLoanAppStatusList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Selected = true,
                    Text = "All (w/ Activity)",
                    Value = "ALL_WITH_ACTIVITY"
                },
                new SelectListItem
                {
                    Text = "All",
                    Value = "ALL"
                },
                new SelectListItem
                {
                    Text = "Approved",
                    Value = "APPROVED"      // AUTHORIZED or LOAN TERMS ACCEPTED
                },
                new SelectListItem
                {
                    Text = "Canceled",
                    Value = "CANCELED"
                },
                new SelectListItem
                {
                    Text = "Declined",
                    Value = "DECLINED"
                },
                new SelectListItem
                {
                    Text = "Expired",
                    Value = "EXPIRED"
                },
                new SelectListItem
                {
                    Text = "Incomplete",
                    Value = "INCOMPLETE"    // INITIATED or ACTIONS REQUIRED
                },
                new SelectListItem
                {
                    Text = "Paid",
                    Value = "PAID"          // LOAN CONFIRMED or SETTLED
                },
                new SelectListItem
                {
                    Text = "Refunded",
                    Value = "REFUNDED"
                }
            };
        }

        public IEnumerable<SelectListItem> GetFinancingReportSignupStatusList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Selected = true,
                    Text = "All",
                    Value = ""
                },
                new SelectListItem
                {
                    Text = "Approved",
                    Value = "APPLICATION_APPROVED"
                },
                new SelectListItem
                {
                    Text = "Declined",
                    Value = "APPLICATION_DECLINED"
                },
                new SelectListItem
                {
                    Text = "In Progress",
                    Value = "InProgress" //Anything that is not Approved, Declined or Submitted
                },
                new SelectListItem
                {
                    Text = "In Progress or Submitted",
                    Value = "InProgressSubmitted" //Anything that is not Approved or Declined
                },
                new SelectListItem
                {
                    Text = "Submitted",
                    Value = "APPLICATION_SUBMITTED"
                },
            };
        }

        public string GetFinancingReportStatus(string wisetackStatus)
        {
            switch (wisetackStatus)
            {
                case "APPLICATION_APPROVED":
                    return "Approved";
                case "APPLICATION_DECLINED":
                    return "Declined";
                case "APPLICATION_SUBMITTED":
                    return "Submitted";
                default:
                    return "In Progress";
            }
        }

        public string GetFinancingLoanAppStatus(string wisetackStatus)
        {
            switch (wisetackStatus)
            {
                case "INITIATED":
                case "ACTIONS REQUIRED":
                    return "Incomplete";
                case "AUTHORIZED":
                case "LOAN TERMS ACCEPTED":
                    return "Approved but Incomplete";
                case "LOAN CONFIRMED":
                case "SETTLED":
                    return "Paid";
                case "DECLINED":
                    return "Declined";
                case "EXPIRED":
                    return "Expired";
                case "CANCELED":
                    return "Canceled";
                case "REFUNDED":
                    return "Refunded";
            }

            return "";
        }
    }
}
