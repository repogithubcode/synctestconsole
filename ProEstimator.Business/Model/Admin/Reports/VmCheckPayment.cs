using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin.Reports
{
    public class VmCheckPayment : IModelMap<VmCheckPayment>
    {
        public string LoginID { get; set; }
        public string LoginName { get; set; }
        public string Organization { get; set; }
        public string AccountName { get; set; }
        public string AccountAddress { get; set; }
        public string BankName { get; set; }
        public string BankCity { get; set; }
        public string BankState { get; set; }
        public string BankZip { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string ContractType { get; set; }
        public string InvoiceID { get; set; }
        public string DueDate { get; set; }
        public string TermDescription { get; set; }
        public string PaymentAmount { get; set; }
        public string NumberOfPayments { get; set; }
        public string PaymentDate { get; set; }
        public string CheckNumber { get; set; }
        public string Notes { get; set; }
        public string SalesRep { get; set; }
        public string isAutoPay { get; set; }

        public VmCheckPayment ToModel(System.Data.DataRow row)
        {
            var model = new VmCheckPayment();

            return model;
        }
    }
}