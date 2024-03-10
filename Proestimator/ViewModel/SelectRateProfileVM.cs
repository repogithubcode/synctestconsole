using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class SelectRateProfileVM
    {
        public int UserID { get; set; }
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public bool ShowProfiles { get; set; }
        public int SelectedProfileID { get; set; }
        public List<RateProfileInfo> Profiles { get; set; }

        public bool ShowAddOnProfiles { get; set; }
        public int SelectedAddOnProfileID { get; set; }
        public List<RateProfileInfo> AddOnProfiles { get; set; }

        public bool UseDefaultRateProfile { get; set; }

        public bool UseDefaultAddOnProfile { get; set; }
        public SelectRateProfileVM()
        {
            Profiles = new List<RateProfileInfo>();
            AddOnProfiles = new List<RateProfileInfo>();
        }
    }

    public class RateProfileInfo 
    {
        public Boolean IsDefault {  get; set; }
        public int ID { get; set; }
        public string Description { get; set; }
    }
}