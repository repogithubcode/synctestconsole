using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Proestimator.ViewModel;
using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.ViewModelMappers.Contracts
{
    public class ContractHistoryTabVMMapper : IVMMapper<ContractHistoryTabVM>
    {

        public ContractHistoryTabVM Map(MappingConfiguration mappingConfiguration)
        {
            ContractHistoryTabVMMapperConfiguration config = mappingConfiguration as ContractHistoryTabVMMapperConfiguration;

            ContractHistoryTabVM vm = new ContractHistoryTabVM();
            vm.LoginID = config.LoginID;

            List<Contract> contracts = Contract.GetAllForLogin(config.LoginID);

            ContractVMMapper mapper = new ContractVMMapper();

            foreach (Contract contract in contracts)
            {
                vm.Contracts.Add(mapper.Map(new ContractVMMapperConfiguration() { Contract = contract }));
            }

            return vm;
        }

    }

    public class ContractHistoryTabVMMapperConfiguration : MappingConfiguration
    {
        public int LoginID { get; set; }
    }
}