using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using Avalara.AvaTax.RestClient;

namespace ProEstimator.Business.Logic
{

    // 1/19/23 - We no longer need to void or commit tax transactions, that is going to be done elsewhere.  Leaving the code commented out in case the requirement changes back

    public static class TaxManager
    {

        private static bool? _avalaraEnabled;
        private static int _avalaraAccountID = 0;
        private static string _avalaraLicenseKey = "";
        private static bool _avalaraSandbox = false;
        private static string _avalaraCompanyCode = "";
        private static List<string> _avalaraStates = new List<string>();

        private static void LoadAvalaraConfig()
        {
            if (!_avalaraEnabled.HasValue)
            {
                string avalaraEnabled = ConfigurationManager.AppSettings["AvalaraEnabled"];
                _avalaraEnabled = !string.IsNullOrEmpty(avalaraEnabled) && avalaraEnabled.ToLower() == "true";

                if (_avalaraEnabled.HasValue && _avalaraEnabled.Value)
                {
                    _avalaraAccountID = InputHelper.GetInteger(ConfigurationManager.AppSettings["AvalaraAccountID"].ToString());
                    _avalaraLicenseKey = ConfigurationManager.AppSettings["AvalaraLicenseKey"].ToString();
                    _avalaraSandbox = InputHelper.GetBoolean(ConfigurationManager.AppSettings["AvalaraSandbox"].ToString());
                    _avalaraCompanyCode = ConfigurationManager.AppSettings["AvalaraCompanyCode"].ToString();

                    string statesString = ConfigurationManager.AppSettings["AvalaraStates"].ToString();
                    _avalaraStates = statesString.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }
        }

        public static FunctionResult CalculateTaxForInvoice(Invoice invoice, Address address)
        {
            // Make sure good data is passed
            if (address == null)
            {
                ErrorLogger.LogError("No address passed for invoice " + invoice.ID, invoice.LoginID, 0, "CalculateTaxForContract");
                return new FunctionResult("No address passed for invoice " + invoice.ID);
            }

            LoadAvalaraConfig();

            if (!_avalaraStates.Contains(address.State))
            {
                ErrorLogger.LogError("State '" + address.State + "' does not collect tax", invoice.LoginID, 0, "CalculateTaxForContract");
                return new FunctionResult("State '" + address.State + "' does not collect tax");
            }

            AvaTaxClientFunctionResult client = GetAvataxClient(invoice.LoginID);
            if (client.Success)
            {
                try
                {
                    var transaction = new TransactionBuilder(client.Client, _avalaraCompanyCode, DocumentType.SalesInvoice, invoice.LoginID.ToString())
                    .WithAddress(TransactionAddressType.SingleLocation, address.Line1, null, null, address.City, address.State, address.Zip, "US")
                    .WithLine(invoice.InvoiceAmount, 1, "P0000000", invoice.Summary)
                    .WithTransactionCode(invoice.ID.ToString())
                    .Create();

                    if (transaction.totalTax.HasValue)
                    {
                        invoice.SalesTax = transaction.totalTax.Value;
                        invoice.Save();

                        ErrorLogger.LogError("Tax calculated for invoice " + invoice.ID + ": " + transaction.totalTax.Value, invoice.LoginID, 0, "CalculateTaxForInvoice Good");
                    }
                    else
                    {
                        if (transaction.messages != null && transaction.messages.Count > 0)
                        {
                            StringBuilder builder = new StringBuilder();

                            foreach(var message in transaction.messages)
                            {
                                builder.AppendLine(message.details);
                            }

                            ErrorLogger.LogError("NO TAX calculated for invoice " + invoice.ID + ": " + builder.ToString(), invoice.LoginID, 0, "CalculateTaxForInvoice Bad");

                            return new FunctionResult(builder.ToString());
                        }
                        else
                        {
                            ErrorLogger.LogError("NO TAX calculated for invoice " + invoice.ID + " with no returned messaged", invoice.LoginID, 0, "CalculateTaxForInvoice Bad");
                        }
                    }
                }
                catch (Exception ex)
                {
                    AvaTaxError avaTaxError = ex as AvaTaxError;
                    if (avaTaxError != null)
                    {
                        string errorMessage = "Code: " + (avaTaxError.error.error.code.HasValue ? avaTaxError.error.error.code.Value.ToString() : "") + Environment.NewLine + "Message:" + avaTaxError.error.error.message;
                        ErrorLogger.LogError(errorMessage, invoice.LoginID, 0, "TaxManager CalcTaxForCustomInvoice");
                        return new FunctionResult(errorMessage);
                    }
                    else
                    {
                        ErrorLogger.LogError(ex, invoice.LoginID, "TaxManager CalcTaxForCustomInvoice");
                        return new FunctionResult(ex.Message);
                    }
                }
            }
            else
            {
                return new FunctionResult(client.ErrorMessage);
            }

            return new FunctionResult();
        }

        private class AvaTaxClientFunctionResult : FunctionResult
        {
            public AvaTaxClient Client { get; private set; }

            public AvaTaxClientFunctionResult(string errorMessage)
                : base(errorMessage)
            {

            }

            public AvaTaxClientFunctionResult(AvaTaxClient client)
                : base()
            {
                Client = client;
            }
        }

        private static AvaTaxClientFunctionResult GetAvataxClient(int loginID)
        {
            LoadAvalaraConfig();

            AvaTaxClient client;

            try
            {
                // Create a client and set up authentication
                client = new AvaTaxClient("WebEst", "1.0", Environment.MachineName, _avalaraSandbox ? AvaTaxEnvironment.Sandbox : AvaTaxEnvironment.Production)
                    .WithSecurity(_avalaraAccountID, _avalaraLicenseKey);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, 0, "TaxManager GetAvataxClient");
                return new AvaTaxClientFunctionResult("Error reaching tax service: " + ex.Message);
            }

            // Verify that we can ping successfully
            try
            {
                PingResultModel pingResult = client.Ping();
                if (pingResult.authenticated.HasValue && pingResult.authenticated.Value)
                {
                    ErrorLogger.LogError("", loginID, 0, "TaxManager GetAvataxClient GoodPing");
                    return new AvaTaxClientFunctionResult(client);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, 0, "TaxManager GetAvataxClient Ping");
                return new AvaTaxClientFunctionResult("Error reaching tax service: " + ex.Message);
            }

            ErrorLogger.LogError("Returning null client, ping failed.", loginID, 0, "TaxManager GetAvataxClient BadPing");
            return new AvaTaxClientFunctionResult("Error: The tax service could not be reached.");
        }

