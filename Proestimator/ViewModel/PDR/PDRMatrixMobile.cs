using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel.PDR;

namespace Proestimator.ViewModel.PDR
{
    public class PDRMatrixMobile : PDRMatrix
    {
     
        public PDRMatrixMobile(int loginID, int estimateID) 
            : base(loginID, estimateID)
        {

        }
    }
}