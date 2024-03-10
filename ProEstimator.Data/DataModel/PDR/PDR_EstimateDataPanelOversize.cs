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
    public class PDR_EstimateDataPanelOversize : ProEstEntity
    {

        public int ID { get; private set; }
        public int EstimateDataPanel { get; set; }
        public PDR_Size Size { get; set; }
        public PDR_Depth Depth { get; set; }
        public decimal Amount { get; set; }
        public int SupplementAdded { get; set; }
        public int SupplementDeleted { get; set; }

        public PDR_EstimateDataPanelOversize()
        {

        }

        public PDR_EstimateDataPanelOversize(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            EstimateDataPanel = InputHelper.GetInteger(row["EstimateDataPanelID"].ToString());
            Size = (PDR_Size)InputHelper.GetInteger(row["Size"].ToString());
            Depth = (PDR_Depth)InputHelper.GetInteger(row["Depth"].ToString());
            Amount = InputHelper.GetDecimal(row["Amount"].ToString());
            SupplementAdded = InputHelper.GetInteger(row["SupplementAdded"].ToString());
            SupplementDeleted = InputHelper.GetInteger(row["SupplementDeleted"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("EstimateDataPanel", EstimateDataPanel));
            parameters.Add(new SqlParameter("Size", (int)Size));
            parameters.Add(new SqlParameter("Depth", (int)Depth));
            parameters.Add(new SqlParameter("Amount", Amount));
            parameters.Add(new SqlParameter("SupplementAdded", SupplementAdded));
            parameters.Add(new SqlParameter("SupplementDeleted", SupplementDeleted));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdatePDREstimateDataPanelOversize", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "PDROversize", ID, loginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static List<PDR_EstimateDataPanelOversize> GetForDataPanelID(int dataPanelID)
        {
            List<PDR_EstimateDataPanelOversize> returnList = new List<PDR_EstimateDataPanelOversize>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("PDR_EstimateDataPanelOversize_GetForDataPanelID", new SqlParameter("EstimateDataPanelID", dataPanelID));
            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new PDR_EstimateDataPanelOversize(row));
                }
            }

            return returnList;
        }

        public static PDR_EstimateDataPanelOversize GetByID(int id)
        {
            List<PDR_EstimateDataPanelOversize> returnList = new List<PDR_EstimateDataPanelOversize>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("PDR_EstimateDataPanelOversize_GetByID", new SqlParameter("ID", id));
            if (tableResult.Success)
            {
                return new PDR_EstimateDataPanelOversize(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public void Delete()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("PDR_EstimateDataPanelOversize_Delete", new SqlParameter("ID", ID));
        }
    }
}
