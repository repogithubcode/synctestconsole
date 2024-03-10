using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;


namespace ProEstimatorData.DataModel
{
    public class OverlapDetails : ProEstEntity
    {

        public int OverlapID { get; private set; }
        public string OverlapAdjacentFlag { get; private set; }
        public double Amount { get; set; }
        public double Minimum { get; set; }
        public bool UserAccepted { get; set; }
        public string Description { get; private set; }

        public OverlapDetails(DataRow row)
        {
            OverlapID = InputHelper.GetInteger(row["OverlapID"].ToString());
            OverlapAdjacentFlag = InputHelper.GetString(row["OverlapAdjacentFlag"].ToString());
            Amount = InputHelper.GetDouble(row["Amount"].ToString());
            Minimum = InputHelper.GetDouble(row["Minimum"].ToString());
            UserAccepted = InputHelper.GetBoolean(row["UserAccepted"].ToString());
            Description = InputHelper.GetString(row["Description"].ToString());

            RowAsLoaded = row;
        }

        public static List<OverlapDetails> GetForLineItem(int lineItemID)
        {
            List<OverlapDetails> returnList = new List<OverlapDetails>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Overlaps_GetForLine", new SqlParameter("LineItemID", lineItemID));

            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new OverlapDetails(row));
                }
            }

            return returnList;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("id", OverlapID));
            parameters.Add(new SqlParameter("OverlapAdjacentFlag", OverlapAdjacentFlag));
            parameters.Add(new SqlParameter("Amount", Amount));
            parameters.Add(new SqlParameter("Minimum", Minimum));
            parameters.Add(new SqlParameter("UserAccepted", UserAccepted));

            DBAccess db = new DBAccess();
            FunctionResult result = db.ExecuteNonQuery("Overlaps_UpdateDetails", parameters);
            ChangeLogManager.LogChange(activeLoginID, "OverlapDetails", OverlapID, loginID, parameters, RowAsLoaded);
            return new SaveResult(result);
        }
    }
}
