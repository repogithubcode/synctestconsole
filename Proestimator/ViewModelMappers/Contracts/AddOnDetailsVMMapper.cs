using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Proestimator.ViewModel.Contracts;
using ProEstimatorData.DataModel.Contracts;
using ProEstimatorData;
using ProEstimator.Business.Logic;

namespace Proestimator.ViewModelMappers.Contracts
{
    public class AddOnDetailsVMMapper : IVMMapper<AddOnDetailsVM>
    {

        public AddOnDetailsVM Map(MappingConfiguration configuration)
        {
            AddOnDetailsVMMapperConfiguration config = configuration as AddOnDetailsVMMapperConfiguration;

            AddOnDetailsVM vm = new AddOnDetailsVM();

            vm.AddOnTypeID = config.AddOnTypeID;
            vm.AddOnType = ContractType.Get(config.AddOnTypeID).Type;
            vm.IsBundleable = config.IsBundleable;
            vm.IsBundled = config.IsBundleable && config.AddOns.FirstOrDefault(o => o.AddOnType.ID == 12) != null;
            vm.IsMultiAdd = config.IsMultiAdd;

            ContractAddOn addOn = config.AddOns.FirstOrDefault(o => o.AddOnType.ID == config.AddOnTypeID);
            if(config.IsMultiAdd)
            {
                addOn = config.AddOn;
            }
            if (addOn != null)
            {
                decimal paymentAmountOverride = 0;

                if (config.Invoices != null && config.Invoices.Count > 0)
                {
                    if (config.IsMultiAdd)
                    {
                        paymentAmountOverride = GetFirstPaidAddOnInvoiceActualPrice(addOn, config.Invoices);
                    }
                    else
                    {
                        paymentAmountOverride = GetFirstPaidAddOnInvoiceActualPrice(config.AddOns, config.Invoices, config.AddOnTypeID);
                    }
                    if (vm.IsMultiAdd)
                    {
                        vm.NeedsPayment = ContractManager.InvoiceDeletable(addOn.ID).FirstOrDefault(o => o.Paid == false) != null;
                    }
                    else
                    {
                        vm.NeedsPayment = NeedsPayment(addOn, config.Invoices);
                    }
                }
                
                vm.HasAddOn = true;
                vm.HasPayment = addOn.HasPayment;
                vm.TermDescription = (config.IsMultiAdd ? addOn.Quantity.ToString() + (addOn.Quantity > 1 ? " AddOns x " : " AddOn x ") : "") + addOn.PriceLevel.ContractTerms.NumberOfPayments + " x " + ((paymentAmountOverride > 0) ? paymentAmountOverride.ToString("C") : addOn.PriceLevel.PaymentAmount.ToString("C"));
                vm.AddOnID = addOn.ID;
                vm.SelectedID = addOn.PriceLevel.ID;
            }

            return vm;
        }

        public AddOnDetailsVM MultiMap(MappingConfiguration configuration)
        {
            MultiAddOnDetailsVMMapperConfiguration config = configuration as MultiAddOnDetailsVMMapperConfiguration;

            AddOnDetailsVM vm = new AddOnDetailsVM();

            vm.AddOnTypeID = config.AddOnTypeID;
            vm.AddOnType = ContractType.Get(config.AddOnTypeID).Type;
            vm.IsBundleable = config.IsBundleable;
            vm.IsBundled = config.IsBundleable && config.AddOns.FirstOrDefault(o => o.AddOnType.ID == 12) != null;
            vm.IsMultiAdd = config.IsMultiAdd;
            vm.SelectedID = config.SelectedID;

            List<ContractAddOn> addOns = config.AddOns.Where(o => o.AddOnType.ID == config.AddOnTypeID).ToList();           
            if (addOns != null && addOns.Count > 0)
            {
                vm.HasAddOn = true;
                vm.AddOnDetails = new List<AddOnDetailsVM>();
                foreach (ContractAddOn addOn in addOns)
                {
                    vm.AddOnDetails.Add(Map(new AddOnDetailsVMMapperConfiguration() { AddOnTypeID = config.AddOnTypeID, AddOns = config.AddOns, AddOn = addOn, IsBundleable = config.IsBundleable, Invoices = config.Invoices, IsMultiAdd = config.IsMultiAdd }));
                }
            }

            List<int> qtys = new List<int>();
            for (int i = 1; i <= config.Qty; i++)
            {
                qtys.Add(i);
            }

            vm.AddOnQtys = new SelectList(qtys);
            vm.SelectedAddOnQty = config.SelectedQty;

            return vm;
        }

        private bool NeedsPayment(ContractAddOn addOn, List<Invoice> allInvoices)
        {
            if (addOn == null)
            {
                return true;
            }

            List<Invoice> addOnInvoices = allInvoices.Where(o => o.AddOnID == addOn.ID).ToList();
            if (addOnInvoices == null || addOnInvoices.Count == 0)
            {
                return false;
            }

            if (addOn.PriceLevel.PaymentAmount == 0)
            {
                return false;
            }

            if (addOnInvoices.FirstOrDefault(o => o.Paid) != null || addOnInvoices.Exists(o => o.Notes.Contains("from previous billing cycle")))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private decimal GetFirstPaidAddOnInvoiceActualPrice(List<ContractAddOn> addOns, List<Invoice> allInvoices, int addOnTypeID)
        {
            foreach (Invoice invoice in allInvoices)
            {
                if (invoice.AddOnID > 0 && invoice.Paid && (!invoice.Notes.ToLower().StartsWith("prorated") || allInvoices.Count == 1))
                {
                    ContractAddOn addOn = addOns.FirstOrDefault(o => o.ID == invoice.AddOnID);
                    if (addOn != null && addOn.AddOnType.ID == addOnTypeID)
                    {
                        return invoice.InvoiceTotal;
                    }
                }
            }

            return 0;
        }

        private decimal GetFirstPaidAddOnInvoiceActualPrice(ContractAddOn addOn, List<Invoice> allInvoices)
        {
            foreach (Invoice invoice in allInvoices)
            {
                if (invoice.AddOnID == addOn.ID && invoice.Paid && (!invoice.Notes.ToLower().Contains("prorated") || allInvoices.Count == 1))
                {
                    int qty = addOn.Quantity;
                    if (invoice.Notes.StartsWith("For"))
                    {
                        string[] notes = invoice.Notes.Split(' ');
                        if (notes.Length > 1 && InputHelper.GetDecimal(notes[1], 0) > 0)
                        {
                            qty = (int)InputHelper.GetDecimal(notes[1], 0);
                        }
                    }
                    return invoice.InvoiceTotal / qty;
                }
            }

            return 0;
        }
    }

    public class AddOnDetailsVMMapperConfiguration : MappingConfiguration
    {
        public int AddOnTypeID { get; set; }
        public List<ContractAddOn> AddOns { get; set; } = new List<ContractAddOn>();
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public bool IsBundleable { get; set; }
        public bool IsMultiAdd { get; set; }
        public ContractAddOn AddOn { get; set; }
    }

    public class MultiAddOnDetailsVMMapperConfiguration : AddOnDetailsVMMapperConfiguration
    {
        public int Qty { get; set; }
        public int SelectedQty { get; set; }
        public int SelectedID { get; set; }
    }
}