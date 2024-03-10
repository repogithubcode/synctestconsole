using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Proestimator.ViewModel.Contracts;
using ProEstimator.Business.Logic;
using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.ViewModelMappers.Contracts
{
    public class PickContractVMMapper : IVMMapper<PickContractVM>
    {
        public PickContractVM Map(MappingConfiguration mappingConfiguration)
        {
            PickContractVMMapperConfiguration config = mappingConfiguration as PickContractVMMapperConfiguration;

            PickContractVM vm = new PickContractVM();

            Contract activeContract = Contract.GetActive(config.LoginID);

            // Cannot pick a new contract if there is an active one not yet in the renewal window
            if (activeContract != null && activeContract.DaysUntilExpiration > ContractManager.MaxRenewalWindow)
            {
                vm.CanPickContract = false;
            }
            else
            {
                vm.CanPickContract = true;
            }

            // If there is a current active contract, show a renewal message
            if (activeContract != null)
            {
                vm.IsRenewal = true;
                if (activeContract.DaysUntilExpiration > 0)
                {
                    vm.RenewalMessage = string.Format(Proestimator.Resources.ProStrings.CurrentContractWillExpireOn, activeContract.ContractPriceLevel.ContractTerms.TermDescription, activeContract.ExpirationDate.ToShortDateString());
                }
                else
                {
                    vm.RenewalMessage = string.Format(Proestimator.Resources.ProStrings.ContractExpiredOn, activeContract.ContractPriceLevel.ContractTerms.TermDescription, activeContract.ExpirationDate.ToShortDateString());
                }
            }
            else
            {
                // If there is no active contract, see if there is an expired contract or trial and show a message about when it expired
                DateTime mostRecentExpiration = DateTime.MinValue;
                string expiredMessage = "";

                foreach (Trial trial in Trial.GetForLogin(config.LoginID))
                {
                    if (trial.EndDate < DateTime.Now.Date && trial.EndDate > mostRecentExpiration)
                    {
                        mostRecentExpiration = trial.EndDate.Date;
                        expiredMessage = string.Format(Proestimator.Resources.ProStrings.TrialExpiredOn, trial.EndDate.ToShortDateString());
                    }
                }

                foreach (Contract contract in Contract.GetAllForLogin(config.LoginID))
                {
                    if (contract.ExpirationDate < DateTime.Now.Date && contract.ExpirationDate > mostRecentExpiration)
                    {
                        mostRecentExpiration = contract.ExpirationDate;
                        expiredMessage = string.Format(Proestimator.Resources.ProStrings.ContractExpiredOn, contract.ContractPriceLevel.ContractTerms.TermDescription, contract.ExpirationDate.ToShortDateString());
                    }
                }

                Trial activeTrial = Trial.GetActive(config.LoginID);
                if (activeTrial != null && activeTrial.EndDate >= DateTime.Now.Date)
                {
                    mostRecentExpiration = activeTrial.EndDate;
                    expiredMessage = string.Format(@Proestimator.Resources.ProStrings.TrialWillExpireOn, activeTrial.EndDate.ToShortDateString());        
                }

                if (!string.IsNullOrEmpty(expiredMessage) && mostRecentExpiration > DateTime.Now.AddYears(-1))
                {
                    vm.RecentExpirationMessage = expiredMessage;
                }
            }

            // Load info about an in progress contract
            Contract inProgressContract = Contract.GetInProgress(config.LoginID);
            if (inProgressContract != null)
            {
                vm.InProgressContractExists = true;
                vm.InProgressContractSummary = inProgressContract.ContractPriceLevel.ContractTerms.TermDescription;
                vm.InProgressContractID = inProgressContract.ID;
            }

            vm.LoginID = config.LoginID;

            return vm;
        }
    }

    public class PickContractVMMapperConfiguration : MappingConfiguration
    {
        public int LoginID { get; set; }
    }
}