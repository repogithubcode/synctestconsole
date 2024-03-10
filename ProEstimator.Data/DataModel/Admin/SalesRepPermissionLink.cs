using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{

    /// <summary>
    /// Links a sales rep with a permission.  
    /// Note: This data is cached in memory, don't create these records manually, only the SalesRepPermissionManager should edit these.
    /// </summary>
    public class SalesRepPermissionLink
    {

        public int ID { get; private set; }
        public int SalesRepID { get; set; }
        public int PermissionID { get; set; }
        public bool HasPermission { get; set; }

        public SalesRepPermissionLink()
        {

        }

        public SalesRepPermissionLink(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            SalesRepID = InputHelper.GetInteger(row["SalesRepID"].ToString());
            PermissionID = InputHelper.GetInteger(row["PermissionID"].ToString());
            HasPermission = InputHelper.GetBoolean(row["HasPermission"].ToString());
        }

        public SaveResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("SalesRepID", SalesRepID));
            parameters.Add(new SqlParameter("PermissionID", PermissionID));
            parameters.Add(new SqlParameter("HasPermission", HasPermission));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("SalesRepPermissionLink_Save", parameters);
            if (result.Success)
            {
                ID = result.Value;
            }

            return new SaveResult(result.ErrorMessage);
        }

        public static Dictionary<int, List<SalesRepPermissionLink>> GetAll()
        {
            Dictionary<int, List<SalesRepPermissionLink>> data = new Dictionary<int, List<SalesRepPermissionLink>>();

            DBAccess db = new DBAccess();
            DBAccessTableResult table = db.ExecuteWithTable("SalesRepPermissionLink_GetAll");

            foreach (DataRow row in table.DataTable.Rows)
            {
                SalesRepPermissionLink link = new SalesRepPermissionLink(row);
                
                if (!data.ContainsKey(link.SalesRepID))
                {
                    data.Add(link.SalesRepID, new List<SalesRepPermissionLink>());
                }

                data[link.SalesRepID].Add(link);
            }

            return data;
        }

    }
}
