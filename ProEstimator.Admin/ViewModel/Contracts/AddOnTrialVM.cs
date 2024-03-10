using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class AddOnTrialVM
    {
        public int ID { get; set; }
        public string ContractType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool IsDeleted { get; set; }

        public string ErrorMessage { get; set; }

        public AddOnTrialVM(ContractAddOnTrial addon)
        {
            if (addon != null)
            {
                ID = addon.ID;
                ContractType = addon.AddOnType.Type;
                StartDate = addon.StartDate.ToShortDateString();
                EndDate = addon.EndDate.ToShortDateString();
                IsDeleted = addon.IsDeleted;
            }

            ErrorMessage = "";
        }
    }
}