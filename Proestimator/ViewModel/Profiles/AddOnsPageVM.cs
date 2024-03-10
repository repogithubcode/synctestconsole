using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.AddOns;

namespace Proestimator.ViewModel.Profiles
{
    public class AddOnsPageVM : RateProfileVMBase
    {
        
    }

    public class AddOnPresetVM
    {
        public int ShellID { get; set; }
        public int PresetID { get; set; }
        public string OperationType { get; set; }
        public string LaborType { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public double Labor { get; set; }
        public double Refinish { get; set; }
        public double Charge { get; set; }
        public bool Active { get; set; }

        public AddOnPresetVM(AddOnPreset preset)
        {
            ShellID = preset.PresetShell.ID;
            PresetID = preset.ID;
            OperationType = preset.PresetShell.OperationType;
            LaborType = preset.PresetShell.LaborType;
            Name = preset.PresetShell.Name;
            Notes = preset.PresetShell.Notes;
            Labor = preset.Labor;
            Refinish = preset.Refinish;
            Charge = preset.Charge;
            Active = preset.Active;
        }

    }
}