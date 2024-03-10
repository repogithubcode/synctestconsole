using ProEstimator.Admin.ViewModel.Contracts;
using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.Admin.ViewModelMappers.Contracts
{
    public class InvoiceHistoryVMMapper : IVMMapper<InvoiceHistoryVM>
    {
        public InvoiceHistoryVM Map(MappingConfiguration mappingConfiguration)
        {
            InvoiceHistoryVMMapperConfiguration config = mappingConfiguration as InvoiceHistoryVMMapperConfiguration;
            ContractAddOnHistory history = config.History;

            InvoiceHistoryVM vm = new InvoiceHistoryVM();
            vm.ID = history.ID;
            vm.ContractAddOnID = history.ContractAddOnID;
            vm.InvoiceID = history.InvoiceID;
            vm.TimeStamp = history.TimeStamp;
            int diff = history.EndQuantity - history.StartQuantity;
            vm.Action = diff > 0 ? "ADD" : "REMOVE";
            vm.Amount = history.Amount;
            vm.SalesTax = history.SalesTax;
            vm.Quantity = diff > 0 ? diff : diff * -1;

            return vm;
        }
    }

    public class InvoiceHistoryVMMapperConfiguration : MappingConfiguration
    {
        public ContractAddOnHistory History { get; set; }
    }
}