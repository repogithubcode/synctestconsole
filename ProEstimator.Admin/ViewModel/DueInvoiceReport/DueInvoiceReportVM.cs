using ProEstimatorData.DataModel;
using ProEstimatorData;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel.DueInvoiceReport
{
    public class DueInvoiceReportVM
    {
        public int ID { get; set; }
        public int InvoiceID { get; set; }
        public string InvoiceSummary { get; set; }
        public double InvoiceAmount { get; set; }
        public int LoginID { get; set; }
        public string DueDate { get; set; }
        public string Paid { get; set; }
        public string AutoPay { get; set; }
        public string HasCard { get; set; }
        public string CardError { get; set; }
        public string ErrorMessage { get; set; }
        public string ContractID { get; set; }

        public DueInvoiceReportVM() { }

        public DueInvoiceReportVM(DueInvoice dueInvoice)
        {
            ID = InputHelper.GetInteger(dueInvoice.ID.ToString());
            InvoiceID = InputHelper.GetInteger(dueInvoice.InvoiceID.ToString());
            InvoiceSummary = InputHelper.GetString(dueInvoice.InvoiceSummary.ToString());
            InvoiceAmount = InputHelper.GetDouble(dueInvoice.InvoiceAmount.ToString());
            LoginID = InputHelper.GetInteger(dueInvoice.LoginID.ToString());
            DueDate = InputHelper.GetString(dueInvoice.DueDate.ToString());

            Paid = InputHelper.GetString(dueInvoice.Paid.ToString()); 
            AutoPay = InputHelper.GetString(dueInvoice.AutoPay.ToString()); 

            HasCard = InputHelper.GetString(dueInvoice.HasCard.ToString());
            CardError = InputHelper.GetString(dueInvoice.CardError.ToString()); 
            ErrorMessage = InputHelper.GetString(dueInvoice.ErrorMessage.ToString());
            ContractID = InputHelper.GetString(dueInvoice.ContractID.ToString());
        }
    }
}