using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Logic
{
    public class SuccessBoxDataSyncer
    {

        public void SyncLogins(StringBuilder builder, System.ComponentModel.BackgroundWorker callerWorker = null)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult unpostedLogins = db.ExecuteWithTable("SuccessBoxAccountLog_GetUnposted");
            if (unpostedLogins.Success)
            {
                StringBuilder detailsBuilder = new StringBuilder();

                SuccessBoxDataPoster poster = new SuccessBoxDataPoster();

                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                builder.AppendLine("Sync logins found " + unpostedLogins.DataTable.Rows.Count.ToString() + " logins to sync");
                int count = 0;

                foreach(DataRow row in unpostedLogins.DataTable.Rows)
                {
                    if (callerWorker != null && callerWorker.CancellationPending)
                    {
                        builder.AppendLine("SyncLogins canceled after " + stopwatch.ElapsedMilliseconds + " MS.  Processed " + count.ToString() + " accounts.");
                        break;
                    }

                    System.Diagnostics.Stopwatch loginStopwatch = new System.Diagnostics.Stopwatch();
                    loginStopwatch.Start();

                    int loginID = InputHelper.GetInteger(row["LoginID"].ToString());
                    FunctionResult loginPostResult = poster.PostAccount(loginID);

                    if (loginPostResult.Success)
                    {
                        detailsBuilder.AppendLine("Posted login " + loginID + " in " + loginStopwatch.ElapsedMilliseconds + " MS");
                        db.ExecuteNonQuery("SuccessBoxAccountLog_Insert", new SqlParameter("LoginID", loginID));
                    }
                    else
                    {
                        detailsBuilder.AppendLine("Error posting login " + loginID + ": " + loginPostResult.ErrorMessage);
                    }
                }

                string logText = detailsBuilder.ToString();
                if (!string.IsNullOrEmpty(logText))
                {
                    ErrorLogger.LogError(logText, "SuccessBox SyncLogins Details");
                }
                
                builder.AppendLine("SyncLogins took " + stopwatch.ElapsedMilliseconds + " MS");
            }
            else
            {
                builder.AppendLine("Sync logins found no logins to sync");
            }
        }

        public void SyncContracts(StringBuilder builder)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult contractsResult = db.ExecuteWithTable("SuccessBox_GetContractsToSync");
            if (contractsResult.Success)
            {
                StringBuilder detailsBuilder = new StringBuilder();
                SuccessBoxDataPoster poster = new SuccessBoxDataPoster();

                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                builder.AppendLine("Sync Contracts found " + contractsResult.DataTable.Rows.Count.ToString() + " Contracts to sync");

                foreach(DataRow row in contractsResult.DataTable.Rows)
                {
                    Contract contract = new Contract(row);
                    FunctionResult postContractResult = poster.PostContract(contract);

                    if (postContractResult.Success)
                    {
                        detailsBuilder.AppendLine("Contract " + contract.ID + " synced.");
                        SuccessBoxContractLogUpdate(contract.ID, 0, true, db);
                    }
                    else
                    {
                        detailsBuilder.AppendLine("Error syncing Contract " + contract.ID + ": " + postContractResult.ErrorMessage);
                    }
                }

                string logText = detailsBuilder.ToString();
                if (!string.IsNullOrEmpty(logText))
                {
                    ErrorLogger.LogError(logText, "SuccessBox SyncContracts Details");
                }
                
                builder.AppendLine("SyncContracts took " + stopwatch.ElapsedMilliseconds + " MS");
            }
            else
            {
                builder.AppendLine("Sync Contracts found no contracts to sync");
            }
        }

        public void SyncContractAddOns(StringBuilder builder)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult contractsResult = db.ExecuteWithTable("SuccessBox_GetContractAddOnsToSync");
            if (contractsResult.Success)
            {
                StringBuilder detailsBuilder = new StringBuilder();
                SuccessBoxDataPoster poster = new SuccessBoxDataPoster();

                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                builder.AppendLine("Sync Contract Add Ons found " + contractsResult.DataTable.Rows.Count.ToString() + " Add Ons to sync");

                foreach (DataRow row in contractsResult.DataTable.Rows)
                {
                    ContractAddOn addOn = new ContractAddOn(row);
                    Contract contract = Contract.Get(addOn.ContractID);

                    FunctionResult postContractResult = poster.PostContract(contract, addOn);

                    if (postContractResult.Success)
                    {
                        detailsBuilder.AppendLine("Contract Add On " + addOn.ID + " synced.");
                        SuccessBoxContractLogUpdate(contract.ID, addOn.ID, true, db);
                    }
                    else
                    {
                        detailsBuilder.AppendLine("Error syncing Contract Add On " + addOn.ID + ": " + postContractResult.ErrorMessage);
                    }
                }

                string logText = detailsBuilder.ToString();
                if (!string.IsNullOrEmpty(logText))
                {
                    ErrorLogger.LogError(logText, "SuccessBox SyncContractAddOns Details");
                }
                
                builder.AppendLine("SyncContractAddOns took " + stopwatch.ElapsedMilliseconds + " MS");
            }
            else
            {
                builder.AppendLine("Sync Contracts Add Ons found no contracts to sync");
            }
        }

        public void SyncInvoices(StringBuilder builder)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult invoicesResult = db.ExecuteWithTable("SuccessBox_GetInvoicesToSync");
            if (invoicesResult.Success)
            {
                StringBuilder detailsBuilder = new StringBuilder();
                SuccessBoxDataPoster poster = new SuccessBoxDataPoster();

                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                builder.AppendLine("Sync Invoices found " + invoicesResult.DataTable.Rows.Count.ToString() + " Invoices to sync");

                foreach (DataRow row in invoicesResult.DataTable.Rows)
                {
                    Invoice invoice = new Invoice(row);
                    FunctionResult postInvoiceResult = poster.PostInvoice(invoice);

                    if (postInvoiceResult.Success)
                    {
                        detailsBuilder.AppendLine("Invoice " + invoice.ID + " synced.");
                        SuccessBoxInvoiceLogUpdate(invoice.ID, true, db);
                    }
                    else
                    {
                        detailsBuilder.AppendLine("Error syncing Invoice " + invoice.ID + ": " + postInvoiceResult.ErrorMessage);
                    }
                }

                string logText = detailsBuilder.ToString();
                if (!string.IsNullOrEmpty(logText))
                {
                    ErrorLogger.LogError(logText, "SuccessBox SyncInvoices Details");
                }
                
                builder.AppendLine("SyncInvoices took " + stopwatch.ElapsedMilliseconds + " MS");
            }
            else
            {
                builder.AppendLine("Sync Invoices found no invoices to sync");
            }
        }

        public static void SuccessBoxContractLogUpdate(int contractID, int addOnID, bool isSynced, DBAccess db = null)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractID", contractID));
            parameters.Add(new SqlParameter("AddOnID", addOnID));
            parameters.Add(new SqlParameter("IsSynced", isSynced));

            if (db == null)
            {
                db = new DBAccess();
            }
            db.ExecuteNonQuery("SuccessBoxContractLog_Update", parameters);

            if (!isSynced)
            {
                List<Invoice> invoices = Invoice.GetForContract(contractID, true, true);
                foreach(Invoice invoice in invoices)
                {
                    SuccessBoxInvoiceLogUpdate(invoice.ID, false, db);
                }
            }
        }

        public static void SuccessBoxInvoiceLogUpdate(int invoiceID, bool isSynced, DBAccess db = null)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("InvoiceID", invoiceID));
            parameters.Add(new SqlParameter("IsSynced", isSynced));

            if (db == null)
            {
                db = new DBAccess();
            }
            db.ExecuteNonQuery("SuccessBoxInvoiceLog_Update", parameters);
        }

    }
}
