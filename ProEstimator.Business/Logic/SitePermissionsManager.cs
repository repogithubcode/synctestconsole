using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimatorData.DataModel;

namespace ProEstimator.Business.Logic
{
    public class SitePermissionsManager
    {

        public int SiteUserID { get; private set; }

        private List<SitePermissionLink> _links;

        public SitePermissionsManager(int siteUserID)
        {
            SiteUserID = siteUserID;

            _links = SitePermissionLink.GetForUser(siteUserID);
        }

        public bool HasPermission(string tag)
        {
            SitePermission sitePermission = SitePermission.GetForTag(tag);

            if (sitePermission != null)
            {
                SitePermissionLink link = _links.FirstOrDefault(o => o.PermissionID == sitePermission.ID);
                if (link != null)
                {
                    return link.HasPermission;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public void SavePermission(string tag, bool hasPermission)
        {
            SitePermission sitePermission = SitePermission.GetForTag(tag);

            if (sitePermission != null)
            {
                SitePermissionLink link = _links.FirstOrDefault(o => o.PermissionID == sitePermission.ID);
                
                if (link == null)
                {
                    link = new SitePermissionLink();
                    link.PermissionID = sitePermission.ID;
                    link.SiteUserID = SiteUserID;
                    link.HasPermission = true;
                }

                if (link.HasPermission != hasPermission)
                {
                    link.HasPermission = hasPermission;
                    link.Save();
                }
            }
        }

        public bool IsPageAllowed(string page)
        {
            if(SiteUserID == 0)
            {
                return true;
            }
            List<SitePermission> allPermission = SitePermission.GetAll();
            foreach(SitePermission permission in allPermission)
            {
                bool hasPermission = HasPermission(permission.Tag);
                if(hasPermission && permission.Tag == "Admin")
                {
                    return true;
                }
                if(!hasPermission)
                {
                    foreach (string s in permission.Path)
                    {
                        if (page.IndexOf(s, StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

    }
}
