using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Business.Logic.Admin
{
    public class SalesRepPermissionManager
    {
        private static Dictionary<int, List<SalesRepPermissionLink>> _links;
        private static object _loadLock = new object();

        private static bool _cacheData = true;

        // This is super hacky.  This was designed to run on the admin site, but for email addresses we now have it run on the front end too.  We have to turn 
        // caching off on the front end so changes made in admin show up.
        public static void TurnOffCache()
        {
            _cacheData = false;
        }

        private static void LoadData()
        {
            if (_links == null || !_cacheData)
            {
                lock(_loadLock)
                {
                    _links = SalesRepPermissionLink.GetAll();
                }
            }
        }

        public static List<SalesRepPermissionLink> GetLinksForRep(int salesRepID)
        {
            LoadData();

            if (_links.ContainsKey(salesRepID))
            {
                return _links[salesRepID];
            }

            return new List<SalesRepPermissionLink>();
        }

        public static bool HasPermission(int salesRepID, string tag)
        {
            if (salesRepID == 0)
            {
                return true;
            }

            LoadData();

            SalesRepPermission permission = SalesRepPermission.GetForTag(tag);
            if (permission == null)
            {
                return false;
            }

            if (!_links.ContainsKey(salesRepID))
            {
                return false;
            }

            SalesRepPermissionLink link = _links[salesRepID].FirstOrDefault(o => o.PermissionID == permission.ID);
            if (link != null)
            {
                return link.HasPermission;
            }

            return false;
        }

        public static void SetPermission(int salesRepID, string tag, bool hasPermission)
        {
            LoadData();

            if (!_links.ContainsKey(salesRepID))
            {
                _links.Add(salesRepID, new List<SalesRepPermissionLink>());
            }

            SalesRepPermission permission = SalesRepPermission.GetForTag(tag);
            if (permission != null)
            {
                SalesRepPermissionLink link = _links[salesRepID].FirstOrDefault(o => o.PermissionID == permission.ID);
                if (link == null)
                {
                    link = new SalesRepPermissionLink();
                    link.PermissionID = permission.ID;
                    link.SalesRepID = salesRepID;

                    _links[salesRepID].Add(link);
                }

                link.HasPermission = hasPermission;
                link.Save();
            }
        }

        public static List<string> GetSalesRepEmailAddressForPermissionTag(string tag)
        {
            List<string> emailAddresses = new List<string>();

            LoadData();

            SalesRepPermission permission = SalesRepPermission.GetForTag(tag);
            if (permission == null)
            {
                return new List<string>();
            }

            foreach (var key in _links.Keys)
            {
                List<SalesRepPermissionLink> salesRepPermissionLinkColl = _links[key].ToList<SalesRepPermissionLink>();
                salesRepPermissionLinkColl = salesRepPermissionLinkColl.Where(o => o.PermissionID == permission.ID).ToList<SalesRepPermissionLink>();

                foreach (SalesRepPermissionLink eachSalesRepPermissionLink in salesRepPermissionLinkColl)
                {
                    if (eachSalesRepPermissionLink.HasPermission)
                    {
                        SalesRep salesRep = SalesRep.Get(eachSalesRepPermissionLink.SalesRepID);
                        emailAddresses.Add(salesRep.Email);
                    }
                }
            }

            return emailAddresses;
        }

    }
}
