using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class EstimateLineLabor : ProEstEntity 
    {
        public int ID { get; private set; }
        public int EstimateLineID { get; set; }
        public LaborType LaborType { get; set; }
        public double LaborTime { get; set; }
        public double LaborCost { get; set; }
        public bool BettermentFlag { get; set; }
        public bool SubletFlag { get; set; }
        public int UniqueSequenceNumber { get; set; }
        public int ModifiesID { get; set; }
        public int AdjacentDeduction { get; set; }
        public bool MajorPanel { get; set; }
        public double BettermentPercent { get; set; }
        public bool Lock { get; set; }
        public bool Include { get; set; }

        public static EstimateLineLabor GetByID(int id)
        {
            List<EstimateLineLabor> results = Get(id, null);
            if (results.Count > 0)
            {
                return results[0];
            }
            return null;
        }

        public static List<EstimateLineLabor> GetByEstimationLineItemsID(int estimationLineItemsID)
        {
            return Get(0, estimationLineItemsID);
        }

        private static List<EstimateLineLabor> Get(int? id, int? estimationLineItemsID)
        {
            List<EstimateLineLabor> returnList = new List<EstimateLineLabor>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            
            if (id.HasValue && id.Value > 0)
            {
                parameters.Add(new SqlParameter("id", id.Value));
            }

            if (estimationLineItemsID.HasValue && estimationLineItemsID.Value > 0)
            {
                parameters.Add(new SqlParameter("EstimationLineItemsID", estimationLineItemsID));
            }

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("GetEstimationLineLabor", parameters);
            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new EstimateLineLabor(row));
                }
            }

            return returnList;
        }

        public EstimateLineLabor(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            EstimateLineID = InputHelper.GetInteger(row["EstimationLineItemsID"].ToString());
            LaborType = (LaborType)InputHelper.GetInteger(row["LaborType"].ToString());
            LaborTime = InputHelper.GetDouble(row["LaborTime"].ToString());
            LaborCost = InputHelper.GetDouble(row["LaborCost"].ToString());
            BettermentFlag = InputHelper.GetBoolean(row["BettermentFlag"].ToString());
            SubletFlag = InputHelper.GetBoolean(row["SubletFlag"].ToString());
            UniqueSequenceNumber = InputHelper.GetInteger(row["UniqueSequenceNumber"].ToString());
            ModifiesID = InputHelper.GetInteger(row["ModifiesID"].ToString());
            AdjacentDeduction = InputHelper.GetInteger(row["AdjacentDeduction"].ToString());
            MajorPanel = InputHelper.GetBoolean(row["MajorPanel"].ToString());
            BettermentPercent = InputHelper.GetDouble(row["BettermentPercentage"].ToString());
            Lock = InputHelper.GetBoolean(row["Lock"].ToString());
            Include = InputHelper.GetBoolean(row["Include"].ToString());
        }

        public SaveResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("id", ID));
            parameters.Add(new SqlParameter("EstimationLineItemsID", EstimateLineID));
            parameters.Add(new SqlParameter("LaborType", LaborType));
            parameters.Add(new SqlParameter("LaborTime", LaborTime));
            parameters.Add(new SqlParameter("LaborCost", LaborCost));
            parameters.Add(new SqlParameter("BettermentFlag", BettermentFlag));
            parameters.Add(new SqlParameter("SubletFlag", SubletFlag));
            parameters.Add(new SqlParameter("UniqueSequenceNumber", UniqueSequenceNumber));
            parameters.Add(new SqlParameter("ModifiesID", ModifiesID));
            parameters.Add(new SqlParameter("AdjacentDeduction", AdjacentDeduction));
            parameters.Add(new SqlParameter("MajorPanel", MajorPanel));
            parameters.Add(new SqlParameter("BettermentPercentage", BettermentPercent));
            parameters.Add(new SqlParameter("Lock", Lock));
            parameters.Add(new SqlParameter("Include", Include));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdateEstimateLineLabor", parameters);
            if (result.Success)
            {
                ID = result.Value;
            }

            return new SaveResult(result);
        } 

        public void Delete()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("DeleteEstimationLineLabor", new SqlParameter("id", this.ID));
        }

        public static void DeleteByEstimationLineItemsID(int estimationLineItemsID)
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("DeleteEstimationLineLabor", new SqlParameter("EstimationLineItemsID", estimationLineItemsID));
        }
    }
}
