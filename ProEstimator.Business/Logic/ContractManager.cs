using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Business.Logic.Admin;
using ProEstimator.Business.Model.Account.Commands;

namespace ProEstimator.Business.Logic
{
    public class ContractFunctionResult : FunctionResult
    {
        public Contract Contract { get; set; }

        public ContractFunctionResult(Contract contract)
        {
            Contract = contract;
        }

        public ContractFunctionResult(string errorMessage)
            : base(errorMessage)
        {

        }
    }

    public class TrialFunctionResult : FunctionResult
    {
        public Trial Trial { get; set; }

        public TrialFunctionResult(Trial trial)
        {
            Trial = trial;
        }

        public TrialFunctionResult(string errorMessage)
            : base(errorMessage)
        {

        }
    }

    public static class ContractManager
    {

        public const int PaymentWindow = 14;
        public const int MaxRenewalWindow = 90;

        /// <summary>
        /// When the contract will expire in this many days we show them a renewal message and also let them create a new contract
        /// </summary>
        public const int RenewalWindow = 30;
        private const bool bSendEmail = false;

        public static TrialFunctionResult CreateTrial(int loginID, DateTime startDate, DateTime endDate, bool ems, bool frameData, bool qbExporter, bool proAdvisor, bool images, bool customReports, bool hasBundle, bool hasMultiUser)
        {
            Trial trial = new Trial();
            trial.Active = true;
            trial.CreationStamp = DateTime.Now;
            trial.EndDate = endDate.Date;
            trial.HasEMS = ems;
            trial.HasFrameData = frameData;
            trial.HasPDR = true;
            trial.HasQBExport = qbExporter;
            trial.HasProAdvisor = proAdvisor;
            trial.HasImages = images;
            trial.HasCustomReports = customReports;
            trial.HasBundle = hasBundle;
            trial.LoginID = loginID;
            trial.StartDate = startDate.Date;
            trial.HasMultiUser = hasMultiUser;

            SaveResult saveResult = trial.Save();
            if (saveResult.Success)
            {
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                loginInfo.ProfileLocked = true;
                loginInfo.Save();

                return new TrialFunctionResult(trial);
            }
            else
            {
                return new TrialFunctionResult(saveResult.ErrorMessage);
            }
        }        

        public static ContractFunctionResult CreateContract(int loginID, int contractPriceLevelID, string overideStartDate = "", bool addon = true, int activeLoginID = 0)
        {
            // Make sure the contractPriceLevel is valid
            ContractPriceLevel priceLevel = ContractPriceLevel.Get(contractPriceLevelID);
            if (priceLevel == null)
            {
                ErrorLogger.LogError("Invalid contract price level ID: " + contractPriceLevelID, loginID, 0, "ContractManager CreateContract PriceLevel");
                return new ContractFunctionResult("Invalid price level");
            }

            // ------------------------------------------------------------------------------------------------------------------------------------
            // First find the start date.  
            // ------------------------------------------------------------------------------------------------------------------------------------
            DateTime startDate = DateTime.Now.Date;

            // For the admin page, a user defined start date can be passed
            if (!string.IsNullOrEmpty(overideStartDate))
            {
                startDate = InputHelper.GetDateTime(overideStartDate);
            }
            else
            {
                // If not start date was passed this contract is being created by a user, find the start date automatically.

                // If there is an active contract or trial, use the expiration date
                Contract activeContract = Contract.GetActive(loginID);
                if (activeContract != null)
                {
                    startDate = activeContract.ExpirationDate.Date.AddDays(1);
                }
                else
                {
                    // If there is no active contract, see if there was a contract that expired within the last 90 days and use its expiration as the new start date
                    List<Contract> contracts = Contract.GetAllForLogin(loginID).OrderByDescending(o => o.ExpirationDate).ToList();
                    if (contracts.Count > 0 && contracts[0].Active && contracts[0].ExpirationDate > DateTime.Now.AddDays(-90))
                    {
                        startDate = contracts[0].ExpirationDate.AddDays(1);
                    }
                }

                // Check for a recent expiration on a Web Est contract
                DateTime? webEstExpiration = GetWebEstExpiration(loginID);
                if (webEstExpiration.HasValue && webEstExpiration.Value <= DateTime.Now.AddDays(30))
                {
                    startDate = webEstExpiration.Value.AddDays(1);
                }
            }

            // Create the new contract record
            Contract contract = new Contract();
            contract.LoginID = loginID;
            contract.ContractPriceLevel = priceLevel;
            contract.DateCreated = DateTime.Now;
            contract.EffectiveDate = startDate;
            contract.ExpirationDate = startDate.AddYears(1).AddDays(-1);
            contract.Active = true;
            SaveResult saveResult = contract.Save(activeLoginID);

            if (!saveResult.Success)
            {
                return new ContractFunctionResult(saveResult.ErrorMessage);
            }
            else
            {
                // The Contract was created, now create the invoices
                DateTime invoiceDue = startDate;

                // Figure out how many months between payments.  
                int invoiceMonthSkip = 1;
                //if (priceLevel.ContractTerms.NumberOfPayments < 11)
                //{
                //    invoiceMonthSkip = 12 / priceLevel.ContractTerms.NumberOfPayments;
                //}

                Address address = Address.GetForLoginID(loginID);

                // Add a deposit if required
                if (priceLevel.ContractTerms.DepositAmount > 0)
                {
                    CreateInvoice(loginID, contract, 0, 1, priceLevel.ContractTerms.DepositAmount, startDate, address);
                    invoiceDue = invoiceDue.AddMonths(invoiceMonthSkip);
                }

                // Add the recurring invoices
                for (int i = 1; i <= priceLevel.ContractTerms.NumberOfPayments; i++)
                {
                    CreateInvoice(loginID, contract, i, 2, priceLevel.PaymentAmount, invoiceDue, address);
                    invoiceDue = invoiceDue.AddMonths(invoiceMonthSkip);
                }

                // If there are any add ons attached to the last contract, recreate them
                Contract activeContract = Contract.GetActive(loginID);
                if (activeContract == null)
                {
                    List<Contract> contracts = Contract.GetAllForLogin(loginID).Where(o => o.ID != contract.ID).OrderByDescending(o => o.ExpirationDate).ToList();
                    if (contracts.Count > 0)
                    {
                        activeContract = contracts[0];
                    }
                }

                if (activeContract != null && addon)
                {
                    List<ContractAddOn> activeAddOns = ContractAddOn.GetForContract(activeContract.ID);
                    foreach (ContractAddOn addOn in activeAddOns)
                    {
                        if (addOn.PriceLevel.PaymentAmount > 0)     // Don't recreate add ons that were free
                        {
                            FunctionResult addOnResult = ContractManager.CreateContractAddOn(contract, addOn.PriceLevel.ID, addOn.AddOnType.ID, startDate, addOn.Quantity);
                            if (!addOnResult.Success)
                            {
                                ErrorLogger.LogError(addOnResult.ErrorMessage, loginID, contract.ID, "InvoiceController Add On " + addOn.AddOnType.Type);
                            }
                        }
                    }
                }

                return new ContractFunctionResult(contract);
            }
        }

