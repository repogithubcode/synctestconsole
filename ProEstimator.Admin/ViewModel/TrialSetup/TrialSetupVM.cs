using System;
using System.Data;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.Model.Admin;

namespace ProEstimator.Admin.ViewModel.Import
{
    public class TrialSetupVM
    {
        public int SessionSalesRepID { get; set; }
        public int LoginID { get; set; }
        public string LoginName { get; set; }
        public string Organizationname { get; set; }
        public string SalesRep { get; set; }
        public Boolean IsAccountInPE { get; set; }
        public Boolean IsAccountInWE { get; set; }
        public Boolean IsActiveTrialExist { get; set; }
        public Boolean IsActiveContractExist { get; set; }
        public string TrialEndDate { get; set; }

        public TrialSetupVM()
        {

        }
    }
}

