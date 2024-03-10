using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;
using ProEstimatorData.Models;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Settings;
using ProEstimatorData.DataModel.Contracts;

using Proestimator.ViewModel.Contracts;

namespace Proestimator.ViewModel
{
    public class ContractHistoryTabVM
    {

        public int LoginID { get; set; }

        public List<ContractVM> Contracts { get; set; }

        public ContractHistoryTabVM()
        {
            Contracts = new List<ContractVM>();
        }

    }
}