        // 1/19/2023 - these functions are no longer needed but kept in case they are needed again.
        #region Commented Out
        //public static void CommitTaxForInvoice(Invoice invoice)
        //{
        //    if (invoice == null)
        //    {
        //        return;
        //    }

        //    AvaTaxClientFunctionResult client = GetAvataxClient(invoice.LoginID);

        //    if (client.Success)
        //    {
        //        try
        //        {
        //            CommitTransactionModel commitModel = new CommitTransactionModel();
        //            commitModel.commit = true;

        //            TransactionModel transactionModel = client.Client.CommitTransaction(_avalaraCompanyCode, invoice.ID.ToString(), DocumentType.SalesInvoice, "", commitModel);
        //        }
        //        catch (Exception ex)
        //        {
        //            string errorMessage = "Error committing invoice " + invoice.ID + ": ";

        //            AvaTaxError avaTaxError = ex as AvaTaxError;
        //            if (avaTaxError != null)
        //            {
        //                errorMessage += avaTaxError.error.error.code + " - " + avaTaxError.error.error.message;
        //            }
        //            else
        //            {
        //                errorMessage += ex.Message;
        //            }

        //            if (!errorMessage.Contains("Transaction not found"))
        //            {
        //                ErrorLogger.LogError(errorMessage, invoice.LoginID, 0, "TaxManager Commit Transaction");
        //            }
        //        }
        //    }           
        //}

        //public static void UncommitTaxForInvoice(Invoice invoice)
        //{
        //    if (invoice == null)
        //    {
        //        return;
        //    }

        //    AvaTaxClientFunctionResult client = GetAvataxClient(invoice.LoginID);

        //    if (client.Success)
        //    {
        //        try
        //        {
        //            TransactionModel transactionModel = client.Client.UncommitTransaction(_avalaraCompanyCode, invoice.ID.ToString(), DocumentType.SalesInvoice, "");
        //        }
        //        catch (Exception ex)
        //        {
        //            string errorMessage = "Error Uncommitting invoice " + invoice.ID + ": ";

        //            AvaTaxError avaTaxError = ex as AvaTaxError;
        //            if (avaTaxError != null)
        //            {
        //                errorMessage += avaTaxError.error.error.code + " - " + avaTaxError.error.error.message;
        //            }
        //            else
        //            {
        //                errorMessage += ex.Message;
        //            }

        //            ErrorLogger.LogError(errorMessage, invoice.LoginID, 0, "TaxManager Uncommit Transaction");
        //        }
        //    }
        //}

        //public static void VoidInvoice(Invoice invoice)
        //{
        //    if (invoice == null || invoice.SalesTax == 0)
        //    {
        //        return;
        //    }

        //    AvaTaxClientFunctionResult client = GetAvataxClient(invoice.LoginID);
        //    if (client.Success)
        //    {
        //        try
        //        {
        //            VoidTransactionModel voidModel = new VoidTransactionModel();
        //            voidModel.code = VoidReasonCode.DocDeleted;

        //            TransactionModel transactionModel = client.Client.VoidTransaction(_avalaraCompanyCode, invoice.ID.ToString(), DocumentType.SalesInvoice, "", voidModel);
        //        }
        //        catch (Exception ex)
        //        {
        //            string errorMessage = "Error voiding invoice " + invoice.ID + ": ";

        //            AvaTaxError avaTaxError = ex as AvaTaxError;
        //            if (avaTaxError != null)
        //            {
        //                errorMessage += avaTaxError.error.error.code + " - " + avaTaxError.error.error.message;
        //            }
        //            else
        //            {
        //                errorMessage += ex.Message;
        //            }

        //            // Getting weird error messages, but it still looks like the transaction gets deleted on Avalara's site.  
        //            //ErrorLogger.LogError(errorMessage, invoice.LoginID, 0, "TaxManager VoidInvoice Transaction");
        //        }
        //    }
        //}
        #endregion
    }
}
