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
    public class PDR_Multiplier : ProEstEntity 
    {

        public int ID { get; private set; }
        public int RateProfileID { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }

        public PDR_Multiplier()
        {
            ID = 0;
            RateProfileID = 0;
            Name = "";
            Value = 0;
        }

        public PDR_Multiplier(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            RateProfileID = InputHelper.GetInteger(row["RateProfileID"].ToString());
            Name = row["Name"].ToString();
            Value = InputHelper.GetDouble(row["Value"].ToString());

            RowAsLoaded = row;
        }

        public static PDR_Multiplier GetByID(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("PDR_Multiplier_Get", new SqlParameter("MultiplierID", id));
            if (result.Success)
            {
                return new PDR_Multiplier(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<PDR_Multiplier> GetByProfile(int profileID)
        {
            List<PDR_Multiplier> results = new List<PDR_Multiplier>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("PDR_Multiplier_Get", new SqlParameter("RateProfileID", profileID));
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    results.Add(new PDR_Multiplier(row));
                }
            }

            return results;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("RateProfileID", RateProfileID));
            parameters.Add(new SqlParameter("Name", Name));
            parameters.Add(new SqlParameter("Value", Value));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdatePDRMultiplier", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "PDRMultiplier", ID, loginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public void Delete()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("PDR_Multiplier_Delete", new SqlParameter("MultiplierID", ID));
        }

    }
}