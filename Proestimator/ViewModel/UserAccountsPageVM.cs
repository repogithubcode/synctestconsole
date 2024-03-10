using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class UserAccountsPageVM
    {
        public int LoginID { get; set; }
        public int MaxUsers { get; set; }

        public List<SiteUserPermissionVM> Permissions { get; set; }

        public UserAccountsPageVM()
        {
            Permissions = new List<SiteUserPermissionVM>();

            List<SitePermission> permissions = SitePermission.GetAll();

            foreach (SitePermission permission in permissions)
            {
                Permissions.Add(new SiteUserPermissionVM(permission, false));
            }
        }
    }

}