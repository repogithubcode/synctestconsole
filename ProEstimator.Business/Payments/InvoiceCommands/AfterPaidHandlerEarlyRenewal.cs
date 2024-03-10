using System.Linq;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Payments.InvoiceCommands
{
    internal class AfterPaidHandlerEarlyRenewal : IProcessInvoice
    {
        public bool ShouldExecute(ProEstimatorData.DataModel.Contracts.Invoice invoice)
        {
            if (invoice.AddOnID == 0 && invoice.EarlyRenewal)
            {
                if(Invoice.GetForContract(invoice.ContractID).FirstOrDefault(o => o.ID != invoice.ID && o.Paid) == null)
                {
                    return true;
                }
            }
            return false;
        }

        public void Execute(ProEstimatorData.DataModel.Contracts.Invoice invoice)
        {
            ProEstimator.Business.Logic.EmailManager.SendContractEarlyRenewDone(invoice.LoginID, invoice.ContractID);
        }

    }
}
