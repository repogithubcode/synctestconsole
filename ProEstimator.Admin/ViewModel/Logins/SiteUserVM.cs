using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.ViewModel.Logins
{
    public class SiteUserVM
    {
        public int ID { get; private set; }
        public int LoginID { get; private set; }
        public string EmailAddress { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

        public bool HasPassword { get; set; }
        public string Password { get; set; }

        public List<SiteUserHasPermissionVM> HasPermissions { get; set; }

        public SiteUserVM(SiteUser siteUser)
        {
            ID = siteUser.ID;
            LoginID = siteUser.LoginID;
            EmailAddress = siteUser.EmailAddress;
            Name = siteUser.Name;
            IsDeleted = siteUser.IsDeleted;

            HasPassword = !string.IsNullOrEmpty(siteUser.Password);
            Password = siteUser.Password;

            HasPermissions = new List<SiteUserHasPermissionVM>();
        }
    }

    public class SiteUserHasPermissionVM
    {
        public string Tag { get; set; }
        public bool HasPermission { get; set; }

        public SiteUserHasPermissionVM()
        {

        }

        public SiteUserHasPermissionVM(string tag, bool hasPermission)
        {
            Tag = tag;
            HasPermission = hasPermission;
        }
    }
}