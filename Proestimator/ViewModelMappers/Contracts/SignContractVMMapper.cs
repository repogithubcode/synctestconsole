using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Proestimator.ViewModel.Contracts;
using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.ViewModelMappers.Contracts
{
    public class SignContractVMMapper : IVMMapper<SignContractVM>
    {

        public SignContractVM Map(MappingConfiguration mappingConfiguration)
        {
            SignContractVMMapperConfiguration config = mappingConfiguration as SignContractVMMapperConfiguration;

            SignContractVM vm = new SignContractVM();

            // See if the passed ContractID belongs to the Login
            Contract contract = Contract.Get(config.ContractID);
            if (contract == null)
            {
                vm.ErrorMessage = @Proestimator.Resources.ProStrings.InvalidContractID;
                vm.ShowForm = false;
            }

            vm.LoginID = config.LoginID;
            vm.ContractID = config.ContractID;

            ContractDigitalSignature digitalSignature = new ContractDigitalSignature();
            digitalSignature.ContractID = config.ContractID;
            digitalSignature.LoginID = config.LoginID;

            vm.ContractText = digitalSignature.GetContractContent(false);

            return vm;
        }

    }

    public class SignContractVMMapperConfiguration : MappingConfiguration
    {
        public int LoginID { get; set; }
        public int ContractID { get; set; }
    }
}