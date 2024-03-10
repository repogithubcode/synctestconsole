using Proestimator.ViewModelMappers.Contracts;
using ProEstimatorData.DataModel.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.Contracts
{
    public class AddOnConfirmVM
    {
        public int AddOnID { get; set; }
        public string ContractType { get; set; }
        public string TermDescription { get; set; }
        public List<InvoiceVM> Invoices { get; set; }
        public int Quantity { get; set; }
    }
}