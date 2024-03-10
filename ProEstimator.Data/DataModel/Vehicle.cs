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
    public class Vehicle : ProEstEntity
    {
        public int EstimateID { get; set; }
        public int VehicleInfoID { get; private set; }

        public int VehicleID { get; set; }
        public string Vin { get; set; }
        public int Year { get; set; }
        public decimal MileageIn { get; set; }
        public decimal MileageOut { get; set; }
        public string License { get; set; }
        public string VehicleNotes { get; set; }
        public string ExteriorColor { get; set; }
        public string ExteriorColor2 { get; set; }
        public string InteriorColor { get; set; }
        public int DriveType { get; set; }
        public decimal EstimatedValue { get; set; }
        public int MakeID { get; set; }
        public int ModelID { get; set; }
        public int TrimID { get; set; }
        public int EngineID { get; set; }
        public int TransID { get; set; }
        public int BodyID { get; set; }
        public int DefaultPaintType { get; set; }
        public string PaintCode { get; set; }
        public string PaintCode2 { get; set; }
        public string ServiceBarcode { get; set; }
        public string LicenseState { get; set; }
        public string StockNumber { get; set; }

        public int POILabel1 { get; set; }
        public int POIOption1 { get; set; }
        public string POICustom1 { get; set; }

        public int POILabel2 { get; set; }
        public int POIOption2 { get; set; }
        public string POICustom2 { get; set; }

        public string ProductionDate { get; set; }

        public bool HasData { get; private set; }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AdmininfoID", EstimateID));
            parameters.Add(new SqlParameter("VehicleID", VehicleID));
            parameters.Add(new SqlParameter("MilesIn ", MileageIn));
            parameters.Add(new SqlParameter("MilesOut", MileageOut));
            parameters.Add(new SqlParameter("License", GetString(License)));
            parameters.Add(new SqlParameter("Condition", GetString(VehicleNotes)));
            parameters.Add(new SqlParameter("ExtColor", GetString(ExteriorColor)));
            parameters.Add(new SqlParameter("ExtColor2", GetString(ExteriorColor2)));
            parameters.Add(new SqlParameter("IntColor", GetString(InteriorColor)));
            parameters.Add(new SqlParameter("TrimLevel", TrimID));
            parameters.Add(new SqlParameter("Vin", GetString(Vin)));
            parameters.Add(new SqlParameter("VinDecode", GetString(Vin)));
            parameters.Add(new SqlParameter("DriveType", DriveType));
            parameters.Add(new SqlParameter("VehicleValue", EstimatedValue));
            parameters.Add(new SqlParameter("Year", Year));
            parameters.Add(new SqlParameter("MakeID", MakeID));
            parameters.Add(new SqlParameter("ModelID", ModelID));
            parameters.Add(new SqlParameter("SubModelID", TrimID));
            parameters.Add(new SqlParameter("EngineType", EngineID));
            parameters.Add(new SqlParameter("TransmissionType", TransID));
            parameters.Add(new SqlParameter("paintcode", GetString(PaintCode)));
            parameters.Add(new SqlParameter("PaintCode2", GetString(PaintCode2)));
            parameters.Add(new SqlParameter("BodyType", BodyID));
            parameters.Add(new SqlParameter("Service_Barcode", GetString(ServiceBarcode)));
            parameters.Add(new SqlParameter("DefaultPaintType", DefaultPaintType));
            parameters.Add(new SqlParameter("State", GetString(LicenseState)));
            parameters.Add(new SqlParameter("ProductionDate", GetString(ProductionDate)));
            parameters.Add(new SqlParameter("StockNumber", GetString(StockNumber)));
            parameters.Add(new SqlParameter("POILabel1", POILabel1));
            parameters.Add(new SqlParameter("POIOption1", POIOption1));
            parameters.Add(new SqlParameter("POICustom1", POICustom1));
            parameters.Add(new SqlParameter("POILabel2", POILabel2));
            parameters.Add(new SqlParameter("POIOption2", POIOption2));
            parameters.Add(new SqlParameter("POICustom2", POICustom2));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("sp_InsertUpdateVehicleInfo", parameters);
            if (result.Success)
            {
                VehicleInfoID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "VehicleInfo", VehicleInfoID, ChangeLogManager.GetLoginIDFromEstimate(EstimateID), parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static Vehicle GetByEstimate(int estimateID)
        {
            if (estimateID < 1)
            {
                return null;
            }

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("sp_getVehicledetails", new SqlParameter("AdminInfoId", estimateID));
            if (tableResult.Success)
            {
                Vehicle vehicle = new Vehicle();
                vehicle.EstimateID = estimateID;
                vehicle.LoadData(tableResult.DataTable.Rows[0]);
                return vehicle;
            }

            // For some reason there was no Vehicle found in the database.  Make a new blank record and return it
            string insertQuery = "INSERT INTO VehicleInfo (EstimationDataId) SELECT id FROM EstimationData WHERE AdminInfoID = " + estimateID;
            db.ExecuteNonQuery(insertQuery, null, false);

            return GetByEstimate(estimateID);
        }

        //public static List<VehicleInfo> GetVINByLoginID(int LoginsID,string VIN)
        //{
        //    List<VehicleInfo> list = new List<VehicleInfo>();
        //    DBAccess dbAccess = new DBAccess();
        //    var parameters = new List<SqlParameter>();
        //    parameters.Add(new SqlParameter("LoginsID", LoginsID));
        //    parameters.Add(new SqlParameter("VIN", VIN.Trim()));
        //    DBAccessTableResult tableResult = dbAccess.ExecuteWithTable("GetVINByLoginsID", parameters);

        //    if (tableResult.Success)
        //    {
        //        foreach (DataRow row in tableResult.DataTable.Rows)
        //        {
        //            list.Add(new VehicleInfo
        //            {
        //                Vin = row.ItemArray[0].ToString(),
        //                EstimationData = new EstimationData { AdminInfoID = (Int32)row.ItemArray[1] }
        //            });
        //        }
        //    }

        //    return list;
        //}

        private void LoadData(DataRow row)
        {
            VehicleInfoID = InputHelper.GetInteger(row["id"].ToString());

            VehicleID = InputHelper.GetInteger(row["VehicleID"].ToString());
            Vin = row["Vin"].ToString();
            if (!string.IsNullOrEmpty(Vin))
            {
                Vin = Vin.ToUpper();
            }
            Year = InputHelper.GetInteger(row["Year"].ToString());
            MileageIn = InputHelper.GetDecimal(row["MilesIn"].ToString());
            MileageOut = InputHelper.GetDecimal(row["MilesOut"].ToString());
            License = row["License"].ToString();
            VehicleNotes = row["Condition"].ToString();
            ExteriorColor = row["ExtColor"].ToString();
            ExteriorColor2 = row["ExtColor2"].ToString();
            InteriorColor = row["IntColor"].ToString();
            DriveType = InputHelper.GetInteger(row["DriveType"].ToString());
            EstimatedValue = InputHelper.GetDecimal(row["VehicleValue"].ToString());
            MakeID = InputHelper.GetInteger(row["MakeID"].ToString());
            ModelID = InputHelper.GetInteger(row["ModelID"].ToString());
            TrimID = InputHelper.GetInteger(row["SubModelID"].ToString());
            ServiceBarcode = row["Service_Barcode"].ToString();
            EngineID = InputHelper.GetInteger(row["EngineType"].ToString());
            TransID = InputHelper.GetInteger(row["TransmissionType"].ToString());
            BodyID = InputHelper.GetInteger(row["BodyType"].ToString());
            DefaultPaintType = InputHelper.GetInteger(row["DefaultPaintType"].ToString());
            PaintCode = row["paintcode"].ToString();
            PaintCode2 = InputHelper.GetString(row["PaintCode2"].ToString());
            LicenseState = row["State"].ToString();
            ProductionDate = row["ProductionDate"].ToString();
            StockNumber = row["StockNumber"].ToString();

            POILabel1 = InputHelper.GetInteger(row["POILabel1"].ToString());
            POIOption1 = InputHelper.GetInteger(row["POIOption1"].ToString());
            POICustom1 = InputHelper.GetString(row["POICustom1"].ToString());

            POILabel2 = InputHelper.GetInteger(row["POILabel2"].ToString());
            POIOption2 = InputHelper.GetInteger(row["POIOption2"].ToString());
            POICustom2 = InputHelper.GetString(row["POICustom2"].ToString());

            HasData = InputHelper.GetBoolean(row["Data"].ToString());

            RowAsLoaded = row;
        }

        /// <summary>
        /// A helper function to get the database VehicleID based on the passed make/model data
        /// </summary>
        public static VehicleIDResult GetVehicleIDFromInfo(int year, int makeID, int modelID, int subModelID = 0, int? bodyID = null)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Year", year));
            parameters.Add(new SqlParameter("MakeID", makeID));
            parameters.Add(new SqlParameter("ModelID", modelID));
            parameters.Add(new SqlParameter("TrimLevelID", subModelID));
            parameters.Add(new SqlParameter("BodyID", bodyID));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("getVehicleID", parameters);
            if (result.Success)
            {
                int vehicleID = InputHelper.GetInteger(result.DataTable.Rows[0]["VehicleID"].ToString());
                string serviceBarcode = result.DataTable.Rows[0]["Service_Barcode"].ToString();
                bool hasData = InputHelper.GetBoolean(result.DataTable.Rows[0]["Data"].ToString());
                bool carryUp = InputHelper.GetBoolean(result.DataTable.Rows[0]["CarryUp"].ToString());

                return new VehicleIDResult(vehicleID, serviceBarcode, hasData, carryUp);
            }

            return null;
        }

        /// <summary>
        /// A helper to get the Make and Model as a string for the passed estimate ID
        /// </summary>
        public static string GetMakeAndModelString(int estimateID)
        {
            DBAccess dbAccess = new DBAccess();
            DBAccessTableResult tableResult = dbAccess.ExecuteWithTable("GetMakeModel", new SqlParameter("AdminInfoID", estimateID));

            if (tableResult.Success && tableResult.DataTable.Rows.Count > 0)
            {
                return tableResult.DataTable.Rows[0]["Make"].ToString() + " " + tableResult.DataTable.Rows[0]["Model"].ToString();
            }

            return "";
        }

        public List<AccessoryInfo> GetAccessoryList()
        {
            List<AccessoryInfo> list = new List<AccessoryInfo>();

            DBAccess dbAccess = new DBAccess();
            DBAccessTableResult tableResult = dbAccess.ExecuteWithTable("Accessories_GetAll", new SqlParameter("AdminInfoID", this.EstimateID));

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    list.Add(new AccessoryInfo(row));
                }
            }

            return list;
        }

        public void SaveAccessoryList(List<AccessoryInfo> accessoryList)
        {
            DBAccess dbAccess = new DBAccess();
            dbAccess.ExecuteNonQuery("Accessories_DeleteLinks", new SqlParameter("AdminInfoID", this.EstimateID));

            foreach (AccessoryInfo accessoryInfo in accessoryList.Where(o => o.IsChecked))
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("AdminInfoID", this.EstimateID));
                parameters.Add(new SqlParameter("AccessoryID", accessoryInfo.ID));
                dbAccess.ExecuteNonQuery("Accessories_InsertLink", parameters);
            }
        }
    }

    public class AccessoryInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get; set; }
        public string IsCheckedString
        {
            get
            {
                return IsChecked ? "checked=\"checked\"" : "";
            }
        }

        public AccessoryInfo(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Name = row["Accessory"].ToString();
            IsChecked = InputHelper.GetBoolean(row["Included"].ToString());
        }

        public AccessoryInfo()
        {

        }
    }

    public class VehicleIDResult
    {
        public int VehicleID { get; private set; }
        public string ServiceBarcode { get; private set; }
        public bool HasData { get; private set; }
        public bool CarryUp { get; private set; }

        public VehicleIDResult(int vehicleID, string serviceBarcode, bool hasData, bool carryUp)
        {
            VehicleID = vehicleID;
            ServiceBarcode = serviceBarcode;
            HasData = hasData;
            CarryUp = carryUp;
        }
    }
}
