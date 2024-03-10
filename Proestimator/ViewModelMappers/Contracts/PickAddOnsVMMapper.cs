using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Proestimator.ViewModel.Contracts;
using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.ViewModelMappers.Contracts
{
    public class PickAddOnsVMMapper : IVMMapper<PickAddOnsVM>
    {
        public PickAddOnsVM Map(MappingConfiguration mappingConfiguration)
        {
            PickAddOnsVMMapperConfiguration config = mappingConfiguration as PickAddOnsVMMapperConfiguration;

            PickAddOnsVM vm = new PickAddOnsVM();

            List<ContractAddOn> addOns = ContractAddOn.GetForContract(config.ContractID);

            AddOnDetailsVMMapper mapper = new AddOnDetailsVMMapper();

            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 12, AddOns = addOns }));  // Bundle
            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 9, AddOns = addOns, IsBundleable = true }));    // QB
            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 10, AddOns = addOns, IsBundleable = true }));   // Pro Advisor
            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 11, AddOns = addOns, IsBundleable = true }));   // Images
            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 13, AddOns = addOns, IsBundleable = true }));   // Custom Reports

            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 2, AddOns = addOns }));   // Frame Data
            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 5, AddOns = addOns }));   // EMS
            int qty = 0;
            int priceLevel = 0;
            addOns.Where(o => o.AddOnType.ID == 8 && o.Active).ToList().ForEach(o => { qty += o.Quantity; priceLevel = o.PriceLevel.ID; });
            if (qty == 0) { qty = 1; }
            if (qty > 10) { qty = 10; }
            Contract contract = Contract.Get(config.ContractID);
            Contract activeContract = Contract.GetActive(contract.LoginID);
            if (activeContract != null && contract.ID == activeContract.ID)
            {
                qty = 0;
            }
            vm.AddOnDetails.Add(mapper.MultiMap(new MultiAddOnDetailsVMMapperConfiguration() { AddOnTypeID = 8, AddOns = addOns, IsMultiAdd = true, Qty = 10, SelectedQty = qty, SelectedID = priceLevel }));   // Multi User
                       
            ContractAddOn bundleAddOn = addOns.FirstOrDefault(o => o.AddOnType == ContractType.Get(12));
            if (bundleAddOn != null && bundleAddOn.HasPayment)
            {
                vm.HasBundle = true;
            }

            vm.GoodData = true;
            vm.ContractID = config.ContractID;
            
            return vm;
        }
    }


    public class PickAddOnsVMMapperConfiguration : MappingConfiguration
    {
        public int ContractID { get; set; }
    }
}