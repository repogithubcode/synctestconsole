using System.Data;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin.Reports
{
    public class VmAutoPayment : IModelMap<VmAutoPayment>
    {
        public string LoginID { get; set; }
        public string LoginName { get; set; }
        public string Organization { get; set; }
        public string ContractType { get; set; }
        public string InvoiceID { get; set; }
        public string DueDate { get; set; }
        public string TermDescription { get; set; }
        public string InvoiceAmount { get; set; }
        public string NumberOfPayments { get; set; }
        public string Notes { get; set; }
        public string SalesRep { get; set; }
        public string isAutoPay { get; set; }
        public VmAutoPayment ToModel(DataRow row)
        {
            var model = new VmAutoPayment();

            return model;
        }
    }
}