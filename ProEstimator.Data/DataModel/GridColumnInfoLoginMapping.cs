//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Data;
//using System.Data.SqlClient;

//namespace ProEstimatorData.DataModel
//{
//    public class GridColumnInfoLoginMapping
//    {
//        public int ID { get; set; }
//        public int LoginID { get; set; }
//        public Boolean Visible { get; set; }
//        public int SortOrderIndex { get; set; }
//        public GridColumnInfo GridColumnInfo { get; set; }

//        public GridColumnInfoLoginMapping()
//        { }

//        public GridColumnInfoLoginMapping(DataRow row)
//        {
//            ID = InputHelper.GetInteger(row["ID"].ToString());
//            GridColumnInfo = GridColumnInfo.Get(InputHelper.GetInteger(row["GridColumnInfoID"].ToString()));
//            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
//            Visible = InputHelper.GetBoolean(row["Visible"].ToString());
//            SortOrderIndex = InputHelper.GetInteger(row["SortOrderIndex"].ToString()); 
//        }

//        public static List<GridColumnInfoLoginMapping> GetForLoginControlID(int loginID, string gridControlID)
//        {
//            List<GridColumnInfoLoginMapping> results = new List<GridColumnInfoLoginMapping>();

//            List<SqlParameter> parameters = new List<SqlParameter>();
//            parameters.Add(new SqlParameter("GridControlID", gridControlID));
//            parameters.Add(new SqlParameter("LoginID", loginID));

//            DBAccess db = new DBAccess();
//            DBAccessTableResult table = db.ExecuteWithTable("GridColumnInfoLoginMapping_GetForGridAndLogin", parameters);

//            foreach(DataRow row in table.DataTable.Rows)
//            {
//                results.Add(new GridColumnInfoLoginMapping(row));
//            }
            
//            return results;
//        }

//        public SaveResult Save()
//        {
//            List<SqlParameter> SqlParameterColl = new List<SqlParameter>();
//            SqlParameterColl.Add(new SqlParameter("@ID", ID));
//            SqlParameterColl.Add(new SqlParameter("@GridColumnInfoID", GridColumnInfo.ID));
//            SqlParameterColl.Add(new SqlParameter("@LoginID", LoginID));
//            SqlParameterColl.Add(new SqlParameter("@Visible", Visible));
//            SqlParameterColl.Add(new SqlParameter("@SortOrderIndex", SortOrderIndex));

//            DBAccess db = new DBAccess();
//            DBAccessIntResult result = db.ExecuteWithIntReturn("GridColumnInfoLoginMapping_Save", SqlParameterColl);
//            if (result.Success)
//            {
//                ID = result.Value;
//            }

//            return new SaveResult(result);
//        }
//    }
//}
