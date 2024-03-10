using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class GridColumnInfoUserMapping : ProEstEntity
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public Boolean Visible { get; set; }
        public int SortOrderIndex { get; set; }
        public GridColumnInfo GridColumnInfo { get; set; }

        public GridColumnInfoUserMapping()
        { }

        public GridColumnInfoUserMapping(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            GridColumnInfo = GridColumnInfo.Get(InputHelper.GetInteger(row["GridColumnInfoID"].ToString()));
            UserID = InputHelper.GetInteger(row["UserID"].ToString());
            Visible = InputHelper.GetBoolean(row["Visible"].ToString());
            SortOrderIndex = InputHelper.GetInteger(row["SortOrderIndex"].ToString());

            RowAsLoaded = row;
        }

        public static List<GridColumnInfoUserMapping> GetForUserControlID(int loginID, string gridControlID)
        {
            List<GridColumnInfoUserMapping> results = new List<GridColumnInfoUserMapping>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("GridControlID", gridControlID));
            parameters.Add(new SqlParameter("UserID", loginID));

            DBAccess db = new DBAccess();
            DBAccessTableResult table = db.ExecuteWithTable("GridColumnInfoUserMapping_GetForGridAndUser", parameters);

            foreach (DataRow row in table.DataTable.Rows)
            {
                results.Add(new GridColumnInfoUserMapping(row));
            }

            return results;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> SqlParameterColl = new List<SqlParameter>();
            SqlParameterColl.Add(new SqlParameter("@ID", ID));
            SqlParameterColl.Add(new SqlParameter("@GridColumnInfoID", GridColumnInfo.ID));
            SqlParameterColl.Add(new SqlParameter("@UserID", UserID));
            SqlParameterColl.Add(new SqlParameter("@Visible", Visible));
            SqlParameterColl.Add(new SqlParameter("@SortOrderIndex", SortOrderIndex));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("GridColumnInfoUserMapping_Save", SqlParameterColl);
            if (result.Success)
            {
                ID = result.Value;

                SiteUser siteUser = SiteUser.Get(UserID);
                if(siteUser != null)
                {
                    ChangeLogManager.LogChange(activeLoginID, "GridColumnInfoUserMapping", ID, siteUser.LoginID, SqlParameterColl, RowAsLoaded);
                }
            }

            return new SaveResult(result);
        }
    }
}
