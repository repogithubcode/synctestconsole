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
    public class PDR_EstimateDataPanel : ProEstEntity
    {

        public int ID { get; private set; }
        public int AdminInfoID { get; private set; }
        public PDR_Panel Panel { get; private set; }
        public PDR_Quantity Quantity { get; set; }
        public PDR_Size Size { get; set; }
        public int OversizedDents { get; set; }
        public int Multiplier { get; set; }
        public string Description { get; set; }
        public double CustomCharge { get; set; }
        public bool Expanded { get; set; }
        public int LineNumber { get; set; }

        public PDR_EstimateDataPanel(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            AdminInfoID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            Panel = PDR_Panel.GetByID(InputHelper.GetInteger(row["PanelID"].ToString()));
            Quantity = PDR_Quantity.GetByID(InputHelper.GetInteger(row["QuantityID"].ToString()));
            Size = (PDR_Size)Enum.Parse(typeof(PDR_Size), row["SizeID"].ToString());
            OversizedDents = InputHelper.GetInteger(row["OversizedDents"].ToString());
            Multiplier = InputHelper.GetInteger(row["Multiplier"].ToString());
            Description = row["Description"].ToString();
            CustomCharge = InputHelper.GetDouble(row["CustomCharge"].ToString());
            Expanded = InputHelper.GetBoolean(row["Expanded"].ToString());
            LineNumber = InputHelper.GetInteger(row["LineNumber"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("AdminInfoID", AdminInfoID));
            parameters.Add(new SqlParameter("PanelID", Panel.ID));
            parameters.Add(new SqlParameter("QuantityID", Quantity == null ? 0 : Quantity.ID));
            parameters.Add(new SqlParameter("SizeID", Quantity == null ? 0 : Size));
            parameters.Add(new SqlParameter("OversizedDents", OversizedDents.ToString()));
            parameters.Add(new SqlParameter("Multiplier", Multiplier.ToString()));
            parameters.Add(new SqlParameter("Description", Description == null ? "" : Description));
            parameters.Add(new SqlParameter("CustomCharge", CustomCharge));
            parameters.Add(new SqlParameter("Expanded", Expanded));
            parameters.Add(new SqlParameter("LineNumber", LineNumber));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdatePDREstimateDataPanel", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "PDREstimateDataPanel", ID, ChangeLogManager.GetLoginIDFromEstimate(AdminInfoID), parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public SaveResult ClearData(int currentSupplement, int lineNumber, int activeLoginID, int loginID)
        {
            if (currentSupplement == 0)
            {
                Quantity = null;
                Size = PDR_Size.None;
                OversizedDents = 0;
                Multiplier = 0;
                Description = "";
                Expanded = false;
                LineNumber = 0;
                CustomCharge = 0;

                List<PDR_EstimateDataPanelOversize> oversizedList = PDR_EstimateDataPanelOversize.GetForDataPanelID(this.ID);
                foreach(PDR_EstimateDataPanelOversize oversized in oversizedList)
                {
                    oversized.Delete();
                }

                return Save(activeLoginID);
            }
            else
            {
                List<PDR_EstimateDataPanelSupplementChange> allSupplementData = PDR_EstimateDataPanelSupplementChange.GetForEstimateDataPanel(this.ID);

                if (allSupplementData == null)
                {
                    return new SaveResult("Supplement data not found.");
                }
                else
                {
                    PDR_EstimateDataPanelSupplementChange supplementData = allSupplementData.FirstOrDefault(o => o.SupplementVersion == currentSupplement);
                    if (supplementData == null)
                    {
                        supplementData = new PDR_EstimateDataPanelSupplementChange();
                        supplementData.EstimateDataPanelID = this.ID;
                    }

                    if (supplementData.SupplementVersion == currentSupplement)
                    {
                        FunctionResult deleteResult = supplementData.Delete();
                        if (deleteResult.Success)
                        {
                            return new SaveResult();
                        }
                        else
                        {
                            return new SaveResult(deleteResult.ErrorMessage);
                        }
                    }
                    else
                    {
                        supplementData.SupplementVersion = currentSupplement;

                        supplementData.Quantity = null;
                        supplementData.Size = PDR_Size.None;
                        supplementData.OversizedDents = 0;
                        supplementData.Multiplier = 0;
                        supplementData.LineNumber = lineNumber;
                        supplementData.CustomCharge = 0;

                        List<PDR_EstimateDataPanelOversize> oversizedList = PDR_EstimateDataPanelOversize.GetForDataPanelID(this.ID);
                        foreach (PDR_EstimateDataPanelOversize oversized in oversizedList)
                        {
                            oversized.Delete();
                        }

                        return supplementData.Save(activeLoginID, loginID);
                    }
                }
            }
        }

        public static List<PDR_EstimateDataPanel> GetForEstimate(int estimateID)
        {
            List<PDR_EstimateDataPanel> returnList = new List<PDR_EstimateDataPanel>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("PDR_EstimateDataPanel_GetForEstimate", new SqlParameter("AdminInfoID", estimateID));
            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new PDR_EstimateDataPanel(row));
                }
            }

            return returnList;
        }

        public static PDR_EstimateDataPanel GetByID(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("PDR_EstimateDataPanel_GetByID", new SqlParameter("id", id));
            if (tableResult.Success)
            {
                return new PDR_EstimateDataPanel(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public static SaveResult CreateEmptyListForEstimate(int estimateID)
        {
            DBAccess db = new DBAccess();
            FunctionResult result = db.ExecuteNonQuery("PDR_EstimateDataPanel_CreateForEstimate", new SqlParameter("AdminInfoID", estimateID));
            return new SaveResult(result.ErrorMessage);
        }

    }
}