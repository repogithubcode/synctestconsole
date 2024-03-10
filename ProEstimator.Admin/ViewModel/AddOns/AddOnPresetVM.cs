using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.AddOns;

namespace ProEstimator.Admin.ViewModel.AddOns
{
    public class AddOnPresetVM
    {

        public int ID { get; set; }
        public string OperationType { get; set; }
        public bool Sublet { get; set; }
        public string LaborType { get; set; }
        public string OtherType { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public int AccessLevel { get; set; }
        public bool OnePerVehicle { get; set; }

        public AddOnPresetVM(AddOnPresetShell preset)
        {
            ID = preset.ID;
            OperationType = preset.OperationType;
            Sublet = preset.Sublet;
            LaborType = preset.LaborType;
            OtherType = preset.OtherType;
            Name = preset.Name;
            Notes = preset.Notes;
            AccessLevel = preset.AccessLevel;
            OnePerVehicle = preset.OnePerVehicle;

            if (string.IsNullOrEmpty(OtherType))
            {
                OtherType = "NA";
            }

            if (OperationType == "RI")
            {
                OperationType = "R&I";
            }
        }
    }
}