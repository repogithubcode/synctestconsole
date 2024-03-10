using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel.PartnerNetwork
{
    public class PartnerNetworkPageVM
    {

        public List<ProEstimatorData.DataModel.PartnerNetwork> Partners { get; set; }

        public PartnerNetworkPageVM()
        {
            Partners = new List<ProEstimatorData.DataModel.PartnerNetwork>();
        }

    }
}