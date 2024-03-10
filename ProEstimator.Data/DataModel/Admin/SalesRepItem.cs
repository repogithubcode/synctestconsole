using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    public class SalesRepItem
    {
        public int SalesRepID { get; set; }
        public string FullName { get; set; }

        public SalesRepItem()
        {

        }

        public SalesRepItem(DataRow row)
        {
            SalesRepID = InputHelper.GetInteger(row["SalesRepID"].ToString());
            FullName = InputHelper.GetString(row["FullName"].ToString());
        }

        public static List<SalesRepItem> GetForFilter()
        {
            List<SalesRepItem> salesRepItems = new List<SalesRepItem>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("SalesRepID", null));

            DBAccessTableResult tableResult = db.ExecuteWithTable("GetSalesReps2", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                salesRepItems.Add(new SalesRepItem(row));
            }

            return salesRepItems;
        }
    }
}