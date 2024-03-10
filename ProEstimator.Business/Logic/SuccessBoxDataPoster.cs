using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Logic
{
    public class SuccessBoxDataPoster
    {

        private readonly HttpClient client = new HttpClient();

        // Sandbox
        //private string _apiKey = "qdFpf5a9lKaGnjPtri+KtId9lA6BToErWjYQEsGZ9hU=GpZenVf8sUJhF49SUo2INGJvwEj0TetP9fgOJX5jcPM=";
        //private string _secretKey = "yNEHIYxFWIaAnDcT+7qyX+SJT+A9tKnt+PS3SQKvoz4=0kXCy/FdtZLXXUu46n6UuoYm1qbEF2PvIJpI/Y68Sfo=";
        //private string _url = "https://webest-sandbox.np.customersuccessbox.com/api/v1_1/";

        // Production
        private string _apiKey = "2lkTQw436dNW9EJibqQ48X4iUr0/PDS8FuNASHDuMHY=";
        private string _secretKey = "O30VV8ms8sLAHBEJ2NHsd5tLqDXlquPmChulvLBF+Xo=";
        private string _url = "https://webest.customersuccessbox.com/api/v1_1/";

        public FunctionResult PostAccount(int loginID)
        {
            try
            {
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                Contact contact = Contact.GetContactForLogins(loginID);
                Address address = Address.GetForLoginID(loginID);

                List<Contract> contracts = Contract.GetAllForLogin(loginID, false).OrderByDescending(o => o.EffectiveDate).ToList();
                DateTime createdOnDate = DateTime.Now;
                foreach(Contract contract in contracts)
                {
                    if(contract.EffectiveDate < DateTime.Now)
                    {
                        createdOnDate = contract.EffectiveDate;
                        break;
                    }
                }

                // Post the Account record
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("account_id", loginID.ToString());
                parameters.Add("name", loginInfo.CompanyName);
                parameters.Add("email", contact.Email);
                parameters.Add("phone", contact.Phone);
                parameters.Add("sales_manager", loginInfo.SalesRepID.ToString());
                parameters.Add("created_on", createdOnDate.ToString("yyyy-MM-dd"));
                parameters.Add("updated_on", DateTime.Now.ToString("yyyy-MM-dd"));
                parameters.Add("billing_street", address.Line1);
                parameters.Add("billing_city", address.City);
                parameters.Add("billing_state", address.State);
                parameters.Add("billing_postal_code", address.Zip);
                parameters.Add("billing_country", "USA");

                DataResult postResult = PostData("account", parameters);
                if (!postResult.Success)
                {
                    return new FunctionResult(postResult.ErrorMessage);
                }

                // Post the User record.  
                parameters.Clear();
                parameters.Add("account_id", loginID.ToString());
                parameters.Add("user_id", loginID.ToString());
                parameters.Add("email", contact.Email);
                parameters.Add("first_name", contact.FirstName);
                parameters.Add("last_name", contact.LastName);
                parameters.Add("phone", contact.Phone);
                parameters.Add("role", "primary");
                parameters.Add("created_on", createdOnDate.ToString("yyyy-MM-dd"));

                postResult = PostData("user", parameters);
                if (!postResult.Success)
                {
                    return new FunctionResult(postResult.ErrorMessage);
                }

                return new FunctionResult();
            }
            catch(Exception ex)
            {
                return new FunctionResult(ex.Message);
            }
        }

        public FunctionResult PostContract(Contract contract)
        {
            try
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("account_id", contract.LoginID.ToString());
                parameters.Add("subscription_id", contract.ID.ToString());

                // parameters.Add("" contract.ContractPriceLevel.ContractTerms.NumberOfPayments

                List<Invoice> invoices = Invoice.GetForContract(contract.ID);
                decimal total = 0;
                invoices.ForEach(o => total += o.InvoiceTotal);
                parameters.Add("amount", total.ToString());

                parameters.Add("billing_period", (contract.ContractPriceLevel.ContractTerms.NumberOfPayments + (contract.ContractPriceLevel.ContractTerms.DepositAmount > 0 ? 1 : 0)).ToString());

                if (contract.ContractPriceLevel.ContractTerms.NumberOfPayments <= 5)
                {
                    parameters.Add("billing_period_unit", "year");
                }
                else
                {
                    parameters.Add("billing_period_unit", "month");
                }

                parameters.Add("subscription_start_on", contract.EffectiveDate.ToString("yyyy-MM-dd"));
                parameters.Add("subscription_ends_on", contract.ExpirationDate.ToString("yyyy-MM-dd"));

                if (contract.ExpirationDate < DateTime.Now)
                {
                    parameters.Add("cancelled_on", contract.ExpirationDate.ToString("yyyy-MM-dd"));
                }

                // Pass the contract status
                LoginInfo loginInfo = LoginInfo.GetByID(contract.LoginID);
                if (contract.IsDeleted || loginInfo.DoubtfulAccount || loginInfo.Disabled)
                {
                    parameters.Add("status", "Cancelled");
                }
                else
                {
                    if (contract.Active)
                    {
                        if (contract.EffectiveDate > DateTime.Now)
                        {
                            parameters.Add("status", "Future");
                        }
                        else
                        {
                            parameters.Add("status", "Active");
                        }
                    }
                    else
                    {
                        parameters.Add("status", "Suspended");
                    }
                }

                // Get the next payment due date
                Invoice nextDueInvoice = Invoice.GetForContract(contract.ID).OrderBy(o => o.DueDate).FirstOrDefault(o => o.Paid == false);
                if (nextDueInvoice != null)
                {
                    parameters.Add("next_billing_on", nextDueInvoice.DueDate.ToString("yyyy-MM-dd"));
                }
                else
                {
                    parameters.Add("next_billing_on", contract.ExpirationDate.ToString("yyyy-MM-dd"));
                }

                parameters.Add("plan_name", contract.ContractPriceLevel.ContractTerms.TermDescription);
                parameters.Add("payments", contract.ContractPriceLevel.ContractTerms.NumberOfPayments.ToString());

                string promoID = "";
                if (contract.PromoID > 0)
                {
                    PromoCode promoCode = PromoCode.GetByID(contract.PromoID);
                    if (promoCode != null)
                    {
                        promoID = promoCode.Code;
                    }
                }
                parameters.Add("Discount", promoID);

                DataResult postResult = PostData("subscription", parameters);
                if (!postResult.Success)
                {
                    return new FunctionResult(postResult.ErrorMessage);
                }
                else
                {
                    return new FunctionResult();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.Message, "SuccessBoxManager PostContract Error");
                return new FunctionResult(ex.Message);
            }
        }

        public FunctionResult PostContract(Contract contract, ContractAddOn addOn)
        {
            try
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("account_id", contract.LoginID.ToString());
                parameters.Add("subscription_id", contract.ID.ToString() + addOn.ID.ToString());

                List<Invoice> invoices = Invoice.GetForContractAddOn(addOn.ID);
                decimal total = 0;
                invoices.ForEach(o => total += o.InvoiceTotal);
                parameters.Add("amount", total.ToString());

                parameters.Add("billing_period", (addOn.PriceLevel.ContractTerms.NumberOfPayments + (addOn.PriceLevel.ContractTerms.DepositAmount > 0 ? 1 : 0)).ToString());

                if (addOn.PriceLevel.ContractTerms.NumberOfPayments <= 5)
                {
                    parameters.Add("billing_period_unit", "year");
                }
                else
                {
                    parameters.Add("billing_period_unit", "month");
                }

                parameters.Add("subscription_start_on", addOn.StartDate.ToString("yyyy-MM-dd"));
                parameters.Add("subscription_ends_on", contract.ExpirationDate.ToString("yyyy-MM-dd"));

                // Pass the contract status
                if (addOn.IsDeleted)
                {
                    parameters.Add("status", "Cancelled");
                }
                else
                {
                    if (addOn.Active)
                    {
                        parameters.Add("status", "Active");
                    }
                    else
                    {
                        parameters.Add("status", "Suspended");
                    }
                }

                // Get the next payment due date
                Invoice nextDueInvoice = Invoice.GetForContractAddOn(addOn.ID).OrderBy(o => o.DueDate).FirstOrDefault(o => o.Paid == false);
                if (nextDueInvoice != null)
                {
                    parameters.Add("next_billing_on", nextDueInvoice.DueDate.ToString("yyyy-MM-dd"));
                }
                else
                {
                    parameters.Add("next_billing_on", contract.ExpirationDate.ToString("yyyy-MM-dd"));
                }

                string contractName = "";
                if (addOn.AddOnType.ID == 8)
                {
                    contractName = "Multi User";
                }
                else if (addOn.AddOnType.ID == 5)
                {
                    contractName = "EMS";
                }
                else if (addOn.AddOnType.ID == 2)
                {
                    contractName = "Frame Data";
                }

                parameters.Add("plan_name", contractName);

                DataResult postResult = PostData("subscription", parameters);
                if (!postResult.Success)
                {
                    return new FunctionResult(postResult.ErrorMessage);
                }
                else
                {
                    return new FunctionResult();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.Message, "SuccessBoxManager PostContract Error");
                return new FunctionResult(ex.Message);
            }
        }

        public FunctionResult PostInvoice(Invoice invoice)
        {
            try
            {
                Contract contract = Contract.Get(invoice.ContractID);

                // Post the invoice record
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("invoice_id", invoice.ID.ToString());
                parameters.Add("subscription_id", invoice.ContractID.ToString() + (invoice.AddOnID > 0 ? invoice.AddOnID.ToString() : ""));
                parameters.Add("amount_paid", invoice.Paid ? invoice.InvoiceTotal.ToString() : "0");
                parameters.Add("amount_due", invoice.Paid ? "0" : invoice.InvoiceTotal.ToString());
                parameters.Add("due_on", invoice.DueDate.ToString("yyyy-MM-dd"));
                parameters.Add("paid_on", invoice.DatePaid.HasValue ? invoice.DatePaid.Value.ToString("yyyy-MM-dd") : "");
                parameters.Add("total", invoice.InvoiceTotal.ToString());
                parameters.Add("created_on", contract.DateCreated.ToString("yyyy-MM-dd"));

                if (invoice.IsDeleted || contract.IsDeleted)
                {
                    parameters.Add("status", "voided");
                }
                else
                {
                    if (invoice.Paid)
                    {
                        parameters.Add("status", "paid");
                    }
                    else
                    {
                        parameters.Add("status", "not_paid");
                    }
                }

                DataResult postResult = PostData("invoice", parameters);
                if (!postResult.Success)
                {
                    return new FunctionResult(postResult.ErrorMessage);
                }
                else
                {
                    return new FunctionResult();
                }
            }
            catch (Exception ex)
            {
                return new FunctionResult(ex.Message);
            }
        }

        public FunctionResult PostFeature(SuccessBoxFeatureLog feature)
        {
            try
            {
                // Post the invoice record
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("product_id", "Webest");
                parameters.Add("module_id", feature.Module.ToString().Replace("EstimateWriting", "Estimate Writing").Replace("RateProfile", "Rate Profile") + "_Proestimator");
                parameters.Add("feature_id", feature.Feature);
                parameters.Add("user_id", feature.LoginID.ToString());
                parameters.Add("account_id", feature.LoginID.ToString());
                parameters.Add("timestamp", feature.TimeStamp.AddHours(5).ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "Z");

                DataResult postResult = PostData("feature", parameters);
                if (!postResult.Success)
                {
                    ErrorLogger.LogError(postResult.ErrorMessage, "SuccessBoxManager PostFeature");
                    return new FunctionResult(postResult.ErrorMessage);
                }
                else
                {
                    return new FunctionResult();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.Message, "SuccessBoxManager PostFeature Error");
                return new FunctionResult(ex.Message);
            }
        }

        public DataResult GetData(string action, string parameter)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url + action + "/" + parameter);
                request.ContentType = "application/json";
                request.Accept = "*/*";
                request.Method = "GET";
                request.Headers.Add("Authorization", "Bearer " + _secretKey);

                var httpResponse = (HttpWebResponse)request.GetResponse();

                string result = "";

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

                return new DataResult(true, "", result);
            }
            catch (Exception ex)
            {
                return new DataResult(false, ex.Message, "");
            }
        }

        public DataResult PostData(string action, Dictionary<string, string> bodyParameters)
        {
            try
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://webest-sandbox.np.customersuccessbox.com/api/v1_1/" + action);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url + action);
                request.ContentType = "application/json";
                request.Accept = "*/*";
                request.Method = "POST";
                request.Headers.Add("Authorization", "Bearer " + _secretKey);

                if (bodyParameters != null && bodyParameters.Count > 0)
                {
                    string seperator = "";
                    StringBuilder jsonBuilder = new StringBuilder();
                    jsonBuilder.Append("{ ");

                    foreach(KeyValuePair<string, string> parameter in bodyParameters)
                    {
                        jsonBuilder.Append(seperator + "\"" + parameter.Key + "\":\"" + (parameter.Value == null ? "" : parameter.Value.Replace("\"", "")) + "\"");
                        seperator = ",";
                    }

                    jsonBuilder.Append(" }");

                    string json = jsonBuilder.ToString();
                    byte[] bytes = new ASCIIEncoding().GetBytes(json);
                    request.ContentLength = bytes.Length;

                    Stream stream = request.GetRequestStream();
                    stream.Write(bytes, 0, bytes.Length);
                }

                var httpResponse = (HttpWebResponse)request.GetResponse();
                string result = "";

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
                
                return new DataResult(true, "", result);
            }
            catch (Exception ex)
            {
                return new DataResult(false, ex.Message, "");
            }
        }
    }

    public class DataResult : FunctionResult
    {
        public string Response { get; private set; }

        public DataResult(bool success, string errorMessage, string response)
            : base(success, errorMessage)
        {
            Response = response;
        }
    }
}
