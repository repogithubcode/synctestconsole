using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Profiles;

namespace Proestimator.ViewModel.Profiles
{
    public class PaintFinishVM : RateProfileVMBase 
    {
        public PaintSettings paintProfile { get; set; }

        //redifining these as bools because you can't bind to bool?
        public bool AllowDeduction { get; set; }
        public bool EdgeInterior { get; set; }
        public bool ThreeStageInner { get; set; }
        public bool TwoToneInner { get; set; }
        public bool ThreeStagePillars { get; set; }
        public bool TwoTonePillars { get; set; }
        public bool ThreeStateInterior { get; set; }
        public bool TwoToneInterior { get; set; }

        public bool UseDefaultProfile { get; set; }
    }
}