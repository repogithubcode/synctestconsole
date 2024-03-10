using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.PDR
{
    public class PDR_EstimateData : ProEstEntity
    {

        public int ID { get; private set; }
        public int AdminInfoID { get; set; }
        public int RateProfileID { get; set; }

        public PDR_EstimateData()
        {

        }

        public PDR_EstimateData(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            AdminInfoID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            RateProfileID = InputHelper.GetInteger(row["RateProfileID"].ToString());

            RowAsLoaded = row;
        }

        public static PDR_EstimateData GetForEstimate(int estimateID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("PDR_EstimateData_Get", new SqlParameter("AdminInfoID", estimateID));
            if (tableResult != null && tableResult.Success && tableResult.DataTable.Rows.Count > 0)
            {
                return new PDR_EstimateData(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("AdminInfoID", AdminInfoID));
            parameters.Add(new SqlParameter("RateProfileID", RateProfileID));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdatePDREstimateData", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "PDREstimateData", ID, ChangeLogManager.GetLoginIDFromEstimate(AdminInfoID), parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        } 

    }
}
