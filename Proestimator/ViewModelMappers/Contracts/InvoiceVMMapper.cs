using Proestimator.ViewModel.Contracts;
using ProEstimatorData.DataModel.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModelMappers.Contracts
{
    public class InvoiceVMMapper : IVMMapper<InvoiceVM>
    {

        public InvoiceVM Map(MappingConfiguration mappingConfiguration)
        {
            InvoiceVMMapperConfiguration config = mappingConfiguration as InvoiceVMMapperConfiguration;
            Invoice invoice = config.Invoice;

            InvoiceVM vm = new InvoiceVM();

            vm.InvoiceID = invoice.ID;
            vm.LoginID = invoice.LoginID;
            vm.ContractID = invoice.ContractID;
            vm.FeeInvoiceID = invoice.FeeInvoiceID;
            vm.PaymentNumber = invoice.PaymentNumber;
            vm.InvoiceType = invoice.InvoiceType.Type;
            vm.InvoiceAmount = invoice.InvoiceAmount;
            vm.SalesTax = invoice.SalesTax;
            vm.InvoiceTotal = invoice.InvoiceTotal;
            vm.DueDate = invoice.DueDate.ToShortDateString();
            vm.Notes = invoice.Notes;
            vm.Paid = invoice.Paid;
            vm.DatePaid = invoice.DatePaid.HasValue ? invoice.DatePaid.Value.ToShortDateString() : "";
            vm.PaymentID = invoice.PaymentID;
            vm.Summary = invoice.Summary;
            vm.PastDue = invoice.DueDate <= DateTime.Now.Date;
            vm.ToPayDue = (invoice.DaysUntilDue > 0 && invoice.DaysUntilDue <= 14) || config.EarlyRenew;
            vm.Deletable = config.Deletable;

            return vm;
        }
    }

    public class InvoiceVMMapperConfiguration : MappingConfiguration
    {
        public Invoice Invoice { get; set; }
        public bool EarlyRenew { get; set; }
        public bool Deletable { get; set; }
    }
}