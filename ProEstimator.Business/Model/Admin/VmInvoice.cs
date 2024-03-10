using System;
using System.Collections.Generic;
using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmInvoice : IModelMap<VmInvoice>
    {
        public VmInvoice()
        {
            Details = new List<VmInvoiceRow>();
            Logins = new List<VmContractLogin>();
            Contracts = new List<VmFrameDataContract>();
            Promos = new List<VmPromo>();
        }
        public int? ContractId { get; set; }
        public int? ContractPriceLevelID { get; set; }
        public int? PromoID { get; set; }
        public int? NextContractPriceLevelID { get; set; }
        public string RenewalDate { get; set; }
        public int? PaymentsRemaining { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpirationDate { get; set; }
        public bool? ExtendedContract { get; set; }
        public int? PriceLevel { get; set; }
        public string TermDescription { get; set; }
        public bool AutoPay { get; set; }
        public decimal NextInvoiceAmount { get; set; }
        public decimal TermTotal { get; set; }
        public decimal TotalRemaining { get; set; }
        public string NextInvoiceDueDate { get; set; }
        public bool? AcceptEChecks { get; set; }
        public bool? Signed { get; set; }
        public string DocuSignEnvelopeId { get; set; }
        public bool? HasFrameDataContract { get; set; }
        public bool? HasEMSContract { get; set; }
        public bool? HasPDRContract { get; set; }
        public bool? HasMultiUserContract { get; set; }
        public bool? Renewed { get; set; }

        public List<VmInvoiceRow> Details { get; set; }
        public List<VmContractLogin> Logins { get; set; }
        public List<VmFrameDataContract> Contracts { get; set; }
        public VmAddOnResult FrameDataDetails { get; set; }
        public VmAddOnResult Ems { get; set; }
        public VmAddOnResult Custom { get; set; }
        public VmAddOnResult MultiUser { get; set; }
        public string SelectedContractId { get; set; }

        public VmInvoice ToModel(DataRow row)
        {
            var model = new VmInvoice();
            model.ContractId = row["ContractID"].SafeInt();
            model.ContractPriceLevelID = row["ContractPriceLevelID"].SafeInt();
            model.PromoID = row["PromoID"].SafeInt();
            model.NextContractPriceLevelID = row["NextContractPriceLevelID"].SafeInt();
            model.RenewalDate = row["RenewalDate"].SafeString();
            model.PaymentsRemaining = row["PaymentsRemaining"].SafeInt();
            model.EffectiveDate = row["EffectiveDate"].SafeString();
            model.ExpirationDate = row["ExpirationDate"].SafeString();
            model.ExtendedContract = row["isExtendedContract"].SafeBool();
            model.Renewed = row["isRenewed"].SafeBool();
            model.PriceLevel = row["PriceLevel"].SafeInt();
            model.TermDescription = row["TermDescription"].SafeString();
            model.AutoPay = (bool)row["isAutoPay"];
            model.NextInvoiceAmount = (decimal)row["NextInvoiceAmount"];
            model.TermTotal = (decimal)row["TermTotal"];
            model.TotalRemaining = (decimal)row["TotalRemaining"];
            model.NextInvoiceDueDate = row["NextInvoiceDueDate"].SafeString();
            model.AcceptEChecks = (bool)row["acceptEChecks"];
            model.Signed = (bool)row["isSigned"];
            model.DocuSignEnvelopeId = row["DocuSignEnvelopeID"].SafeString();

            return model;
        }

        public List<VmPromo> Promos { get; set; }

        public VmAddOnResult PDR { get; set; }
    }
}
