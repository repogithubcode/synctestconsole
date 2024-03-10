using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin.Reports
{
    public class VmCreditCardPayment : IModelMap<VmCreditCardPayment>
    {
        public int LoginID { get; set; }
        public string LoginName { get; set; }
        public string Organization { get; set; }
        public string CCNameOnCard { get; set; }
        public string CCAddress { get; set; }
        public string CC_City { get; set; }
        public string CC_State { get; set; }
        public string CCZip { get; set; }
        public string CCType { get; set; }
        public string CCNumber { get; set; }
        public string CCExpirationDate { get; set; }
        public string CCSecurityCode { get; set; }
        public string ContractType { get; set; }
        public int InvoiceID { get; set; }
        public string DueDate { get; set; }
        public string TermDescription { get; set; }
        public decimal PaymentAmount { get; set; }
        public string NumberOfPayments { get; set; }
        public string PaymentDate { get; set; }
        public string WorkPhone { get; set; }
        public string Emailaddress { get; set; }
        public string PaymentTerms { get; set; }
        public string MethodOfNotification { get; set; }
        public string Tax { get; set; }
        public string TaxCode { get; set; }
        public string CustomerType { get; set; }
        public string Notes { get; set; }
        public string SalesRep2 { get; set; }
        public bool? isAutoPay { get; set; }

        public VmCreditCardPayment ToModel(System.Data.DataRow row)
        {
            var model = new VmCreditCardPayment();
            model.LoginID = (int)row["LoginID"];
            model.LoginName = row["LoginName"].SafeString();
            model.Organization = row["Organization"].SafeString();
            model.CCNameOnCard = row["CCNameOnCard"].SafeString();
            model.CCAddress = row["CCAddress"].SafeString();
            //model.CC_City = row["CC_City"].SafeString();
            //model.CC_State = row["CC_State"].SafeString();
            model.CCZip = row["CCZip"].SafeString();
            model.CCType = row["CCType"].SafeString();
            model.CCNumber = row["CCNumber"].SafeString();
            model.CCExpirationDate = row["CCExpirationDate"].SafeString();
            model.CCSecurityCode = row["CCSecurityCode"].SafeString();
            model.ContractType = row["ContractType"].SafeString();
            model.InvoiceID = (int)row["InvoiceID"];
            model.DueDate = row["DueDate"].SafeString();
            //model.TermDescription = row["TermDescription"].SafeString();
            model.PaymentAmount = (decimal)row["PaymentAmount"];
            //model.NumberOfPayments = row["NumberOfPayments"].SafeString();
            model.PaymentDate = row["PaymentDate"].SafeString();
            //model.WorkPhone = row["WorkPhone"].SafeString();
            //model.Emailaddress = row["Emailaddress"].SafeString();
            //model.PaymentTerms = row["PaymentTerms"].SafeString();
            //model.MethodOfNotification = row["MethodOfNotification"].SafeString();
            //model.Tax = row["Tax"].SafeString();
            //model.TaxCode = row["TaxCode"].SafeString();
            //model.CustomerType = row["CustomerType"].SafeString();
            model.Notes = row["Notes"].SafeString();
            //model.SalesRep2 = row["SalesRep2"].SafeString();
            model.isAutoPay = row["isAutoPay"].SafeBool();

            return model;
        }
    }
}