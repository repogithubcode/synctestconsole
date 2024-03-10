using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;
using ProEstimatorData.Models;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.DataModel.Profiles;
using ProEstimatorData.DataModel.ProAdvisor;

namespace Proestimator.ViewModel.Profiles
{
    public class RateProfileListVM
    {

        public bool UseDefaultProfile { get; set; }
        public bool UsePDRDefaultProfile { get; set; }
        public bool UseDefaultAddOnProfile { get; set; }

        public bool ShowPDR { get; set; }
        public bool ShowAddOns { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class RateProfileGridRow
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool DefaultProfile { get; set; }
        public bool DefaultPreset { get; set; }
        public DateTime? Created { get; set; }
        public bool History { get; set; }

        public RateProfileGridRow()
        {
            
        }

        public RateProfileGridRow(RateProfile rateProfile)
        {
            ID = rateProfile.ID;
            Name = rateProfile.Name;
            Description = rateProfile.Description;
            DefaultProfile = rateProfile.IsDefault;
            DefaultPreset = rateProfile.IsDefaultPresets;
            Created = rateProfile.CreationStamp;
            History = rateProfile.GetHistory().Count > 0 ? true : false;
        }

        public RateProfileGridRow(PDR_RateProfile rateProfile)
        {
            ID = rateProfile.ID;
            Name = rateProfile.ProfileName;
            DefaultProfile = rateProfile.IsDefault;
            Created = rateProfile.CreationStamp;
            History = rateProfile.GetHistory().Count > 0 ? true : false;
        }

        public RateProfileGridRow(ProAdvisorPresetProfile rateProfile)
        {
            ID = rateProfile.ID;
            Name = rateProfile.Name;
            DefaultProfile = rateProfile.DefaultFlag;
            Created = rateProfile.CreationStamp;
            History = rateProfile.GetHistory().Count > 0 ? true : false;
        }

        public RateProfileGridRow(int id, string name)
        {
            ID = id;
            Name = name;
            Created = null;
        }
    }
}