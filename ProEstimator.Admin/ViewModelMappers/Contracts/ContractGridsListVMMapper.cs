using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Admin.ViewModel.Contracts;

namespace Proestimator.Admin.ViewModelMappers.Contracts
{
    public class ContractGridsListVMMapper : IVMMapper<ContractGridsListVM>
    {
        public ContractGridsListVM Map(MappingConfiguration mappingConfiguration)
        {
            ContractGridsListVMMapperConfiguration config = mappingConfiguration as ContractGridsListVMMapperConfiguration;
            
            bool showDeleted = config.ShowDeleted;
            bool showAddOnDeleted = config.ShowAddOnDeleted;
            bool showAddOnTrialDeleted = config.ShowAddOnTrialDeleted;

            ContractGridsListVM vm = new ContractGridsListVM();

            Contract contract = Contract.Get(config.ContractID);
            
            if (contract != null)
            {
                List<ContractAddOn> addOns = ContractAddOn.GetForContract(contract.ID, showAddOnDeleted);

                if (config.AddOn)
                {
                    vm.AddOns = new List<AddOnVM>();

                    AddOnVMMapper mapper = new AddOnVMMapper();
                    foreach (ContractAddOn addOn in addOns)
                    {
                        vm.AddOns.Add(mapper.Map(new AddOnVMMapperConfiguration() { AddOn = addOn }));
                    }                  
                }
                if (config.AddOnTrial)
                {
                    vm.AddOnTrials = new List<AddOnTrialVM>();

                    List<ContractAddOnTrial> addOnTrials = ContractAddOnTrial.GetForContract(contract.ID, showAddOnTrialDeleted);
                    foreach (ContractAddOnTrial addOnTrial in addOnTrials)
                    {
                        vm.AddOnTrials.Add(new AddOnTrialVM(addOnTrial));
                    }                 
                }
                if (config.Invoice)
                {
                    vm.MainInvoices = new List<InvoiceGridVM>();
                    List<Invoice> baseInvoices = Invoice.GetForContractForAdmin(contract.ID, false, showDeleted).Where(o => o.InvoiceType.ID != 3).ToList();

                    foreach (Invoice invoice in baseInvoices.OrderBy(o => o.DueDate))
                    {
                        vm.MainInvoices.Add(new InvoiceGridVM(invoice));
                    }

                    List<Invoice> invoices = Invoice.GetForContractForAdmin(contract.ID, true, showDeleted, showAddOnDeleted).Where(o => o.InvoiceType.ID != 3).ToList();  // Invoice Type 3 is Custom, those are showing in their own grid
                    
                    vm.FrameInvoices = GetInvoices(invoices, addOns, 2);
                    vm.EmsInvoices = GetInvoices(invoices, addOns, 5);
                    vm.MultiUserInvoices = GetInvoices(invoices, addOns, 8);
                    vm.QbInvoices = GetInvoices(invoices, addOns, 9);
                    vm.AdvisorInvoices = GetInvoices(invoices, addOns, 10);
                    vm.ImageInvoices = GetInvoices(invoices, addOns, 11);
                    vm.ReportsInvoices = GetInvoices(invoices, addOns, 13);
                    vm.BundleInvoices = GetInvoices(invoices, addOns, 12);

                    vm.CustomInvoices = new List<InvoiceGridVM>();
                    List<Invoice> customInvoices = Invoice.GetForContractForAdmin(contract.ID, true, showDeleted, showAddOnDeleted).Where(o => o.InvoiceType.ID == 3).ToList();  // Invoice Type 3 is Custom

                    foreach (Invoice invoice in customInvoices.OrderBy(o => o.DueDate))
                    {
                        vm.CustomInvoices.Add(new InvoiceGridVM(invoice));
                    }
                }
            }
            else
            {
                return null;
            }

            return vm;
        }

        private List<InvoiceGridVM> GetInvoices(List<Invoice> invoices, List<ContractAddOn> addOns, int typeID)
        {
            List<InvoiceGridVM> invoiceList = new List<InvoiceGridVM>();
            foreach (ContractAddOn addOn in addOns.Where(o => o.AddOnType.ID == typeID))
            {
                foreach (Invoice invoice in invoices.Where(o => o.AddOnID == addOn.ID).OrderBy(o => o.DueDate))
                {
                    invoiceList.Add(new InvoiceGridVM(invoice));
                }
            }
            return invoiceList;
        }
    }

    public class ContractGridsListVMMapperConfiguration : MappingConfiguration
    {
        public int ContractID { get; set; }
        public bool ShowDeleted { get; set; }
        public bool ShowAddOnDeleted { get; set; }
        public bool ShowAddOnTrialDeleted { get; set; }
        public bool AddOn { get; set; }
        public bool AddOnTrial { get; set; }
        public bool Invoice { get; set; }
    }
}