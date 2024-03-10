using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.PDR;

namespace Proestimator.ViewModel.PDR
{
    public class DescriptionPresetsVM
    {
        public int LoginID { get; set; }
        public bool GoodData { get; set; }

        public string ErrorMessage { get; set; }

        public List<DescriptionPresetVM> DescriptionPresets { get; private set; }

        public DescriptionPresetsVM()
        {
            GoodData = false;
            ErrorMessage = "";

            DescriptionPresets = new List<DescriptionPresetVM>();
        }

        public void LoadDescriptionPresets(List<PDR_DescriptionPreset> descriptionPresets)
        {
            descriptionPresets.ForEach(o => DescriptionPresets.Add(new DescriptionPresetVM(o)));
        }

    }

    public class DescriptionPresetVM
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public int Index { get; set; }

        public DescriptionPresetVM()
        {
            ID = 0;
            Text = "";
            Index = 0;
        }

        public DescriptionPresetVM(PDR_DescriptionPreset descriptionPreset)
        {
            ID = descriptionPreset.ID;
            Text = descriptionPreset.Text;
            Index = 0;
        }
    }
}