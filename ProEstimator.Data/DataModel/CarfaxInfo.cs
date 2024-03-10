//using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class CarfaxInfo : ProEstEntity
    {
        public int ID { get; set; }
        public string Vin { get; set; }
        public string VinInfo { get; set; }

        public CarfaxInfo()
        {
            ID = 0;
            Vin = "";
            VinInfo = "";
        }

        public CarfaxInfo(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Vin = InputHelper.GetString(row["Vin"].ToString());
            VinInfo = InputHelper.GetString(row["VinInfo"].ToString());

            RowAsLoaded = row;
        }

        public static CarfaxInfo GetByVin(string vin)
        {
            if (string.IsNullOrEmpty(vin))
            {
                return null;
            }

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetCarfaxInfoByVin", new SqlParameter("Vin", vin));
            if (result.Success)
            {
                return new CarfaxInfo(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<string> GetAllVin()
        {
            List<string> results = new List<string>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CarfaxInfo_GetAllVin");

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                results.Add(row[0].ToString());
            }

            return results;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("Vin", Vin));
            parameters.Add(new SqlParameter("VinInfo", VinInfo));
            
            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("CarfaxInfo_Save", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "CarfaxInfo", ID, 0, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }
    }
}
