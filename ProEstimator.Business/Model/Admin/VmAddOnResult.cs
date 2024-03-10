using System.Collections.Generic;
using System.Data;
using System.Globalization;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmAddOnResult : IModelMap<VmAddOnResult>
    {
        public VmAddOnResult()
        {
            Details = new List<VmInvoiceRow>();
        }
        public VmAddOnResult ToModel(DataRow row)
        {
            var model = new VmAddOnResult();
            model.ContractId = row["ContractID"].SafeInt();
            model.AllowReactivation = (bool) row["allowReactivation"];
            model.ContractActive = row["ContractActive"].SafeBool();
            model.NextContractPriceLevelID = row["NextContractPriceLevelID"].SafeInt();
            model.PaymentsRemaining = row["PaymentsRemaining"].SafeInt();
            model.AllowReactivation = (bool) row["allowReactivation"];
            model.ParentContractDateRange = row["ParentContractDateRange"].SafeString();
            model.EffectiveDate = row["EffectiveDate"].SafeString();
            model.ExpirationDate = row["ExpirationDate"].SafeString();
            model.TermDescription = row["TermDescription"].SafeString();
            model.AutoPay = (bool) row["isAutoPay"];
            model.NextInvoiceAmount = ((decimal) row["NextInvoiceAmount"]).ToString(CultureInfo.InvariantCulture);
            model.NextInvoiceDueDate = row["NextInvoiceDueDate"].SafeString();
            model.TermTotal = (decimal) row["TermTotal"];
            model.TotalRemaining = (decimal) row["TotalRemaining"];

            return model;
        }

        public decimal TotalRemaining { get; set; }
        public decimal TermTotal { get; set; }
        public string NextInvoiceDueDate { get; set; }
        public string NextInvoiceAmount { get; set; }
        public string TermDescription { get; set; }
        public string ExpirationDate { get; set; }
        public string EffectiveDate { get; set; }
        public string ParentContractDateRange { get; set; }
        public int? PaymentsRemaining { get; set; }
        public bool? ContractActive { get; set; }
        public int? ContractId { get; set; }
        public int NewestContractId { get; set; }
        public bool Trial { get; set; }

        public int? NextContractPriceLevelID;
        public bool AllowReactivation;
        public bool AutoPay;
        public List<VmInvoiceRow> Details { get; set; }
        public bool DisplayPayAll { get; set; }
    }
}
