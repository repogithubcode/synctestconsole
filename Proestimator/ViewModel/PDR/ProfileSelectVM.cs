using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.PDR
{
    public class ProfileSelectVM
    {
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public string ErrorMessage { get; set; }

        public List<ProfileVM> Profiles { get; set; }

        public bool UseDefaultPDRRateProfile { get; set; }

        public ProfileSelectVM()
        {
            ErrorMessage = "";
            Profiles = new List<ProfileVM>();
        }
    }

    public class ProfileVM
    {
        public string ProfileName { get; set; }
        public int ProfileID { get; set; }

        public Boolean IsDefault { get; set; }

        public ProfileVM(string profileName, int profileID)
        {
            ProfileName = profileName;
            ProfileID = profileID;
        }
        public ProfileVM(string profileName, int profileID, Boolean isDefault)
        {
            ProfileName = profileName;
            ProfileID = profileID;
            IsDefault = isDefault;
        }
    }
}