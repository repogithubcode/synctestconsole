using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.ProAdvisor;

namespace Proestimator.ViewModel.Profiles
{
    public class AddOnsPageVM : RateProfileVMBase
    {
        public bool CanBeEdited { get; set; }
    }

    public class AddOnPresetVM
    {
        public int ShellID { get; set; }
        public int PresetID { get; set; }
        public string OperationType { get; set; }
        public string LaborType { get; set; }
        public string OtherType { get; set; }
        public string OtherTypeOverride { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public double Labor { get; set; }
        public double Refinish { get; set; }
        public double Charge { get; set; }
        public double OtherCharge { get; set; }
        public bool Active { get; set; }
        public bool AutoSelect { get; set; }

        public AddOnPresetVM(ProAdvisorPreset preset)
        {
            ShellID = preset.PresetShell.ID;
            PresetID = preset.ID;
            OperationType = preset.PresetShell.OperationType;
            LaborType = preset.PresetShell.LaborType;
            OtherType = preset.PresetShell.OtherType;
            OtherTypeOverride = preset.OtherTypeOverride;
            Name = preset.PresetShell.Name;
            Notes = preset.PresetShell.Notes;
            Labor = preset.Labor;
            Refinish = preset.Refinish;
            Charge = preset.Charge;
            OtherCharge = preset.OtherCharge;
            Active = preset.Active;
            AutoSelect = preset.AutoSelect;

            if (OperationType == "RI")
            {
                OperationType = "R&I";
            }
        }

    }
}