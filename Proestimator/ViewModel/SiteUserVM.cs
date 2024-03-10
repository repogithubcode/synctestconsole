using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class SiteUserVM
    {

        public int UserID { get; set; }
        public int LoginID { get; set; }
        public string EmailAddress { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public bool IsDeleted { get; set; }
        public bool Admin { get; private set; }
        public SiteUserVM()
        {

        }

        public SiteUserVM(SiteUser siteUser)
        {
            UserID = siteUser.ID;
            LoginID = siteUser.LoginID;
            EmailAddress = siteUser.EmailAddress;
            Name = siteUser.Name;
            Password = siteUser.Password;
            IsDeleted = siteUser.IsDeleted;
            Admin = IsAdmin();
        }

        private bool IsAdmin()
        {
            ProEstimator.Business.Logic.SitePermissionsManager manager = new ProEstimator.Business.Logic.SitePermissionsManager(UserID);
            return manager.HasPermission("Admin");
        }

    }
}