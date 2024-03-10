using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.PDR
{
    public class PDR_Rate : ProEstEntity
    {
        public int ID { get; private set; }
        public int RateProfileID { get; set; }
        public PDR_Size Size { get; private set; }
        public decimal Amount { get; set; }
        public PDR_Panel Panel { get; private set; }
        public PDR_Depth Depth { get; private set; }
        public PDR_Quantity Quantity { get; private set; }

        public static PDR_Rate GetByID(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("PDR_Rate_Get", new SqlParameter("RateID", id));
            if (result.Success)
            {
                PDR_Rate rate = new PDR_Rate();
                rate.LoadData(result.DataTable.Rows[0]);
                return rate;
            }

            return null;
        }

        public static List<PDR_Rate> GetByProfile(int profileID)
        {
            List<PDR_Rate> results = new List<PDR_Rate>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("PDR_Rate_Get", new SqlParameter("RateProfileID", profileID));
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    PDR_Rate rateProfile = new PDR_Rate();
                    rateProfile.LoadData(row);

                    if (rateProfile.Panel != null)  // Remove bad records with no panel
                    {
                        results.Add(rateProfile);
                    }                    
                }
            }

            return results;
        }

        public SaveResult Save(int activeLoginID = 0, int loginID = 0, string name = "")
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("RateProfileID", RateProfileID));
            parameters.Add(new SqlParameter("Size", (int)Size));
            parameters.Add(new SqlParameter("Amount", Amount));
            parameters.Add(new SqlParameter("QuantityID", Quantity == null ? 0 : Quantity.ID));
            parameters.Add(new SqlParameter("PanelID", Panel.ID));
            parameters.Add(new SqlParameter("Depth", (int)Depth));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdatePDRRate", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "PDRRate", ID, loginID, parameters, RowAsLoaded, name);
            }

            return new SaveResult(result);
        }

        public void Delete()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("PDR_Rate_Delete", new SqlParameter("RateID", ID));
        }

        public void LoadData(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            RateProfileID = InputHelper.GetInteger(row["RateProfileID"].ToString());
            Size = (PDR_Size)Enum.Parse(typeof(PDR_Size), row["Size"].ToString());
            Amount = InputHelper.GetDecimal(row["Amount"].ToString());
            Panel = PDR_Panel.GetByID(InputHelper.GetInteger(row["PanelID"].ToString()));
            Depth = (PDR_Depth)Enum.Parse(typeof(PDR_Depth), row["Depth"].ToString());

            int quantityID = InputHelper.GetInteger(row["Quantity"].ToString());
            Quantity = PDR_Quantity.GetByID(quantityID);

            RowAsLoaded = row;
        }
    }


}
