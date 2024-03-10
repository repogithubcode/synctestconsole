namespace ProEstimator.Business.Payments.InvoiceCommands
{
    internal class AfterPaidHanlderFirstAddOnPaymentEmail : IProcessInvoice
    {
        public bool ShouldExecute(ProEstimatorData.DataModel.Contracts.Invoice invoice)
        {
            return invoice.PaymentNumber == 1 && invoice.AddOnID > 0;
        }

        public void Execute(ProEstimatorData.DataModel.Contracts.Invoice invoice)
        {
            ProEstimator.Business.Logic.EmailManager.SendFirstAddOnPaymentEmail(invoice.AddOnID, invoice.LoginID, invoice.ID);
        }
        
    }
}
