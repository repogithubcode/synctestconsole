using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProEstimatorData.Models.EditorTemplateModel;

namespace Proestimator.ViewModel.Profiles
{
    public class PresetChargesVM : RateProfileVMBase 
    {
        public ManualEntry ManualEntry { get; set; }

        public bool UseDefaultProfile { get; set; }

        public string JavascriptID { get { return "1"; } }
    }
}