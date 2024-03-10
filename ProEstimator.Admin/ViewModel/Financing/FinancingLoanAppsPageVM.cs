using System.Web.Mvc;

namespace ProEstimator.Admin.ViewModel.Financing
{
    public class FinancingLoanAppsPageVM
    {
        public SelectList LoanAppStatusList { get; set; }
        public string SelectedLoanAppStatus { get; set; }
    }
}