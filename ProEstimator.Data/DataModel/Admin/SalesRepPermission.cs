using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    public class SalesRepPermission
    {
        public int ID { get; private set; }
        public SalesRepPermissionGroup Group { get; private set; }
        public string Tag { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        private static List<SalesRepPermission> _list;
        private static object _loadLock = new object();

        public SalesRepPermission(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Group = (SalesRepPermissionGroup)InputHelper.GetInteger(row["PermissionGroup"].ToString());
            Tag = row["Tag"].ToString();
            Name = row["Name"].ToString();
            Description = row["Description"].ToString();
        }

        public static List<SalesRepPermission> GetAll()
        {
            if (_list == null)
            {
                lock(_loadLock)
                {
                    _list = new List<SalesRepPermission>();

                    DBAccess db = new DBAccess();
                    DBAccessTableResult table = db.ExecuteWithTable("SalesRepPermission_GetAll");

                    foreach(DataRow row in table.DataTable.Rows)
                    {
                        _list.Add(new SalesRepPermission(row));
                    }
                }
            }

            return _list.ToList();
        }

        public static SalesRepPermission GetForTag(string tag)
        {
            return GetAll().FirstOrDefault(o => o.Tag == tag);
        }

    }

    public enum SalesRepPermissionGroup
    {
        Admin = 1
        , SalesRep = 2
        , UserMaintenance = 3
        , EmailCC = 255
    }
}
