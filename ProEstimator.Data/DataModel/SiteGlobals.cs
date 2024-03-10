using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class SiteGlobals : ProEstEntity
    {

        public string HomePageMessage { get; set; }
        public int AluminumEstimateID { get; set; }

        public SiteGlobals()
        {
            HomePageMessage = "";
        }

        public SiteGlobals(DataRow row)
        {
            HomePageMessage = InputHelper.GetString(row["HomePageMessage"].ToString());
            AluminumEstimateID = InputHelper.GetInteger(row["AluminumEstimateID"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("HomePageMessage", InputHelper.GetString(HomePageMessage)));
            parameters.Add(new SqlParameter("AluminumEstimateID", AluminumEstimateID));

            DBAccess db = new DBAccess();
            FunctionResult result = db.ExecuteNonQuery("SiteGlobals_Update", parameters);

            if (result.Success)
            {
                ChangeLogManager.LogChange(activeLoginID, "SiteGlobals", 0, 0, parameters, RowAsLoaded);
                return new SaveResult(); 
            }
            else
            {
                return new SaveResult(result.ErrorMessage);
            }
            
        }

        public static SiteGlobals Get()
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SiteGlobals_Get");
            if (tableResult.Success)
            {
                return new SiteGlobals(tableResult.DataTable.Rows[0]);
            }

            return new SiteGlobals();
        }

    }
}
