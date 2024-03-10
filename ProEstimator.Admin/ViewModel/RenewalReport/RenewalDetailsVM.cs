using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel.RenewalReport
{
    public class RenewalDetailsVM
    {
        public string LoginID { get; set; }
        public string ContractID { get; set; }
        public string SalesRep { get; set; }
        public string Company { get; set; }
        public string AddOns { get; set; }
        public string State { get; set; }
        public double RenewalAmount { get; set; }
        public string EstimateCountSummary { get; set; }
        public int EstimateCountTotal { get; set; }
        public int EstimateCountCurrent { get; set; }
        public int YearsWithWebEst { get; set; }
        public string RenewalDate { get; set; }
        public bool WillRenew { get; set; }
        public bool WillNotRenew { get; set; }
        public double PastDue { get; set; }
        public string Source { get; set; }
        public string Notes { get; set; }
        public bool DidRenew { get; set; }
        public string PETrial { get; set; }
        public bool AutoRenew { get; set; }
        public bool IsContractSigned { get; set; }

        public RenewalDetailsVM()
        {

        }

        public RenewalDetailsVM(RenewalDetails details)
        {
            LoginID = details.LoginID.ToString();
            ContractID = details.ContractID.ToString();
            SalesRep = details.SalesRep;
            Company = "Name: " + details.CompanyName + "<br />Contact: " + details.Contact + "<br />Phone: " + InputHelper.FormatPhone(details.PhoneNumber);

            AddOns = "FD : " + (details.HasFrame ? "Y" : "N") + "<br />EMS : " + (details.HasEMS ? "Y" : "N") + "<br />MULTI : " + (details.HasMU ? "Y" : "N");
            if (details.HasBundle)
            {
                AddOns += "<br />Bundle : " + (details.HasBundle ? "Y" : "N") + "<br />QB : B<br />PA : B<br />IE : B<br />ER : B";
            }
            else
            {
                AddOns += "<br />Bundle : " + (details.HasBundle ? "Y" : "N") + "<br />QB : " + (details.HasQB ? "Y" : "N") + "<br />PA : " + (details.HasPA ? "Y" : "N") + "<br />IE : " + (details.HasIE ? "Y" : "N") + "<br />ER : " + (details.HasER ? "Y" : "N");
            }

            State = details.State;
            RenewalAmount = details.ContractTotal;
            EstimateCountSummary = "Total: " + details.TotalEstimates + "<br />Current: " + details.CurrentEstimates;
            EstimateCountTotal = details.TotalEstimates;
            EstimateCountCurrent = details.CurrentEstimates;
            YearsWithWebEst = details.TotalYears;
            //RenewalDate = details.Source == "PE" ? details.ContractEnd.AddDays(1).ToShortDateString() : details.ContractEnd.ToShortDateString();
            RenewalDate = details.ContractEnd.ToShortDateString();
            WillRenew = details.WillRenew;
            WillNotRenew = details.WillNotRenew;
            PastDue = details.TotalDue;
            Source = details.Source;
            Notes = details.Notes;
            DidRenew = details.DidRenew;
            PETrial = details.PETrial.HasValue ? details.PETrial.Value.ToShortDateString() : "";
            AutoRenew = details.AutoRenew;
            IsContractSigned = details.IsContractSigned;
        }
    }
}