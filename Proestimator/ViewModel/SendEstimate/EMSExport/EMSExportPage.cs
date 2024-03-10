using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.SendEstimate.EMSExport
{
    public class EMSExportPage
    {
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public bool ContractDeactivated { get; set; }
    }
}