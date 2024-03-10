using System.Collections.Generic;
using System.Linq;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Payments.InvoiceCommands
{
    internal class AfterPaidHandlerBundledAddOns : IProcessInvoice
    {
        public bool ShouldExecute(ProEstimatorData.DataModel.Contracts.Invoice invoice)
        {
            return invoice.AddOnID > 0;
        }

        public void Execute(ProEstimatorData.DataModel.Contracts.Invoice invoice)
        {
            // If the invoice is a first payment for a Bundle add on, delete any existing bundleable add ons. 
            ContractAddOn addOn = ContractAddOn.Get(invoice.AddOnID);
            if (addOn != null && addOn.AddOnType.ID == 12)
            {
                List<ContractAddOn> addOns = ContractAddOn.GetForContract(addOn.ContractID);
                addOns = addOns.Where(o => o.AddOnType.IsBundlable).ToList();

                foreach (ContractAddOn eachAddOn in addOns)
                {
                    eachAddOn.IsDeleted = true;
                    eachAddOn.Save();
                }
            }
        }

    }
}
