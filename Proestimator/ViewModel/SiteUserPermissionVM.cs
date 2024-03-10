using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class SiteUserPermissionVM
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasPermission { get; set; }

        public SiteUserPermissionVM()
        {

        }

        public SiteUserPermissionVM(SitePermission permission, bool hasPermission)
        {
            Tag = permission.Tag;
            Name = permission.Name;
            Description = permission.Description;
            HasPermission = hasPermission;
        }
    }
}