using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class SitePermissionLink
    {

        public int ID { get; private set; }
        public int SiteUserID { get; set; }
        public int PermissionID { get; set; }
        public bool HasPermission { get; set; }

        public SitePermissionLink()
        {

        }

        public SitePermissionLink(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            SiteUserID = InputHelper.GetInteger(row["SiteUserID"].ToString());
            PermissionID = InputHelper.GetInteger(row["PermissionID"].ToString());
            HasPermission = InputHelper.GetBoolean(row["HasPermission"].ToString());
        }

        public SaveResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("SiteUserID", SiteUserID));
            parameters.Add(new SqlParameter("PermissionID", PermissionID));
            parameters.Add(new SqlParameter("HasPermission", HasPermission));

            DBAccess db = new DBAccess();
            FunctionResult result = db.ExecuteNonQuery("SitePermissionLinks_Update", parameters);

            return new SaveResult(result.ErrorMessage);
        }

        public static List<SitePermissionLink> GetForUser(int siteUserID)
        {
            List<SitePermissionLink> links = new List<SitePermissionLink>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SitePermissionLinks_GetForUser", new SqlParameter("SiteUserID", siteUserID));

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                links.Add(new SitePermissionLink(row));
            }

            return links;
        }
    }
}
