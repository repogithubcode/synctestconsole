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
    public class PDR_EstimateDataPanelSupplementChange : ProEstEntity
    {
        public int ID { get; private set; }
        public int EstimateDataPanelID { get; set; }
        public PDR_Quantity Quantity { get; set; }
        public PDR_Size Size { get; set; }
        public int OversizedDents { get; set; }
        public int Multiplier { get; set; }
        public double CustomCharge { get; set; }
        public int SupplementVersion { get; set; }
        public int LineNumber { get; set; }

        public PDR_EstimateDataPanelSupplementChange()
        {
            EstimateDataPanelID = 0;
            Quantity = null;
            Size = PDR_Size.None;
            OversizedDents = 0;
            Multiplier = 0;
            CustomCharge = 0;
            SupplementVersion = 0;
            LineNumber = 0;
        }

        public PDR_EstimateDataPanelSupplementChange(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            EstimateDataPanelID = InputHelper.GetInteger(row["EstimateDataPanelID"].ToString());
            Quantity = PDR_Quantity.GetByID(InputHelper.GetInteger(row["QuantityID"].ToString()));
            Size = (PDR_Size)Enum.Parse(typeof(PDR_Size), row["SizeID"].ToString());
            OversizedDents = InputHelper.GetInteger(row["OversizedDents"].ToString());
            Multiplier = InputHelper.GetInteger(row["Multiplier"].ToString());
            CustomCharge = InputHelper.GetDouble(row["CustomCharge"].ToString());
            SupplementVersion = InputHelper.GetInteger(row["SupplementVersion"].ToString());
            LineNumber = InputHelper.GetInteger(row["LineNumber"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("EstimateDataPanelID", EstimateDataPanelID));
            parameters.Add(new SqlParameter("QuantityID", Quantity == null ? 0 : Quantity.ID));
            parameters.Add(new SqlParameter("SizeID", Size == null ? 0 : Size));
            parameters.Add(new SqlParameter("OversizedDents", OversizedDents.ToString()));
            parameters.Add(new SqlParameter("Multiplier", Multiplier));
            parameters.Add(new SqlParameter("CustomCharge", CustomCharge));
            parameters.Add(new SqlParameter("SupplementVersion", SupplementVersion));
            parameters.Add(new SqlParameter("LineNumber", LineNumber));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdatePDREstimateDataPanelSupplementChange", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "PDRSupplementChange", ID, loginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public FunctionResult Delete()
        {
            DBAccess db = new DBAccess();
            return db.ExecuteNonQuery("PDR_EstimateDataPanelSupplementChange_Delete", new SqlParameter("ID", ID));
        }

        public static List<PDR_EstimateDataPanelSupplementChange> GetForEstimateDataPanel(int dataPanelID)
        {
            List<PDR_EstimateDataPanelSupplementChange> returnList = new List<PDR_EstimateDataPanelSupplementChange>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("PDR_EstimateDataPanelSupplementChange_GetByEstimateDataPanelID", new SqlParameter("estimateDataPanelID", dataPanelID));
            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new PDR_EstimateDataPanelSupplementChange(row));
                }
            }

            return returnList;
        }

        public static List<PDR_EstimateDataPanelSupplementChange> GetForEstimate(int estimateID)
        {
            List<PDR_EstimateDataPanelSupplementChange> returnList = new List<PDR_EstimateDataPanelSupplementChange>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("PDR_EstimateDataPanelSupplementChange_GetByEstimateID", new SqlParameter("estimateID", estimateID));
            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new PDR_EstimateDataPanelSupplementChange(row));
                }
            }

            return returnList;
        }

    }
}