        public static void CreateInvoice(int loginID, Contract contract, int paymentNumber, int invoiceTypeID, decimal invoiceAmount, DateTime dueDate, Address address, ContractAddOn addOn = null, string note = "", int quantity = 1, int num = 0)
        {
            Invoice invoice = new Invoice();
            invoice.LoginID = loginID;
            invoice.ContractID = contract.ID;
            invoice.AddOnID = addOn == null ? 0 : addOn.ID;
            invoice.PaymentNumber = paymentNumber;
            invoice.InvoiceType = InvoiceType.Get(invoiceTypeID);
            invoice.InvoiceAmount = invoiceAmount;
            if(invoiceAmount == 0)
            {
                invoice.Paid = true;
            }
            invoice.DueDate = dueDate.Date;
            invoice.Notes = note;

            if (addOn != null)
            {
                Invoice invoiceExist = Invoice.GetForContractAddOn(addOn.ID).Where(o => o.ContractIsDeleted == false && o.DueDate == dueDate.Date && (o.Paid == false || (invoiceAmount == 0 && o.InvoiceAmount == 0))).OrderBy(o => o.ID).FirstOrDefault();
                if (invoiceExist != null)
                {
                    invoice = invoiceExist;
                    invoice.InvoiceAmount += invoiceAmount;
                    if(invoice.InvoiceAmount == 0)
                    {
                        invoice.Paid = true;
                    }
                    invoice.Notes += note;
                }
                invoice.Summary = addOn.AddOnType.Type + (paymentNumber == 0 ? " - Down Payment" : " - Payment Number " + paymentNumber);
            }
            else
            {
                if (invoiceTypeID == 3)
                {
                    invoice.Summary = "Custom Invoice";
                }
                else
                {
                    invoice.Summary = contract.ContractPriceLevel.ContractTerms.TermDescription + (paymentNumber == 0 ? " - Down Payment" : " - Payment Number " + invoice.PaymentNumber);
                }
            }

            SaveResult saveResult = invoice.Save();

            FunctionResult functionResult = TaxManager.CalculateTaxForInvoice(invoice, address);

            if (addOn != null)
            {
                if (functionResult.Success)
                {
                    ContractAddOnHistory.Insert(addOn.ID, invoice.ID, invoiceAmount, invoice.SalesTax / addOn.Quantity * quantity, addOn.Quantity - quantity, addOn.Quantity, num, note);
                }
                else if (saveResult.Success)
                {
                    ContractAddOnHistory.Insert(addOn.ID, invoice.ID, invoiceAmount, 0, addOn.Quantity - quantity, addOn.Quantity, num, note);
                }
            }
        }

        public static ContractFunctionResult RenewContract(int loginID)
        {
            LoginAutoRenew autoRenew = LoginAutoRenew.GetLastForLogin(loginID);
            if (autoRenew != null && autoRenew.Enabled)
            {
                Contract activeContract = Contract.GetActive(loginID);
                if (activeContract != null)
                {
                    return CreateContract(loginID, activeContract.ContractPriceLevel.ID);
                }

                return new ContractFunctionResult("Active contract not found for " + loginID + ".");
            }

            return new ContractFunctionResult("Auto renew is not enabled for " + loginID + ".");
        }

        public static FunctionResult DoAutoRenewContracts()
        {
            StringBuilder errorBuilder = new StringBuilder();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("LoginsAutoRenew_GetLoginsForRenewal");

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                int loginID = InputHelper.GetInteger(row["LoginID"].ToString());

                ContractFunctionResult result = RenewContract(loginID);
                if (!result.Success)
                {
                    errorBuilder.AppendLine(result.ErrorMessage);
                }
                else
                {
                    TurnOnAutoPay(loginID, result.Contract.IgnoreAutoPay, true);
                    result.Contract.AutoRenew = true;
                    result.Contract.Save();
                    //EmailManager.SendContractAutoRenewDone(loginID);//will be sent upon the first auto pay
                }
            }

            return new FunctionResult(errorBuilder.ToString());
        }

        public static bool TurnOnAutoPay(int loginID, bool ignoreAutoPay, bool update)
        {
            StripeInfo stripeInfo = StripeInfo.GetForLogin(loginID);
            if (stripeInfo != null && !string.IsNullOrEmpty(stripeInfo.StripeCardID))
            {
                if (!stripeInfo.AutoPay && !ignoreAutoPay)
                {
                    if (update)
                    {
                        return new TurnOnAutoPay(loginID, "ForcedOnAutoRenew").Execute();
                    }
                    return true;
                }
            }
            return false;
        }

