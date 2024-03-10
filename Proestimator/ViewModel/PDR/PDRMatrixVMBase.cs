using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.PDR
{
    public class PDRMatrixVMBase
    {
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public bool PDREnabled { get; set; }
        public bool PDRAutoOpen { get; set; }

        public PDRMatrix PDRMatrix { get; set; }
        public PDRMatrixMobile PDRMatrixMobile { get; set; }

        public PDRMatrixVMBase(int loginID, int estimateID, bool isMobile)
        {
            LoginID = loginID;
            EstimateID = estimateID;

            PDREnabled = false;
            PDRAutoOpen = false;

            PDRMatrixMobile = new PDRMatrixMobile(loginID, estimateID);
            PDRMatrix = new PDRMatrix(loginID, estimateID);
        }
    }
}