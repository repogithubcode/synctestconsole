using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Contracts
{
    public class Invoice : ProEstEntity
    {

        // Database fields
        public int ID { get; set; }
        public int LoginID { get; set; }
        public int ContractID { get; set; }
        public int AddOnID { get; set; }
        public int FeeInvoiceID	{ get; set; }
        public int PaymentNumber { get; set; }
        public InvoiceType InvoiceType { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal SalesTax	{ get; set; }
        public DateTime DueDate	{ get; set; }
        public string Notes	{ get; set; }
        public string Summary { get; set; }
        public bool Paid { get; set; }
        public DateTime? DatePaid { get; set; }
        public int PaymentID { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? FailStamp { get; set; }
        public DateTime? EarlyRenewalStamp { get; set; }
        public bool EarlyRenewal { get; set; }

        // Calculated or derived fields
        public decimal InvoiceTotal
        {
            get { return InvoiceAmount + SalesTax; }
        }

        public bool ContractIsDeleted { get; private set; }

        public int DaysUntilDue
        {
            get
            {
                TimeSpan span = DueDate.Date - DateTime.Now.Date;
                return (int)span.TotalDays;
            }
        }

        public Invoice()
        {

        }

        public Invoice(DataRow row)
        {
            ID = InputHelper.GetInteger(row["InvoiceID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            ContractID = InputHelper.GetInteger(row["ContractID"].ToString());
            AddOnID = InputHelper.GetInteger(row["AddOnID"].ToString());
            FeeInvoiceID = InputHelper.GetInteger(row["FeeInvoiceID"].ToString());
            PaymentNumber = InputHelper.GetInteger(row["PaymentNumber"].ToString());
            InvoiceType = InvoiceType.Get(InputHelper.GetInteger(row["InvoiceTypeID"].ToString()));
            InvoiceAmount = InputHelper.GetDecimal(row["InvoiceAmount"].ToString());
            SalesTax = InputHelper.GetDecimal(row["SalesTax"].ToString());
            DueDate	= InputHelper.GetDateTime(row["DueDate"].ToString());
            Notes = row["Notes"].ToString();
            Summary = row["Summary"].ToString();
            Paid = InputHelper.GetBoolean(row["Paid"].ToString());
            if (!string.IsNullOrEmpty(row["DatePaid"].ToString()))
            {
                DatePaid = InputHelper.GetDateTime(row["DatePaid"].ToString());
            }
            PaymentID = InputHelper.GetInteger(row["PaymentID"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());
            FailStamp = InputHelper.GetDateTimeNullable(row["FailStamp"].ToString());
            EarlyRenewalStamp = InputHelper.GetDateTimeNullable(row["EarlyRenewalStamp"].ToString());
            Contract contract = Contract.Get(ContractID);
            if (contract != null)
            {
                EarlyRenewal = contract.EarlyRenewal;
            }

            RowAsLoaded = row;
        }

        public static Invoice Get(int invoiceID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetInvoice", new SqlParameter("InvoiceID", invoiceID));
            if (result.Success)
            {
                return new Invoice(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<Invoice> GetForLogin(int loginID)
        {
            List<Invoice> list = new List<Invoice>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetInvoicesForLogin", new SqlParameter("LoginID", loginID));
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    list.Add(new Invoice(row));
                }
            }

            return list;
        }

        public static List<Invoice> GetForContract(int contractID, bool includeAddons = false, bool showDeleted = false)
        {
            List<Invoice> list = new List<Invoice>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractID", contractID));
            parameters.Add(new SqlParameter("IncludeAddons", includeAddons));
            parameters.Add(new SqlParameter("Deleted", showDeleted));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetInvoicesForContract", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    list.Add(new Invoice(row));
                }
            }

            return list;
        }

        public static List<Invoice> GetForContractForAdmin(int contractID, bool includeAddons = false, bool showDeleted = false, bool showAddOnDeleted = false)
        {
            List<Invoice> list = new List<Invoice>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractID", contractID));
            parameters.Add(new SqlParameter("IncludeAddons", includeAddons));
            parameters.Add(new SqlParameter("Deleted", showDeleted));
            parameters.Add(new SqlParameter("AddOnDeleted", showAddOnDeleted));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetInvoicesForContractForAdmin", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    list.Add(new Invoice(row));
                }
            }

            return list;
        }

        public static List<Invoice> GetForContractAddOn(int addOnID)
        {
            List<Invoice> list = new List<Invoice>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetInvoicesForAddOn", new SqlParameter("AddOnID", addOnID));
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    list.Add(new Invoice(row));
                }
            }

            return list;
        }

        /// <summary>
        /// Get all invoices that are unpaid and due now.
        /// </summary>
        public static List<Invoice> GetAllDueInvoices()
        {
            List<Invoice> list = new List<Invoice>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetInvoices_AllDue", new SqlParameter("DueDate", DateTime.Now.AddDays(1)));
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    list.Add(new Invoice(row));
                }
            }

            return list;
        }

        /// <summary>
        /// Get all invoices that were paid by the passed Stripe Payment ID
        /// </summary>
        public static List<Invoice> GetByPaymentID(string paymentID)
        {
            List<Invoice> list = new List<Invoice>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetInvoicesForPaymentID", new SqlParameter("paymentID", paymentID));
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    list.Add(new Invoice(row));
                }
            }

            return list;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("InvoiceID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("ContractID", ContractID));
            parameters.Add(new SqlParameter("AddOnID", AddOnID));
            parameters.Add(new SqlParameter("FeeInvoiceID", FeeInvoiceID));
            parameters.Add(new SqlParameter("PaymentNumber", PaymentNumber));
            parameters.Add(new SqlParameter("InvoiceTypeID", InvoiceType.ID));
            parameters.Add(new SqlParameter("InvoiceAmount", InvoiceAmount));
            parameters.Add(new SqlParameter("SalesTax", SalesTax));
            parameters.Add(new SqlParameter("DueDate", DueDate));
            parameters.Add(new SqlParameter("Notes", InputHelper.GetString(Notes)));
            parameters.Add(new SqlParameter("Summary", InputHelper.GetString(Summary)));
            parameters.Add(new SqlParameter("Paid", Paid));
            parameters.Add(new SqlParameter("DatePaid", Paid ? DatePaid : null));
            parameters.Add(new SqlParameter("PaymentID", PaymentID));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));
            parameters.Add(new SqlParameter("FailStamp", FailStamp));
            parameters.Add(new SqlParameter("EarlyRenewalStamp", EarlyRenewalStamp));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdateInvoice", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "Invoice", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public FunctionResult Delete()
        {
            if (Paid == false)
            {
                ErrorLogger.LogError("Invoice ID: " + ID, 0, 0, "Invoice DeletePermanent");

                DBAccess db = new DBAccess();
                db.ExecuteNonQuery("Invoice_Delete", new SqlParameter("id", ID));

                return new FunctionResult();
            }
            else
            {
                return new FunctionResult("Could not permanently delete invoice, its invoice has already been paid.");
            }
        }
    }
}
