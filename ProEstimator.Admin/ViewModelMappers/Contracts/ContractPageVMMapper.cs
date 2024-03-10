using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Admin.ViewModel.Contracts;
using ProEstimatorData;

namespace Proestimator.Admin.ViewModelMappers.Contracts
{
    public class ContractPageVMMapper : IVMMapper<ContractPageVM>
    {
        public ContractPageVM Map(MappingConfiguration mappingConfiguration)
        {
            ContractPageVMMapperConfiguration config = mappingConfiguration as ContractPageVMMapperConfiguration;
            int loginID = config.LoginID;

            ContractPageVM vm = new ContractPageVM(); 
            vm.GoodData = false;

            List<int> priceLevels = new List<int>();
            priceLevels.Add(8);
            for (int i = -1; i <= 7; i++)
            {
                priceLevels.Add(i);
            }
            priceLevels.Add(201);
            priceLevels.Add(501);

            vm.ContractPriceLevels = new SelectList(priceLevels);

            vm.SelectedContractPriceLevel = 8;

            List<SelectListItem> addOnTypes = new List<SelectListItem>();
            addOnTypes.Add(new SelectListItem() { Value = "2", Text = "Frame Data" });
            addOnTypes.Add(new SelectListItem() { Value = "5", Text = "EMS" });
            addOnTypes.Add(new SelectListItem() { Value = "8", Text = "Multi User" });
            addOnTypes.Add(new SelectListItem() { Value = "9", Text = "QB Export" });
            addOnTypes.Add(new SelectListItem() { Value = "10", Text = "Pro Advisor" });
            addOnTypes.Add(new SelectListItem() { Value = "11", Text = "Image Editing" });
            addOnTypes.Add(new SelectListItem() { Value = "13", Text = "Custom Reports" });
            addOnTypes.Add(new SelectListItem() { Value = "12", Text = "Bundle" });

            vm.AddOnTypes = new SelectList(addOnTypes, "Value", "Text");

            List<int> qtys = new List<int>();
            for (int i = 1; i <= 10; i++)
            {
                qtys.Add(i);
            }

            vm.AddOnQtys = new SelectList(qtys);
            vm.SelectedAddOnQty = 1;

            // Load organization info
            LoginInfo loginInfo = LoginInfo.GetByID(loginID);
            if (loginInfo != null)
            {
                vm.GoodData = true;
                vm.Organizationname = loginInfo.Organization;
                vm.LoginID = loginID;

                SalesRep salesRep = SalesRep.Get(loginInfo.SalesRepID);
                if (salesRep != null)
                {
                    vm.SalesRep = salesRep.SalesRepID + " - " + salesRep.FirstName + " " + salesRep.LastName;
                }

                Contact contact = Contact.GetContactForLogins(loginID);
                vm.CustomerName = contact.FirstName + " " + contact.LastName;

                // Load all contracts
                vm.Contracts = new List<ContractVM>();

                ContractVMMapper mapper = new ContractVMMapper();

                List<Contract> allContracts = null;
                // config.InvoiceID > 0
                if (config.InvoiceID > 0)
                {
                    allContracts = new List<Contract>();
                    Invoice invoice = Invoice.Get(config.InvoiceID);
                    Contract contract = Contract.Get(invoice.ContractID);
                    vm.ContractID = contract.ID;
                    vm.InvoiceID = invoice.ID;
                    allContracts.Add(contract);
                }

                // config.ContractID > 0
                if (config.ContractID > 0)
                {
                    allContracts = new List<Contract>();
                    Contract contract = Contract.Get(config.ContractID);
                    vm.ContractID = contract.ID;
                    allContracts.Add(contract);
                }

                // config.LoginID > 0
                if (config.LoginID > 0 && config.ContractID <= 0 && config.InvoiceID <= 0)
                {
                    allContracts = Contract.GetAllForLogin(config.LoginID);
                }

                foreach (Contract contract in allContracts)
                {
                    vm.Contracts.Add(mapper.Map(new ContractVMMapperConfiguration() { Contract = contract }));
                }

                // Get the default start date for a new contract, use the expiration date +1 day of the last active contract
                DateTime startDate = DateTime.Now;

                try
                {
                    Contract lastContract = allContracts.Where(o => o.Active && !o.IsDeleted).OrderByDescending(o => o.ExpirationDate).FirstOrDefault();
                    if (lastContract != null)
                    {
                        startDate = lastContract.ExpirationDate.AddDays(1);
                    }
                }
                catch { }

                vm.NewContractStartDate = startDate.ToShortDateString();

                // See if auto pay is on
                StripeInfo stripeInfo = StripeInfo.GetForLogin(loginID);
                if (stripeInfo != null && stripeInfo.AutoPay)
                {
                    vm.AutoPay = true;
                }
            }

            return vm;
        }
    }

    public class ContractPageVMMapperConfiguration : MappingConfiguration
    {
        public int LoginID { get; set; }
        public int ContractID { get; set; }
        public int InvoiceID { get; set; }

    }
}