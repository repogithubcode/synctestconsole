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
    public class VehicleInfoManual : ProEstEntity
    {
        public int VehicleInfoID { get; set; }
        public bool UseManualSelection { get; set; }

        public string Country { get; set; }
        public string Manufacturer { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string ModelYear { get; set; }
        public string SubModel { get; set; }
        public string ServiceBarcode { get; set; }

        public VehicleInfoManual() { }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("vehicleinfoid", VehicleInfoID));
                parameters.Add(new SqlParameter("country", Country));
                parameters.Add(new SqlParameter("manufacturer", Manufacturer));
                parameters.Add(new SqlParameter("make", Make));
                parameters.Add(new SqlParameter("model", Model));
                parameters.Add(new SqlParameter("modelyear", ModelYear));
                parameters.Add(new SqlParameter("submodel", SubModel));
                parameters.Add(new SqlParameter("service_barcode", ServiceBarcode));
                parameters.Add(new SqlParameter("manualSelection", UseManualSelection));

                DBAccess db = new DBAccess();
                DBAccessIntResult intResult = db.ExecuteWithIntReturn("AddOrUpdateVehicleInfoManual", parameters);
                if (intResult.Success)
                {
                    VehicleInfoID = intResult.Value;

                    ChangeLogManager.LogChange(activeLoginID, "VehicleInfoManual", VehicleInfoID, loginID, parameters, RowAsLoaded);
                }
            }
            catch (System.Exception ex)
            {
                return new SaveResult("Error saving manual vehicle data: " + ex.Message);
            }

            return new SaveResult();
        }

        public static VehicleInfoManual GetByEstimate(int estimateID)
        {
            if (estimateID < 1)
            {
                return null;
            }

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("GetVehicleInfoManualByAdminInfo", new SqlParameter("AdminInfoId", estimateID));
            if (tableResult.Success)
            {
                VehicleInfoManual vehicleInfoManual = new VehicleInfoManual();
                vehicleInfoManual.LoadData(tableResult.DataTable.Rows[0]);
                return vehicleInfoManual;
            }

            return null;
        }

        private void LoadData(DataRow row)
        {
            VehicleInfoID = InputHelper.GetInteger(row["VehicleInfoID"].ToString());
            Country = row["Country"].ToString();
            Manufacturer = row["Manufacturer"].ToString();
            Make = row["Make"].ToString();
            Model = row["Model"].ToString();
            ModelYear = row["ModelYear"].ToString();
            SubModel = row["SubModel"].ToString();
            ServiceBarcode = row["Service_Barcode"].ToString();
            UseManualSelection = InputHelper.GetBoolean(row["ManualSelection"].ToString());

            RowAsLoaded = row;
        }

        public static void Delete(int estimateID)
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("DeleteVehicleInfoByAdminInfo", new SqlParameter("admininfoid", estimateID));
        }
    }
}
