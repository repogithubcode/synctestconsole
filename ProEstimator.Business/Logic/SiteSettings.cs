using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimatorData.DataModel.Settings;

namespace ProEstimator.Business.Logic
{
    public static class SiteSettings
    {

        public static SiteSetting Get(int loginID, string tag, string tagGroup, string defaultValue = "")
        {
            SiteSetting siteSetting = SiteSetting.GetByTag(loginID, tag);

            if (siteSetting == null)
            {
                siteSetting = new SiteSetting();
                siteSetting.LoginID = loginID;
                siteSetting.TagGroup = tagGroup;
                siteSetting.Tag = tag;
                siteSetting.ValueString = defaultValue;

                siteSetting.Save(0);
            }

            return siteSetting;
        }

        public static void SaveSetting(int activeLoginID, int loginID, string tag, string tagGroup, string value)
        {
            SiteSetting siteSetting = SiteSettings.Get(loginID, tag, tagGroup, value);

            siteSetting.ValueString = value;
            siteSetting.Save(activeLoginID);
        }

        public static void SaveSetting(int activeLoginID, int loginID, string tag, string tagGroup, int value)
        {
            SaveSetting(activeLoginID, loginID, tag, tagGroup, value.ToString());
        }

    }
}
