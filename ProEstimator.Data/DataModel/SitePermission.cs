using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class SitePermission
    {
        public int ID { get; private set; }
        public int SortOrder { get; private set; }
        public string Tag { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public List<string> Path { get; private set; }

        private static List<SitePermissionPath> _path;
        private static List<SitePermission> _list;
        private static object _loadLock = new object();
        private static object _pathLock = new object();

        public SitePermission(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            SortOrder = InputHelper.GetInteger(row["SortOrder"].ToString());
            Tag = row["Tag"].ToString();
            Name = row["Name"].ToString();
            Description = row["Description"].ToString();
            SetPath();
        }

        public static List<SitePermission> GetAll()
        {
            if (_list == null)
            {
                lock (_loadLock)
                {
                    _list = new List<SitePermission>();

                    DBAccess db = new DBAccess();
                    DBAccessTableResult table = db.ExecuteWithTable("SitePermissions_GetAll");

                    foreach (DataRow row in table.DataTable.Rows)
                    {
                        _list.Add(new SitePermission(row));
                    }

                    _list = _list.OrderBy(o => o.SortOrder).ToList();
                }
            }

            return _list.ToList();
        }

        public static SitePermission GetForTag(string tag)
        {
            return GetAll().FirstOrDefault(o => o.Tag == tag);
        }

        private List<SitePermissionPath> GetAllPath()
        {
            lock (_pathLock)
            {
                if (_path == null)
                {
                    _path = new List<SitePermissionPath>();

                    DBAccess db = new DBAccess();
                    DBAccessTableResult table = db.ExecuteWithTable("SitePermissionPaths_GetAll");

                    foreach (DataRow row in table.DataTable.Rows)
                    {
                        _path.Add(new SitePermissionPath(row));
                    }

                }
            }
            return _path;
        }

        private void SetPath()
        {
            Path = new List<string>();
            List<SitePermissionPath> temp = GetAllPath().Where(o => o.PermissionID == ID).ToList();
            foreach(SitePermissionPath p in temp)
            {
                Path.Add(p.Path);
            }
        }

    }

    public class SitePermissionPath
    {
        public int ID { get; private set; }
        public int PermissionID { get; private set; }
        public string Path { get; private set; }

        public SitePermissionPath(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            PermissionID = InputHelper.GetInteger(row["PermissionID"].ToString());
            Path = row["PagePath"].ToString();
        }
    }
}
