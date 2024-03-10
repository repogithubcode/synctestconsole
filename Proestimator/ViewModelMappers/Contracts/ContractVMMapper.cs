using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Proestimator.ViewModel.Contracts;
using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.ViewModelMappers.Contracts
{
    public class ContractVMMapper : IVMMapper<ContractVM>
    {
        public ContractVM Map(MappingConfiguration mappingConfiguration)
        {
            ContractVMMapperConfiguration config = mappingConfiguration as ContractVMMapperConfiguration;
            Contract contract = config.Contract;

            ContractVM vm = new ContractVM();

            vm.ContractID = contract.ID;
            vm.TermDescription = contract.ContractPriceLevel.ContractTerms.TermDescription;
            vm.EffectiveDate = contract.EffectiveDate;
            vm.ExpirationDate = contract.ExpirationDate;

            ContractDigitalSignature digitalSignature = ContractDigitalSignature.GetForContract(contract.ID);
            if (digitalSignature != null)
            {
                vm.DigitalSignatureID = digitalSignature.ID;
            }

            List<Invoice> allInvoices = Invoice.GetForContract(contract.ID, true);

            List<ContractAddOn> addOns = ContractAddOn.GetForContract(contract.ID);
            bool hasBundleAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 12) != null;

            AddOnDetailsVMMapper mapper = new AddOnDetailsVMMapper();

            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 2, AddOns = addOns, Invoices = allInvoices }));   // Frame Data
            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 5, AddOns = addOns, Invoices = allInvoices }));   // EMS
            vm.AddOnDetails.Add(mapper.MultiMap(new MultiAddOnDetailsVMMapperConfiguration() { AddOnTypeID = 8, AddOns = addOns, Invoices = allInvoices, IsMultiAdd = true, Qty = 10, SelectedQty = 1 }));   // Multi User
            
            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 12, AddOns = addOns, Invoices = allInvoices }));  // Bundle
            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 9, AddOns = addOns, IsBundleable = true, Invoices = allInvoices }));    // QB
            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 10, AddOns = addOns, IsBundleable = true, Invoices = allInvoices }));   // Pro Advisor
            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 11, AddOns = addOns, IsBundleable = true, Invoices = allInvoices }));   // Images
            vm.AddOnDetails.Add(mapper.Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = 13, AddOns = addOns, IsBundleable = true, Invoices = allInvoices }));   // Custom Reports

            vm.IsActive = contract.Active && contract.HasPayment;

            return vm;
        }

        
    }

    public class ContractVMMapperConfiguration : MappingConfiguration
    {
        public Contract Contract { get; set; }
        
    }
}