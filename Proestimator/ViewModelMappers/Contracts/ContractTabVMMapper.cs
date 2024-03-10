using Proestimator.ViewModel.Contracts;
using ProEstimator.Business.Logic;
using ProEstimatorData.DataModel.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModelMappers.Contracts
{
    public class ContractTabVMMapper : IVMMapper<ContractTabVM>
    {

        public ContractTabVM Map(MappingConfiguration mappingConfiguration)
        {
            ContractTabVMMapperConfiguration config = mappingConfiguration as ContractTabVMMapperConfiguration;

            ContractTabVM vm = new ContractTabVM();

            vm.LoginID = config.LoginID;

            // Get all contracts including old and future ones
            List<Contract> allContracts = Contract.GetAllForLogin(config.LoginID);

            vm.HasContractHistory = allContracts.Where(o => o.HasPayment && o.ExpirationDate < DateTime.Now).ToList().Count > 1;

            Contract activeContract = Contract.GetActive(allContracts);
            if (activeContract != null)
            {
                vm.HasActiveContract = true;

                ContractVMMapper contractMapper = new ContractVMMapper();
                vm.CurrentContract = contractMapper.Map(new ViewModelMappers.Contracts.ContractVMMapperConfiguration() { Contract = activeContract });

                // If the active contract will exire soon, let the user renew
                if (activeContract.DaysUntilExpiration <= ContractManager.MaxRenewalWindow && allContracts.FirstOrDefault(o => o.EffectiveDate.Date >= activeContract.ExpirationDate.Date) == null)
                {
                    vm.ShowContractRenewalButton = true;
                    vm.ContractExpirationDays = activeContract.DaysUntilExpiration;
                }

                // If there is an already paid for contract in the future
                Contract futureContract = allContracts.FirstOrDefault(o => o.HasPayment && o.EffectiveDate >= activeContract.ExpirationDate);
                if (futureContract != null)
                {
                    vm.FutureContract = contractMapper.Map(new ViewModelMappers.Contracts.ContractVMMapperConfiguration() { Contract = futureContract });
                    vm.HasFutureContract = true;
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
            }

            // If there is an in progress contract show the button to change the contract
            Contract inProgressContract = Contract.GetInProgress(config.LoginID);
            if (inProgressContract != null)
            {
                vm.HasActiveContract = true;
                vm.HasInProgressContract = true;

                ContractVMMapper contractMapper = new ContractVMMapper();
                vm.InProgressContract = contractMapper.Map(new ViewModelMappers.Contracts.ContractVMMapperConfiguration() { Contract = inProgressContract });
            }

            // Get the current invoice total
            InvoiceVMMapper invoiceMapper = new InvoiceVMMapper();

            List<Invoice> dueInvoices = ContractManager.GetInvoicesToBePaid(config.LoginID);
            decimal invoiceTotal = 0;
            foreach (Invoice invoice in dueInvoices)
            {
                vm.DueInvoices.Add(invoiceMapper.Map(new InvoiceVMMapperConfiguration() { Invoice = invoice }));

                invoiceTotal += invoice.InvoiceTotal;
            }

            if (invoiceTotal > 0)
            {
                vm.InvoiceAmount = invoiceTotal;
                vm.NeedsPayment = true;
            }

            // If there are no contracts, check if there is a trial and set up the trial expiration message
            if (!vm.HasActiveContract)
            {
                Trial trial = Trial.GetActive(config.LoginID);
                if (trial != null && trial.EndDate >= DateTime.Now.Date)
                {
                    vm.TrialExpirationMessage = string.Format(@Proestimator.Resources.ProStrings.TrialWillExpireOn, trial.EndDate.ToShortDateString());
                    vm.HasActiveTrial = true;
                }
            }

            return vm;
        }
    }

    public class ContractTabVMMapperConfiguration : MappingConfiguration
    {
        public int LoginID { get; set; }
    }
}