using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimator.Business.Logic.Admin;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel.SalesRep
{
    public class SalesRepPermissionsVM
    {
        public int SessionSalesRepID { get; set; }
        public int SalesRepID { get; set; }

        public SelectList SalesRepPermissionLookupDDL { get; set; }
        public int SelectedSalesRepPermission { get; set; }

        public List<SalesRepPermissionGroupVM> PermissionGroups { get; private set; }

        public SalesRepPermissionsVM()
        {
            PermissionGroups = new List<SalesRepPermissionGroupVM>();
        }

        public void SetSalesRepID(int salesRepID)
        {
            List<SalesRepPermission> permissions = SalesRepPermission.GetAll();
            List<SalesRepPermissionLink> links = SalesRepPermissionManager.GetLinksForRep(salesRepID);

            PermissionGroups = new List<SalesRepPermissionGroupVM>();
            PermissionGroups.Add(new SalesRepPermissionGroupVM("Admin Permissions", permissions.Where(o => o.Group == SalesRepPermissionGroup.Admin).ToList(), links));
            PermissionGroups.Add(new SalesRepPermissionGroupVM("Sales Rep Permissions", permissions.Where(o => o.Group == SalesRepPermissionGroup.SalesRep).ToList(), links));
            PermissionGroups.Add(new SalesRepPermissionGroupVM("User Maintenance Permissions", permissions.Where(o => o.Group == SalesRepPermissionGroup.UserMaintenance).ToList(), links));
            PermissionGroups.Add(new SalesRepPermissionGroupVM("Email CCs", permissions.Where(o => o.Group == SalesRepPermissionGroup.EmailCC).ToList(), links));
        }
    }

    public class SalesRepPermissionGroupVM 
    {
        public string Name { get; set; }
        public List<SalesRepPermissionVM> Permissions { get; private set; }

        public SalesRepPermissionGroupVM()
        {
            Permissions = new List<SalesRepPermissionVM>();
        }

        public SalesRepPermissionGroupVM(string name, List<SalesRepPermission> permissions, List<SalesRepPermissionLink> links)
        {
            Name = name;
            Permissions = new List<SalesRepPermissionVM>();
            
            foreach (SalesRepPermission permission in permissions)
            {
                Permissions.Add(new SalesRepPermissionVM(permission, links.FirstOrDefault(o => o.PermissionID == permission.ID)));
            }
        }
    }

    public class SalesRepPermissionVM
    {
        public int PermissionID { get; set; }
        public string Tag { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasPermission { get; set; }

        public SalesRepPermissionVM()
        {

        }

        public SalesRepPermissionVM(SalesRepPermission salesRepPermission, SalesRepPermissionLink link)
        {
            PermissionID = salesRepPermission.ID;
            Tag = salesRepPermission.Tag;
            Name = salesRepPermission.Name;
            Description = salesRepPermission.Description;

            if (link != null)
            {
                HasPermission = link.HasPermission;
            }
            else
            {
                HasPermission = false;
            }
        }
    }
}