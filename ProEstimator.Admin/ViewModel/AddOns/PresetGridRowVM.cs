using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.AddOns;

namespace ProEstimator.Admin.ViewModel.AddOns
{
    public class PresetGridRowVM
    {
        public int ID { get; set; }
        public string OperationType { get; set; }
        public bool Sublet { get; set; }
        public string LaborType { get; set; }
        public string Name { get; set; }

        public PresetGridRowVM(AddOnPresetShell presetShell)
        {
            ID = presetShell.ID;
            OperationType = presetShell.OperationType;
            Sublet = presetShell.Sublet;
            LaborType = presetShell.LaborType;
            Name = presetShell.Name;
        }
    }
}