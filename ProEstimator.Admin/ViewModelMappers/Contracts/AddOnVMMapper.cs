using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Admin.ViewModel.Contracts;

namespace Proestimator.Admin.ViewModelMappers.Contracts
{
    public class AddOnVMMapper : IVMMapper<AddOnVM>
    {
        public AddOnVM Map(MappingConfiguration mappingConfiguration)
        {
            AddOnVMMapperConfiguration config = mappingConfiguration as AddOnVMMapperConfiguration;
            ContractAddOn addon = config.AddOn;

            AddOnVM vm = new AddOnVM();

            if (addon != null)
            {
                vm.ID = addon.ID;
                vm.ContractPriceLevelID = addon.PriceLevel.ID;
                vm.ContractType = addon.AddOnType.Type;
                vm.Description = addon.AddOnType.Description;
                vm.TermDescription = addon.PriceLevel.ContractTerms.TermDescription;
                vm.StartDate = addon.StartDate.ToShortDateString();
                vm.IsActive = addon.Active;
                vm.IsDeleted = addon.IsDeleted;
                vm.Quantity = addon.Quantity;
                vm.HasPayment = addon.HasPayment;
            }

            vm.ErrorMessage = "";
            
            return vm;
        }

        
    }

    public class AddOnVMMapperConfiguration : MappingConfiguration
    {
        public ContractAddOn AddOn { get; set; }
        
    }
}