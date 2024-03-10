
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Proestimator.ViewModel.Contracts;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Business.Logic;

namespace Proestimator.ViewModelMappers.Contracts
{
    public class AddOnConfirmVMMapper : IVMMapper<AddOnConfirmVM>
    {
        public AddOnConfirmVM Map(MappingConfiguration configuration)
        {
            AddOnConfirmVMMapperConfiguration config = configuration as AddOnConfirmVMMapperConfiguration;

            AddOnConfirmVM vm = new AddOnConfirmVM();

            vm.AddOnID = config.AddOn.ID;
            vm.ContractType = config.AddOn.AddOnType.Type;
            vm.TermDescription = config.AddOn.PriceLevel.ContractTerms.TermDescription;
            vm.Quantity = config.AddOn.Quantity;
            vm.Invoices = new List<InvoiceVM>();

            InvoiceVMMapper invoiceMapper = new InvoiceVMMapper();

            List<Invoice> invoices = ContractManager.InvoiceDeletable(config.AddOn.ID);
            foreach (Invoice invoice in invoices)
            {
                if (vm.Invoices.FirstOrDefault(o => o.InvoiceID == invoice.ID) == null)
                {
                    vm.Invoices.Add(invoiceMapper.Map(new InvoiceVMMapperConfiguration() { Invoice = invoice }));
                }
            }

            return vm;
        }
    }

    public class AddOnConfirmVMMapperConfiguration : MappingConfiguration
    {
        public ContractAddOn AddOn { get; set; }
    }
}