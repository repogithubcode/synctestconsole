using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class CopyEstimateVM
    {
        public int UserID { get; set; }
        public int LoginID { get; set; }
        public int EstimateID { get; set; }
        public bool GoodEstimate { get; set; }

        public bool CreateBlankCustomer { get; set; }
        public bool CopyInsuranceInformation { get; set; }
        public bool CopyClaimantInformation { get; set; }
        public bool CopyAttachedImages { get; set; }
        public bool CopyLineItems { get; set; }
        public bool CopyLatestItemsOnly { get; set; }
        
        public string NewName { get; set; }

    }
}