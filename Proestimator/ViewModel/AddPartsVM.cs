using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;
using ProEstimatorData.Models.EditorTemplateModel;

using Proestimator.ViewModel.PDR;

namespace Proestimator.ViewModel
{
    public class AddPartsVM : PDRMatrixVMBase
    {

        public bool ShowGraphicalButton { get; set; }
        public ManualEntry ManualEntry { get; set; }

        public int Supplement { get; set; }

        public bool EstimateIsLocked { get; set; }
        public bool HasPDRContract { get; set; }

        public AddPartsVM(int loginID, int estimateID, bool isMobile)
            : base(loginID, estimateID, isMobile)
        {
            
        }
    }
}