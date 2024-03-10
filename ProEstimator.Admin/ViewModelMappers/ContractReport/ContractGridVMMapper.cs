using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Admin.ViewModel.ContractReport;
using ProEstimator.Business.Logic;

namespace Proestimator.Admin.ViewModelMappers.ContractReport
{
    public class ContractGridVMMapper : IVMMapper<ContractGridVM>
    {
        public ContractGridVM Map(MappingConfiguration mappingConfiguration)
        {
            ContractGridVMMapperConfiguration config = mappingConfiguration as ContractGridVMMapperConfiguration;
            Contract contract = config.Contract;

            ContractGridVM vm = new ContractGridVM();
            vm.ID = contract.ID;
            vm.LoginID = contract.LoginID;
            vm.ContractTermDescription = contract.ContractPriceLevel.ContractTerms.TermDescription;
            vm.EffectiveDate = contract.EffectiveDate;
            vm.SalesRep = contract.CompanyContactName.Trim();
            vm.Notes = contract.Notes;
            vm.Active = contract.Active;
            vm.IsDeleted = contract.IsDeleted;
            vm.DateCreated = contract.DateCreated;
            vm.IsRenewal = ContractManager.IsContractRenewal(contract);

            return vm;
        }
    }

    public class ContractGridVMMapperConfiguration : MappingConfiguration
    {
        public Contract Contract { get; set; }
    }
}