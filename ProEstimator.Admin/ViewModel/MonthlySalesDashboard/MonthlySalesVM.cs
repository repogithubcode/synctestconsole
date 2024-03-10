using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;
using ProEstimatorData.DataModel.Admin;
using System.Globalization;

namespace ProEstimator.Admin.ViewModel.MonthlySalesDashboard
{
    public class MonthlySalesVM
    {
        public Boolean PaymentExtension { get; set; }
        public Boolean Select { get; set; }
        public int CustomerID { get; set; }
        public int ContractID { get; set; }
        public int PreviousCycleContractID { get; set; }
        public string State { get; set; }
        public string SalesDate { get; set; }
        public string CustomerName { get; set; }
        public double SalesPrice { get; set; }
        public double Promo { get; set; }
        public double DownPayment { get; set; }
        public double MonthlyPayment { get; set; }
        public bool HasFrame { get; set; }
        public bool HasEMS { get; set; }
        public bool HasPDR { get; set; }
        public bool HasMultiUser { get; set; }
        public bool HasQBExport { get; set; }
        public bool HasProAdvisor { get; set; }
        public bool HasImageEditor { get; set; }
        public bool HasBundle { get; set; }
        public bool HasReporting { get; set; }
        public int SalesRepID { get; set; }
        public string SalesRep { get; set; }

        public string WalkThrough { get; set; }
        public double? Commission { get; set; }
        public int PayCode { get; set; }

        public double? MUCommission { get; set; }
        public double? QBCommission { get; set; }

        public double? TotalCommission { get; set; }
        public string ContractTermDescription { get; set; }

        public Boolean RowValChanged { get; set; }

        // renew fields
        public bool Has1styrRenewal { get; set; }
        public bool HasLateRenewal { get; set; }

        public string ExpectedRenewalDate { get; set; }
        public string ActualRenewalDate { get; set; }
        public double RenewalDiscount { get; set; }
        public double PreviousCycleSalesPrice { get; set; }

        public string MarkStatus { get; set; }

        public MonthlySalesVM()
        {

        }

        public MonthlySalesVM(MonthlyNewSales monthlyNewSales)
        {
            CustomerID = monthlyNewSales.CustomerID;
            ContractID = monthlyNewSales.ContractID;
            State = monthlyNewSales.State;
            if(monthlyNewSales.SalesDate != null)
            {
                SalesDate = Convert.ToDateTime(monthlyNewSales.SalesDate).ToString("MM/dd/yyyy");
            }
            
            CustomerName = monthlyNewSales.CustomerName;
            SalesPrice = monthlyNewSales.SalesPrice;
            Promo = monthlyNewSales.Promo;
            DownPayment = monthlyNewSales.DownPayment;
            MonthlyPayment = monthlyNewSales.MonthlyPayment;

            HasFrame = monthlyNewSales.HasFrame;
            HasEMS = monthlyNewSales.HasEMS;
            HasPDR = monthlyNewSales.HasPDR;
            HasMultiUser = monthlyNewSales.HasMultiUser;
            HasQBExport = monthlyNewSales.HasQBExport;
            HasProAdvisor = monthlyNewSales.HasProAdvisor;
            HasImageEditor = monthlyNewSales.HasImageEditor;
            HasBundle = monthlyNewSales.HasBundle;
            HasReporting = monthlyNewSales.HasReporting;

            SalesRepID = monthlyNewSales.SalesRepID;
            SalesRep = monthlyNewSales.SalesRep;

            // WalkThrough = "2021-12-09";
            if (!string.IsNullOrEmpty(monthlyNewSales.WalkThrough))
            {
                WalkThrough = Convert.ToDateTime(monthlyNewSales.WalkThrough).ToString("yyyy-MM-dd");
            }

            Commission = monthlyNewSales.Commission;
            PayCode = monthlyNewSales.PayCode;

            MUCommission = monthlyNewSales.MUCommission;
            QBCommission = monthlyNewSales.QBCommission;
            ContractTermDescription = monthlyNewSales.ContractTermDescription;

            TotalCommission = monthlyNewSales.TotalCommission;
            PaymentExtension = monthlyNewSales.PaymentExtension;
        }

        public MonthlySalesVM(MonthlyRenewSales monthlyRenewSales)
        {
            CustomerID = monthlyRenewSales.CustomerID;
            ContractID = monthlyRenewSales.ContractID;
            PreviousCycleContractID = monthlyRenewSales.PreviousCycleContractID;

            if (monthlyRenewSales.ContractID == 0)
            {
                ContractID = PreviousCycleContractID;
            }

            State = monthlyRenewSales.State;

            CustomerName = monthlyRenewSales.CustomerName;
            SalesPrice = monthlyRenewSales.SalesPrice;
            DownPayment = monthlyRenewSales.DownPayment;
            MonthlyPayment = monthlyRenewSales.MonthlyPayment;

            HasFrame = monthlyRenewSales.HasFrame;
            HasEMS = monthlyRenewSales.HasEMS;
            HasPDR = monthlyRenewSales.HasPDR;
            HasMultiUser = monthlyRenewSales.HasMultiUser;
            HasQBExport = monthlyRenewSales.HasQBExport;
            HasProAdvisor = monthlyRenewSales.HasProAdvisor;
            HasImageEditor = monthlyRenewSales.HasImageEditor;
            HasBundle = monthlyRenewSales.HasBundle;
            HasReporting = monthlyRenewSales.HasReporting;

            SalesRepID = monthlyRenewSales.SalesRepID;
            SalesRep = monthlyRenewSales.SalesRep;

            Commission = monthlyRenewSales.Commission;
            PayCode = monthlyRenewSales.PayCode;

            MUCommission = monthlyRenewSales.MUCommission;
            QBCommission = monthlyRenewSales.QBCommission;

            TotalCommission = monthlyRenewSales.TotalCommission;

            Has1styrRenewal = monthlyRenewSales.Has1styrRenewal;
            HasLateRenewal = monthlyRenewSales.HasLateRenewal;

            if (monthlyRenewSales.ExpectedRenewalDate != null)
            {
                ExpectedRenewalDate = Convert.ToDateTime(monthlyRenewSales.ExpectedRenewalDate).ToString("MM/dd/yyyy");
            }

            if (monthlyRenewSales.ActualRenewalDate != null)
            {
                ActualRenewalDate = Convert.ToDateTime(monthlyRenewSales.ActualRenewalDate).ToString("MM/dd/yyyy");
            }

            RenewalDiscount = monthlyRenewSales.RenewalDiscount;
            PreviousCycleSalesPrice = monthlyRenewSales.PreviousCycleSalesPrice;
            PaymentExtension = monthlyRenewSales.PaymentExtension;
        }
    }
}