        public static FunctionResult DoAutoRenewWarnings()
        {
            StringBuilder errorBuilder = new StringBuilder();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("LoginsAutoRenew_GetLoginsForWarning");

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                int loginID = InputHelper.GetInteger(row["LoginID"].ToString());

                Contract activeContract = Contract.GetActive(loginID);
                if (activeContract != null)
                {
                    EmailManager.SendContractAutoRenewWarning(loginID);
                    ReminderLog.Insert(loginID, activeContract.ID, "AutoRenewWarning");
                }
            }

            return new FunctionResult(errorBuilder.ToString());
        }

        public static FunctionResult CreateContractAddOnTrial(Contract parentContract, int contractType, DateTime startDate, DateTime endDate)
        {
            // Create the add on
            ContractAddOnTrial addOnTrial = new ContractAddOnTrial();
            addOnTrial.ContractID = parentContract.ID;
            addOnTrial.AddOnType = ContractType.Get(contractType);
            addOnTrial.StartDate = startDate;
            addOnTrial.EndDate = endDate;

            SaveResult saveResult = addOnTrial.Save();
            if (saveResult.Success)
            {
                return new FunctionResult();
            }
            else
            {
                return new FunctionResult(saveResult.ErrorMessage);
            }
        }

        public static List<ContractTerms> GetContractTermsForAddOn(int contractID, int addOnType, bool includeAll = false)
        {
            List<ContractTerms> contractTerms = ContractTerms.GetAll();
            contractTerms = contractTerms.Where(o => o.ContractTypeID == addOnType && o.Active && (o.NumberOfPayments > 0 || o.TermDescription.Contains("Upgrade"))).ToList();

            if (!includeAll)
            {
                contractTerms = FilterContractTerms(contractTerms, contractID, addOnType);
            }

            return contractTerms;
        }

        private static List<ContractTerms> FilterContractTerms(List<ContractTerms> contractTerms, int contractID, int addOnType)
        {
            // For the first year of the Bundle.  If an account already has a bundleable add on they get a different price level to upgrade
            // Until 5/10/2023 (for the first year of the bundle).
            if (addOnType == 12 && DateTime.Now < new DateTime(2023, 5, 10))
            {
                Contract contract = Contract.Get(contractID);
                if (contract != null)
                {
                    
                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(contractID);
                    bool hasAnyBundlableAddOn = addOns.Where(o => o.AddOnType.IsBundlable && o.HasPayment && !o.IsDeleted).Count() > 0;

                    if (hasAnyBundlableAddOn)
                    {
                        if (contract.DaysUntilExpiration > 90)
                        {
                            return contractTerms.Where(o => o.IsUpgrade && !o.TermDescription.Contains("Free")).ToList();
                        }
                        else if (contract.DaysUntilExpiration <= 90)
                        {
                            return contractTerms.Where(o => o.IsUpgrade && o.TermDescription.Contains("Free")).ToList();
                        }
                    }
                }
            }

            // If none of the above code returned a filtered list, filter out any terms that have "Updgrade" in their title.
            return contractTerms.Where(o => !o.IsUpgrade).ToList();
        }

        public static FunctionResult CreateContractAddOn(Contract parentContract, int priceLevelID, int contractType, DateTime startDate, int quantity = 1)
        {
            // Make sure the contractPriceLevel is valid
            ContractPriceLevel priceLevel = ContractPriceLevel.Get(priceLevelID);
            if (priceLevel == null)
            {
                return new ContractFunctionResult("Invalid price level, " + priceLevelID);
            }

            // Create the add on
            ContractAddOn addOn = new ContractAddOn();
            addOn.ContractID = parentContract.ID;
            addOn.PriceLevel = priceLevel;
            addOn.AddOnType = ContractType.Get(contractType);
            addOn.StartDate = startDate;
            addOn.Active = true;
            ContractAddOn addOnExist = ContractAddOn.GetForContract(parentContract.ID).FirstOrDefault(o => o.PriceLevel.ID == priceLevelID && o.Active);
            if(addOnExist != null)
            {
                addOn = addOnExist;
            }
            addOn.Quantity += quantity;

            SaveResult saveResult = addOn.Save();
            if (saveResult.Success)
            {
                // The Contract was created, now create the invoices
                DateTime invoiceDue = parentContract.EffectiveDate;

                // Figure out how many months between payments.  
                int invoiceMonthSkip = 1;
                if (priceLevel.ContractTerms.NumberOfPayments < 11 && priceLevel.ContractTerms.NumberOfPayments > 0)
                {
                    invoiceMonthSkip = 12 / priceLevel.ContractTerms.NumberOfPayments;
                }

                int paymentCounter = 1;

                decimal leftOverAmount = 0;
                int leftOverDays = 0;

                Address address = Address.GetForLoginID(parentContract.LoginID);
                int num = ContractAddOnHistory.CountForAddOn(addOn);
                // Add the recurring invoices
                for (int i = 1; i <= priceLevel.ContractTerms.NumberOfPayments; i++)
                {
                    DateTime invoiceStartDate = invoiceDue.Date;
                    DateTime endDate = invoiceDue.Date.AddMonths(invoiceMonthSkip);

                    // Stop if we've gone beyond the contract expiration date.  This can happen if the contract length was shortened from 12 months.
                    if (invoiceStartDate >= parentContract.ExpirationDate)
                    {
                        break;
                    }

                    if ((startDate.Date <= invoiceStartDate && endDate.AddDays(-1) <= parentContract.ExpirationDate) || priceLevel.ContractTerms.IsUpgrade)
                    {
                        // A full payment

                        // The upgrade doesn't get pro-rated, so it is here.  In this case we need to make sure the invoice start date is today
                        if (priceLevel.ContractTerms.IsUpgrade)
                        {
                            invoiceStartDate = startDate;
                        }

                        // If there is a left over payment from a period with 7 days or less, add that to this payment
                        if (leftOverDays > 0)
                        {
                            CreateInvoice(parentContract.LoginID, parentContract, paymentCounter, 2, 0, startDate.Date, address, addOn, "Prorated amount due included in next invoice", quantity, num);
                            CreateInvoice(parentContract.LoginID, parentContract, paymentCounter, 2, quantity * (priceLevel.PaymentAmount + leftOverAmount), invoiceStartDate, address, addOn, "Includes " + leftOverDays + " day" + (leftOverDays > 1 ? "s" : "") + " from previous billing cycle", quantity, num);

                            leftOverAmount = 0;
                            leftOverDays = 0;
                        }
                        else
                        {
                            CreateInvoice(parentContract.LoginID, parentContract, paymentCounter, 2, quantity * priceLevel.PaymentAmount, invoiceStartDate, address, addOn, "", quantity, num);
                        }

                        paymentCounter++;
                    }
                    else if (startDate.Date > invoiceStartDate && startDate.Date < endDate.AddDays(-7)) 
                    {
                        // A prorated first payment
                        decimal daysInPeriod = (endDate - invoiceStartDate).Days - 1;
                        decimal daysRemaining = (endDate - startDate.Date).Days - 1;
                        decimal percentOfDaysLeft = daysRemaining / daysInPeriod;
                        decimal paymentAmount = priceLevel.PaymentAmount * percentOfDaysLeft;

                        CreateInvoice(parentContract.LoginID, parentContract, paymentCounter, 2, quantity * paymentAmount, startDate.Date, address, addOn, "Prorated payment for " + daysRemaining + " out of " + daysInPeriod + " days in billing cycle", quantity, num);
                        paymentCounter++;
                    }
                    else if (startDate.Date <= invoiceStartDate && endDate > parentContract.ExpirationDate)
                    {
                        // A prorated last payment
                        decimal daysInPeriod = (endDate - invoiceStartDate).Days - 1;
                        decimal daysRemaining = (parentContract.ExpirationDate - invoiceStartDate).Days;
                        decimal percentOfDaysLeft = daysRemaining / daysInPeriod;
                        decimal paymentAmount = priceLevel.PaymentAmount * percentOfDaysLeft;

                        CreateInvoice(parentContract.LoginID, parentContract, paymentCounter, 2, quantity * paymentAmount, invoiceStartDate, address, addOn, "Prorated payment for " + daysRemaining + " out of " + daysInPeriod + " days in billing cycle", quantity, num);
                        paymentCounter++;

                        break;
                    }
                    else if (startDate.Date > invoiceStartDate && startDate.Date >= endDate.AddDays(-7))
                    {
                        // Less than 7 days left in this payment period, add these days to the next payment.
                        decimal daysInPeriod = (endDate - invoiceStartDate).Days;
                        leftOverDays = (endDate - startDate.Date).Days;
                        if (leftOverDays > 0)
                        {
                            decimal percentOfDaysLeft = (decimal)leftOverDays / daysInPeriod;
                            leftOverAmount = priceLevel.PaymentAmount * percentOfDaysLeft;
                        }
                    }

                    invoiceDue = invoiceDue.AddMonths(invoiceMonthSkip);
                }

                // If there is still a left over amount it means this add on was added within 7 days of the contract expiring so there is no future payment to add it to, add it now
                if (leftOverDays > 0)
                {
                    CreateInvoice(parentContract.LoginID, parentContract, paymentCounter, 2, 0, startDate.Date, address, addOn, "No charge due to upcoming end of contract", quantity, num);
                }      
                
                // Very special case.  If we added a free bundle add on (upgrade), delete existing bundlable add ons
                if (priceLevel.PaymentAmount == 0 && priceLevel.ContractTerms.ContractTypeID == 12)
                {
                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(addOn.ContractID);
                    addOns = addOns.Where(o => o.AddOnType.IsBundlable).ToList();

                    foreach (ContractAddOn eachAddOn in addOns)
                    {
                        eachAddOn.IsDeleted = true;
                        eachAddOn.Save();
                    }
                }

                return new FunctionResult();
            }
            else
            {
                return new FunctionResult(saveResult.ErrorMessage);
            }
        }

        public static FunctionResult RemoveContractAddOn(int loginID, int addOnID, int quantity = 1, int activeLoginID = 0)
        {
            ContractAddOn addOn = ContractAddOn.Get(addOnID);
            if (addOn == null)
            {
                return new ContractFunctionResult("Invalid add on, " + addOnID);
            }
            if (addOn.Quantity < quantity)
            {
                return new ContractFunctionResult("Invalid quantity, " + quantity);
            }

            while (quantity > 0)
            {
                List<Invoice> invoicesDelete = InvoiceDeletable(addOnID);
                List<ContractAddOnHistory> addOnHistory = ContractAddOnHistory.GetForAddOn(addOn);
                if (addOnHistory.Count > 0)
                {
                    int diff = addOnHistory[0].EndQuantity - addOnHistory[0].StartQuantity;
                    int tempDiff = addOn.Quantity - addOnHistory[0].StartQuantity;
                    int qty = quantity > tempDiff ? tempDiff : quantity;
                    bool notes = addOn.Quantity - qty == addOnHistory[0].StartQuantity;
                    bool success = false;

                    foreach (ContractAddOnHistory history in addOnHistory)
                    {
                        Invoice inv = Invoice.Get(history.InvoiceID);
                        
                        if(inv != null && invoicesDelete.FirstOrDefault(o => o.ID == inv.ID) != null)
                        {
                            decimal amount = history.Amount / diff * qty;
                            decimal tax = history.SalesTax / diff * qty;
                            string note = "";
                            inv.InvoiceAmount -= amount;
                            inv.SalesTax -= tax;
                            if (inv.InvoiceAmount <= 0.1M)
                            {
                                inv.InvoiceAmount = 0;
                                inv.SalesTax = 0;
                                inv.Paid = true;
                                if (addOn.Quantity == qty)
                                {
                                    inv.IsDeleted = true;
                                }
                            }
                            if (notes)
                            {
                                note = history.Notes;
                                int i = inv.Notes.LastIndexOf(note);
                                if (i >= 0)
                                {
                                    inv.Notes = inv.Notes.Substring(0, i) + inv.Notes.Substring(i + note.Length);
                                }
                            }
                            SaveResult invSaveResult = inv.Save(activeLoginID);
                            if (invSaveResult.Success)
                            {
                                success = true;
                                ContractAddOnHistory.Insert(addOn.ID, inv.ID, amount, tax, addOn.Quantity, addOn.Quantity - qty, 1, note);
                            }
                            else
                            {
                                ContractAddOnHistory.Insert(addOn.ID, inv.ID, amount, tax, addOn.Quantity, addOn.Quantity - qty, 0, note);
                                ErrorLogger.LogError("Invoice ID: " + inv.ID, loginID, 0, "Invoice Amount Save RemoveContractAddOn");
                            }
                        }
                    }
                    if (success)
                    {
                        addOn.Quantity -= qty;
                        if (addOn.Quantity == 0)
                        {
                            addOn.IsDeleted = true;
                        }
                        SaveResult saveResult = addOn.Save(activeLoginID);
                        if (saveResult.Success == false)
                        {
                            ErrorLogger.LogError("AddOn ID: " + addOn.ID, loginID, 0, "AddOn Quantity Save RemoveContractAddOn");
                        }
                    }
                    quantity -= qty;
                }
                else
                {
                    return new ContractFunctionResult("Invalid add on history, " + addOnID);
                }
            }
            
            return new FunctionResult();
        }

        public static List<Invoice> InvoiceDeletable(int addOnID)
        {
            List<Invoice> invoices = new List<Invoice>();
            ContractAddOn addOn = ContractAddOn.Get(addOnID);
            if (addOn == null)
            {
                return invoices;
            }

            int quantity = addOn.Quantity;
            while (quantity > 0)
            {
                List<ContractAddOnHistory> addOnHistory = ContractAddOnHistory.GetForAddOn(addOn);
                if (addOnHistory.Count > 0)
                {
                    int qty = addOnHistory[0].EndQuantity - addOnHistory[0].StartQuantity;
                    bool paid = false;
                    foreach (ContractAddOnHistory history in addOnHistory)
                    {
                        Invoice invoice = Invoice.Get(history.InvoiceID);
                        if (invoice != null && invoice.Paid && invoice.InvoiceAmount > 0)
                        {
                            paid = true;
                        }
                    }
                    if (!paid)
                    {
                        foreach (ContractAddOnHistory history in addOnHistory)
                        {
                            Invoice invoice = Invoice.Get(history.InvoiceID);
                            invoices.Add(invoice);
                        }
                    }
                    quantity -= qty;
                    addOn.Quantity -= qty;
                }
                else
                {
                    return invoices;
                }
            }

            return invoices;
        }

        public static DateTime? GetWebEstExpiration(int loginID)
        {
            DBAccess db = new DBAccess();
            // This times out locally, only do it when live
#if !DEBUG
            DBAccessStringResult result = db.ExecuteWithStringReturn("GetWebEstExpiration", new SqlParameter("LoginID", loginID));
            if (result.Success)
            {
                return InputHelper.GetDateTimeNullable(result.Value);
            }
#endif
            return null;
        }

        public static void InsertSalesBoard(int loginID, int contractID)
        {
            LoginInfo loginInfo = LoginInfo.GetByID(loginID);
            List<ContractAddOn> addOns = ContractAddOn.GetForContract(contractID).Where(o => o.HasPayment).ToList();

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("SalesBoard_ShouldInsert", new SqlParameter("LoginID", loginID));
            if (intResult.Success && intResult.Value > 0)
            {
                List<SqlParameter> insertParameters = new List<SqlParameter>();
                insertParameters.Add(new SqlParameter("LoginID", loginID));
                insertParameters.Add(new SqlParameter("Est", true));
                insertParameters.Add(new SqlParameter("Frame", (addOns.FirstOrDefault(o => o.AddOnType.ID == 2) != null)));
                insertParameters.Add(new SqlParameter("EMS", (addOns.FirstOrDefault(o => o.AddOnType.ID == 5) != null)));
                insertParameters.Add(new SqlParameter("SalesRep", loginInfo.SalesRepID));
                insertParameters.Add(new SqlParameter("DateSold", DateTime.Now));
                insertParameters.Add(new SqlParameter("AddUser", (addOns.FirstOrDefault(o => o.AddOnType.ID == 8) != null)));
                insertParameters.Add(new SqlParameter("HasQBExporter", (addOns.FirstOrDefault(o => o.AddOnType.ID == 9) != null)));
                insertParameters.Add(new SqlParameter("ProAdvisor", (addOns.FirstOrDefault(o => o.AddOnType.ID == 10) != null)));
                insertParameters.Add(new SqlParameter("Bundle", (addOns.FirstOrDefault(o => o.AddOnType.ID == 12) != null)));
                insertParameters.Add(new SqlParameter("ImageEditor", (addOns.FirstOrDefault(o => o.AddOnType.ID == 11) != null)));
                insertParameters.Add(new SqlParameter("EnterpriseReporting", (addOns.FirstOrDefault(o => o.AddOnType.ID == 13) != null)));

                db.ExecuteNonQuery("InsertSalesBoard", insertParameters);
            }

            try
            {
                //---------------------------------------------------------------------------------------------------------------------------
                // Send an e-mail to the sales rep and admins
                //---------------------------------------------------------------------------------------------------------------------------

                List<Contract> contracts = Contract.GetAllForLogin(loginID);

                // Set the email addresses
                List<string> emailAddresses = new List<string>();
                List<string> salesRepEmailAddresses = SalesRepPermissionManager.GetSalesRepEmailAddressForPermissionTag("IsEmailSentInsertSalesBoard");

                foreach (string eachSalesRepStaffEmailSent in salesRepEmailAddresses)
                {
                    emailAddresses.Add(eachSalesRepStaffEmailSent);
                }

                SalesRep salesRep = SalesRep.Get(loginInfo.SalesRepID);
                if (salesRep != null)
                {
                    emailAddresses.Add(salesRep.Email);
                }

                bool renewal = false;

                // Old Logins are from WE and always considered a renewal
                if (loginInfo.ID < 69000)
                {
                    renewal = true;
                }
                else if (contracts.Count > 1)
                {
                    renewal = true;
                }

                // Create the message
                System.Text.StringBuilder messageBuilder = new System.Text.StringBuilder();
                messageBuilder.AppendLine("New Contract!");
                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("Type: " + (renewal ? "Renewal" : "PURCHASE"));
                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("Sales Rep: " + salesRep.SalesNumber + " - " + salesRep.FirstName + " " + salesRep.LastName);
                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("Customer ID: " + loginID);
                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("Company Name: " + loginInfo.CompanyName);
                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("Purchased On: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                messageBuilder.AppendLine("");

                Contract newContract = Contract.Get(contractID);

                StringBuilder builder = new StringBuilder();

                List<Invoice> invoices = Invoice.GetForContract(contractID, false, false).Where(o => o.InvoiceType.ID == 1 || o.InvoiceType.ID == 2).ToList();  // Only get system added invoices for the base contract
                builder.Append("Got invoices for login: " + loginID + " contractID: " + contractID + ".  Invoice count: " + invoices.Count.ToString() + " ");
                decimal invoicesTotal = 0;
                decimal taxTotal = 0;
                foreach (Invoice invoice in invoices)
                {
                    invoicesTotal += invoice.InvoiceAmount;
                    taxTotal += invoice.SalesTax;

                    builder.Append("InvoiceID " + invoice.ID + " Amount " + invoice.InvoiceAmount + " Tax " + invoice.SalesTax + " | ");
                }

                ErrorLogger.LogError(builder.ToString(), "InsertSalesBoard Invoice Log");

                messageBuilder.AppendLine("Contract Effective Date: " + newContract.EffectiveDate.ToShortDateString());
                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("Contract Terms: " + newContract.ContractPriceLevel.ContractTerms.TermDescription);
                bool autoPay = false;
                StripeInfo stripeInfo = StripeInfo.GetForLogin(loginID);
                if (stripeInfo != null && !string.IsNullOrEmpty(stripeInfo.StripeCardID))
                {
                    autoPay = stripeInfo.AutoPay;
                    if (!autoPay && newContract.ContractPriceLevel.ContractTerms.ForceAutoPay && !newContract.IgnoreAutoPay)
                    {
                        autoPay = true;
                    }
                }
                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("Auto Pay: " + autoPay.ToString());

                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("Contract Amount: " + invoicesTotal.ToString("C"));

                if (newContract.PromoID > 0)
                {
                    PromoCode promoCode = PromoCode.GetByID(newContract.PromoID);
                    if (promoCode != null)
                    {
                        messageBuilder.AppendLine("");
                        messageBuilder.AppendLine("Promo Code: " + promoCode.Code);
                    }
                }

                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("New Frame Contract: " + (addOns.FirstOrDefault(o => o.AddOnType.ID == 2) != null));

                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("New EMS Contract: " + (addOns.FirstOrDefault(o => o.AddOnType.ID == 5) != null));

                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("New Multi User Contract: " + (addOns.FirstOrDefault(o => o.AddOnType.ID == 8) != null));

                List<ContractAddOn> muAddOns = addOns.Where(o => o.AddOnType.ID == 8).ToList();
                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("MULTI USER QTY: " + muAddOns.Sum(o => o.Quantity).ToString());

                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("New Bundle Contract: " + (addOns.FirstOrDefault(o => o.AddOnType.ID == 12) != null));

                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("New QB Contract: " + (addOns.FirstOrDefault(o => o.AddOnType.ID == 9) != null));

                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("New Pro Advisor Contract: " + (addOns.FirstOrDefault(o => o.AddOnType.ID == 10) != null));

                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("New Image Editor Contract: " + (addOns.FirstOrDefault(o => o.AddOnType.ID == 11) != null));

                messageBuilder.AppendLine("");
                messageBuilder.AppendLine("New Enterprise Reporting Contract: " + (addOns.FirstOrDefault(o => o.AddOnType.ID == 13) != null));

                string message = messageBuilder.ToString().Replace(Environment.NewLine, "<br />");

                Email email = new Email();
                email.LoginID = loginID;
                emailAddresses.ForEach(o => email.AddToAddress(o));
                email.Subject = "New Contract #" + loginID;
                email.Body = message;
                email.Save(0);
                EmailSender.SendEmail(email);
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "ContractManager SignComplete InsertSalesBoard");
            }

        }

        public static Contract GetLatestContract(int loginID)
        {
            return GetLatestContract(Contract.GetAllForLogin(loginID));
        }

        public static Contract GetLatestContract(List<Contract> allContracts)
        {
            Contract result = null;

            try
            {
                result = allContracts.OrderByDescending(c => c.ExpirationDate).FirstOrDefault();
            }
            catch { }

            return result;
        }

        public static List<Invoice> GetInvoicesToBePaid(int loginID)
        {
            List<Invoice> allInvoices = Invoice.GetForLogin(loginID);

            // Get all due invoices in a list
            List<Invoice> invoices = new List<Invoice>();

            invoices = allInvoices.Where(o => 
                !o.Paid 
                && 
                (
                    o.DaysUntilDue <= PaymentWindow || (o.PaymentNumber <= 1 && o.EarlyRenewal)
                )
            ).ToList();

            // Very special case... if there is an invoice for a Bundle add on, don't show invoices for bundlable add ons, they will be deleted after the bundle invoice is paid but haven't yet.
            bool hasBundleInvoice = false;
            List<Invoice> addOnInvoices = new List<Invoice>();

            foreach(Invoice invoice in invoices)
            {
                if (invoice.AddOnID > 0)
                {
                    ContractAddOn addOn = ContractAddOn.Get(invoice.AddOnID);
                    if (addOn != null)
                    {
                        if (addOn.AddOnType.ID == 12)
                        {
                            hasBundleInvoice = true;
                        }

                        if (addOn.AddOnType.IsBundlable)
                        {
                            addOnInvoices.Add(invoice);
                        }
                    }
                }
            }

            if (hasBundleInvoice)
            {
                foreach(Invoice invoice in addOnInvoices)
                {
                    if (invoices.Contains(invoice))
                    {
                        invoices.Remove(invoice);
                    }
                }
            }

            return invoices.OrderBy(o => o.DueDate).ToList();
        }

        /// <summary>
        /// Return True if the user has a current active contract, as well as another unpaid contract after this contrat already in the db.
        /// </summary>
        public static bool DoesUserHaveOnDeckContract(int loginID)
        {
            return DoesUserHaveOnDeckContract(Contract.GetAllForLogin(loginID));
        }

        public static bool DoesUserHaveOnDeckContract(List<Contract> allContracts)
        {
            List<Contract> contracts = allContracts.ToList();
            bool hasActive = false;
            bool hasFuture = false;

            foreach (Contract contract in contracts)
            {
                if (contract.EffectiveDate < DateTime.Now && contract.ExpirationDate > DateTime.Now)
                {
                    hasActive = true;
                }

                if (contract.EffectiveDate > DateTime.Now & contract.ExpirationDate > DateTime.Now)
                {
                    hasFuture = true;
                }
            }

            return hasActive && hasFuture;
        }

        public static bool IsInEarlyRenewalPeriod(int loginID, int renewNumDay = RenewalWindow)
        {
            Contract activeContract = Contract.GetActive(loginID);
            return IsInEarlyRenewalPeriod(activeContract, renewNumDay);
        }

        public static bool IsInEarlyRenewalPeriod(Contract activeContract, int renewNumDay = RenewalWindow)
        {
            // Check if we should show the user the early renewal message
            if (activeContract != null)
            {
                if(activeContract.ExpirationDate > DateTime.Now.AddDays(RenewalWindow).Date && activeContract.ExpirationDate <= DateTime.Now.AddDays(MaxRenewalWindow).Date)
                {
                    if (!DoesContractHavePaidRenewal(activeContract, false))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns True if there is another contract that starts after this one and has a payment
        /// </summary>
        public static bool DoesContractHavePaidRenewal(Contract activeContract)
        {
            List<Contract> allContracts = Contract.GetAllForLogin(activeContract.LoginID);

            StripeInfo stripeInfo = StripeInfo.GetForLogin(activeContract.LoginID);
            bool isAutoPay = stripeInfo != null && stripeInfo.AutoPay;

            foreach (Contract contract in allContracts)
            {
                if (contract.ID != activeContract.ID
                    && contract.ExpirationDate > activeContract.ExpirationDate
                    && (contract.HasPayment || isAutoPay)
                    && !contract.IsDeleted)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool DoesContractHavePaidRenewal(Contract activeContract, bool isAutoPay)
        {
            List<Contract> allContracts = Contract.GetAllForLogin(activeContract.LoginID);

            foreach (Contract contract in allContracts)
            {
                if (contract.ID != activeContract.ID
                    && contract.ExpirationDate > activeContract.ExpirationDate
                    && (contract.HasPayment || isAutoPay)
                    && !contract.IsDeleted)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsContractRenewal(Contract currentContract)
        {
            List<Contract> allContracts = Contract.GetAllForLogin(currentContract.LoginID);

            foreach (Contract contract in allContracts)
            {
                if (contract.ID != currentContract.ID
                    && contract.ExpirationDate <= currentContract.EffectiveDate
                    && contract.ExpirationDate.AddDays(90) >= currentContract.EffectiveDate
                    && contract.HasPayment
                    && !contract.IsDeleted)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Delete a contract that has no payments. 
        /// </summary>
        public static FunctionResult DeleteUncommitted(Contract contract, int loginID, bool deleteAddOns)
        {
            if (contract == null)
            {
                ErrorLogger.LogError("ContractID: " + contract.ID, loginID, 0, "DeleteUncommitted InvalidContract");
                return new FunctionResult("Could not delete existing contract.");
            }

            // Make sure there are no payments made to the contract
            List<Invoice> invoices = Invoice.GetForContract(contract.ID);
            if (invoices.Where(o => o.Paid == true).ToList().Count > 0)
            {
                ErrorLogger.LogError("ContractID: " + contract.ID, loginID, 0, "DeleteUncommitted HasInvoices");
                return new FunctionResult("Could not delete existing contract because there have already been payments made.");
            }

            // We can delete the contract.  First void the invoices with Avalara
            //foreach (Invoice invoice in invoices)
            //{
            //    TaxManager.VoidInvoice(invoice);
            //}

            // Delete the contract and invoices for the database
            ErrorLogger.LogError("Permanently deleting contract " + contract.ID, loginID, 0, "DeleteUncommited");

            DBAccess db = new DBAccess();
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractID", contract.ID));
            parameters.Add(new SqlParameter("DeleteAddOns", deleteAddOns));
            db.ExecuteNonQuery("Contract_DeleteUncommitted", parameters);

            return new FunctionResult();
        }

        public static bool DoesAccountHaveAnyPaidContracts(int loginID)
        {
            List<Contract> allContracts = Contract.GetAllForLogin(loginID).ToList();
            foreach (Contract contract in allContracts)
            {
                if (contract.HasPayment)
                {
                    return true;
                }
            }

            return false;
        }

        public static FunctionResult ExtendOrCreateTrial(int loginID, int days, int? salesRepId)
        {
            try
            {
                DateTime oldDate = DateTime.Now;
                DateTime newDate = DateTime.Now;

                Trial activeTrial = Trial.GetActive(loginID);
                if (activeTrial != null)
                {
                    oldDate = activeTrial.EndDate;

                    activeTrial.EndDate = activeTrial.EndDate.AddDays(days);
                    activeTrial.Save(salesRepId.HasValue ? salesRepId.Value : 0);

                    newDate = activeTrial.EndDate;
                }
                else
                {
                    newDate = DateTime.Now.AddDays(days).Date;
                    ContractManager.CreateTrial(loginID, DateTime.Now.Date, newDate, true, true, false, true, true, true, false, false);
                }

                InsertTrialExtension(loginID, salesRepId.HasValue ? salesRepId.Value : 0, oldDate, newDate);

                return new FunctionResult();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "ContractManager ExtendTrial");
                return new FunctionResult(ex.Message);
            }
        }

        public static void InsertTrialExtension(int loginID, int salesRep, DateTime original, DateTime newDate)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("SalesRep", salesRep));
            parameters.Add(new SqlParameter("ExtendFrom", original));
            parameters.Add(new SqlParameter("Days", (newDate - original).Days));
            parameters.Add(new SqlParameter("ExtendTo", newDate));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("ContractExtension_Insert", parameters);
        }

        #region Promos

        public static FunctionResult ApplyPromo(Contract contract, string promoCodeInput, int activeLoginID, bool ignoreDateRange = false)
        {
            if (contract.HasPayment)
            {
                return new FunctionResult("Cannot apply promo because contract already has a payment.");
            }

            if (contract.PromoID > 0)
            {
                return new FunctionResult("Contract already has a promo attached.  Only one promo is allowed per contract.");
            }

            if (string.IsNullOrEmpty(promoCodeInput))
            {
                return new FunctionResult("Please enter a promo code.");
            }

            PromoCode promoCode = PromoCode.Search(contract.ContractPriceLevel.ID, promoCodeInput, ignoreDateRange);
            return ApplyPromo(contract, promoCode, activeLoginID);
        }

        public static FunctionResult ApplyPromo(Contract contract, PromoCode promoCode, int activeLoginID)
        {
            if (promoCode != null)
            {
                // The Promo code is valid for this contract
                List<Invoice> invoices = Invoice.GetForContract(contract.ID).Where(o => o.InvoiceType.ID != 3).ToList();    // 3 is custom, only apply to base invoices.
                if (invoices != null)
                {
                    Invoice invoice = invoices.OrderBy(o => o.PaymentNumber).ToList()[0];
                    if (invoice != null)
                    {
                        contract.PromoID = promoCode.ID;
                        contract.Save(activeLoginID);

                        invoice.InvoiceAmount -= promoCode.PromoAmount;

                        // Add a note to the invoice, but only the first time the promo is applied
                        if (!invoice.Notes.Contains("Promo Code: " + promoCode.Code))
                        {
                            invoice.Notes += "Promo Code: " + promoCode.Code + ", Credit: " + promoCode.PromoAmount.ToString("C");
                        }
                        
                        TaxManager.CalculateTaxForInvoice(invoice, Address.GetForLoginID(contract.LoginID));
                        invoice.Save(activeLoginID);

                        return new FunctionResult();
                    }
                }
            }
            else
            {
                return new FunctionResult("Invalid Promo Code");
            }

            return new FunctionResult("There was an error applying the promo code, please contact support.");

        }

        public static FunctionResult RemovePromo(Contract contract, int activeLoginID)
        {
            if (contract.HasPayment)
            {
                return new FunctionResult("Cannot remove promo because contract already has a payment.");
            }

            if (contract.PromoID == 0)
            {
                return new FunctionResult("The contract does not have a promo code to remove.");
            }

            // Get the promo so we know how much to add back to the invoice
            PromoCode promo = PromoCode.GetByID(contract.PromoID);
            if (promo == null)
            {
                return new FunctionResult("The promo attached to this contract was not found and cannot be removed.  Please contact support.");
            }

            // Get the first invoice for this contract to remove the promo from
            List<Invoice> invoices = Invoice.GetForContract(contract.ID);
            if (invoices != null)
            {
                Invoice invoice = invoices.OrderBy(o => o.PaymentNumber).ToList()[0];
                if (invoice != null)
                {
                    contract.PromoID = 0;
                    contract.Save(activeLoginID);

                    invoice.InvoiceAmount += promo.PromoAmount;
                    if (invoice.Notes.Contains("Promo Code: " + promo.Code))
                    {
                        invoice.Notes = invoice.Notes.Replace("Promo Code: " + promo.Code + ", Credit: " + promo.PromoAmount.ToString("C"), "");
                    }
                    TaxManager.CalculateTaxForInvoice(invoice, Address.GetForLoginID(contract.LoginID));
                    invoice.Save(activeLoginID);

                    return new FunctionResult();
                }
            }

            return new FunctionResult("There was an error removing the promo code from this contract, please contact support.");
        }

        #endregion

    }

}
