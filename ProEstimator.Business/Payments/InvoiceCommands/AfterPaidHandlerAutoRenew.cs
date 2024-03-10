using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Payments.InvoiceCommands
{
    internal class AfterPaidHandlerAutoRenew : IProcessInvoice
    {
        public bool ShouldExecute(ProEstimatorData.DataModel.Contracts.Invoice invoice)
        {
            Contract contract = Contract.Get(invoice.ContractID);
            return contract.AutoRenew && invoice.AddOnID == 0 && invoice.PaymentNumber == 0;
        }

        public void Execute(ProEstimatorData.DataModel.Contracts.Invoice invoice)
        {
            ProEstimator.Business.Logic.EmailManager.SendContractAutoRenewDone(invoice.LoginID, invoice.ContractID);
        }

    }
}
