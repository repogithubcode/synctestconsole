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
    public class EstimateLineItem
    {
        public int ID { get; set; }
        public int EstimationDataID	{ get; set; }
        public int StepID { get; set; }
        public string PartNumber { get; set; }
        public string PartSource { get; set; }
        public string ActionCode { get; set; }
        public string Description { get; set; }
        public double Price	{ get; set; }
        public int Quantity	{ get; set; }
        public int ImageID { get; set; }
        public string ActionDescription	{ get; set; }
        public bool PartOfOverhaul { get; set; }
        public int PartSourceVendorID { get; set; }
        public bool BettermentParts	{ get; set; }
        public bool SubletPartsFlag	{ get; set; }
        public bool BettermentMaterials	{ get; set; }
        public bool SubletOperationFlag	{ get; set; }
        public int SupplementVersion { get; set; }
        public int LineNumber { get; set; }
        public int UniqueSequenceNumber	{ get; set; }
        public int ModifiesID { get; set; }
        public string ACDCode { get; set; }
        public decimal CustomerPrice { get; set; }
        public bool AutomaticCharge { get; set; }
        public string SourcePartNumber { get; set; }
        public int SectionID { get; set; }
        public string VehiclePosition { get; set; }
        public string Barcode { get; set; }
        public bool AutoAdd { get; set; }
        public DateTime DateEntered	{ get; set; }
        public string BettermentType { get; set; }
        public double BettermentValue { get; set; }

        public static EstimateLineItem GetByID(int id)
        {
            List<EstimateLineItem> results = Get(id, null);
            if (results.Count > 0)
            {
                return results[0];
            }
            return null;
        }

        public static List<EstimateLineItem> GetByEstimationLineItemsID(int estimationLineItemsID)
        {
            return Get(0, estimationLineItemsID);
        }

        private static List<EstimateLineItem> Get(int? id, int? estimateID)
        {
            List<EstimateLineItem> returnList = new List<EstimateLineItem>();

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (id.HasValue && id.Value > 0)
            {
                parameters.Add(new SqlParameter("id", id.Value));
            }

            if (estimateID.HasValue && estimateID.Value > 0)
            {
                parameters.Add(new SqlParameter("AdminInfoID", estimateID));
            }

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("GetEstimationLineItem", parameters);
            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new EstimateLineItem(row));
                }
            }

            return returnList;
        }

        public EstimateLineItem(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            EstimationDataID = InputHelper.GetInteger(row["EstimationDataID"].ToString());
            StepID = InputHelper.GetInteger(row["StepID"].ToString());
            PartNumber = row["PartNumber"].ToString();
            PartSource = row["PartSource"].ToString();
            ActionCode = row["ActionCode"].ToString();
            Description = row["Description"].ToString();
            Price = InputHelper.GetDouble(row["Price"].ToString());
            Quantity = InputHelper.GetInteger(row["Qty"].ToString());
            ImageID = InputHelper.GetInteger(row["ImageID"].ToString());
            ActionDescription = row["ActionDescription"].ToString();
            PartOfOverhaul = InputHelper.GetBoolean(row["PartOfOverhaul"].ToString());
            PartSourceVendorID = InputHelper.GetInteger(row["PartSourceVendorID"].ToString());
            BettermentParts = InputHelper.GetBoolean(row["BettermentParts"].ToString());
            SubletPartsFlag = InputHelper.GetBoolean(row["SubletPartsFlag"].ToString());
            BettermentMaterials = InputHelper.GetBoolean(row["BettermentMaterials"].ToString());
            SubletOperationFlag = InputHelper.GetBoolean(row["SubletOperationFlag"].ToString());
            SupplementVersion = InputHelper.GetInteger(row["SupplementVersion"].ToString());
            LineNumber = InputHelper.GetInteger(row["LineNumber"].ToString());
            UniqueSequenceNumber = InputHelper.GetInteger(row["UniqueSequenceNumber"].ToString());
            ModifiesID = InputHelper.GetInteger(row["ModifiesID"].ToString());
            ACDCode = row["ACDCode"].ToString();
            CustomerPrice = InputHelper.GetDecimal(row["CustomerPrice"].ToString());
            AutomaticCharge = InputHelper.GetBoolean(row["AutomaticCharge"].ToString());
            SourcePartNumber = row["SourcePartNumber"].ToString();
            SectionID = InputHelper.GetInteger(row["SectionID"].ToString());
            VehiclePosition = row["VehiclePosition"].ToString();
            Barcode = row["Barcoden"].ToString();
            AutoAdd = InputHelper.GetBoolean(row["AutoAdd"].ToString());
            DateEntered = InputHelper.GetDateTime(row["Date_Entered"].ToString());
            BettermentType = row["BettermentType"].ToString();
            BettermentValue = InputHelper.GetDouble(row["BettermentValue"].ToString());
        }

        public SaveResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("id", ID));
            parameters.Add(new SqlParameter("EstimationDataID", EstimationDataID));
            parameters.Add(new SqlParameter("@StepID", StepID));
            parameters.Add(new SqlParameter("@PartNumber", PartNumber));
            parameters.Add(new SqlParameter("@PartSource", PartSource));
            parameters.Add(new SqlParameter("@ActionCode", ActionCode));
            parameters.Add(new SqlParameter("@Description", Description));
            parameters.Add(new SqlParameter("@Price", Price));
            parameters.Add(new SqlParameter("@Qty", Quantity));
            parameters.Add(new SqlParameter("@ImageID", ImageID));
            parameters.Add(new SqlParameter("@ActionDescription", ActionDescription));
            parameters.Add(new SqlParameter("@PartOfOverhaul", PartOfOverhaul));
            parameters.Add(new SqlParameter("@PartSourceVendorID", PartSourceVendorID));
            parameters.Add(new SqlParameter("@BettermentParts", BettermentParts));
            parameters.Add(new SqlParameter("@SubletPartsFlag", SubletPartsFlag));
            parameters.Add(new SqlParameter("@BettermentMaterials", BettermentMaterials));
            parameters.Add(new SqlParameter("@SubletOperationFlag", SubletOperationFlag));
            parameters.Add(new SqlParameter("@SupplementVersion", SupplementVersion));
            parameters.Add(new SqlParameter("@LineNumber", LineNumber));
            parameters.Add(new SqlParameter("@UniqueSequenceNumber", UniqueSequenceNumber));
            parameters.Add(new SqlParameter("@ModifiesID", ModifiesID));
            parameters.Add(new SqlParameter("@ACDCode", ACDCode));
            parameters.Add(new SqlParameter("@CustomerPrice", CustomerPrice));
            parameters.Add(new SqlParameter("@AutomaticCharge", AutomaticCharge));
            parameters.Add(new SqlParameter("@SourcePartNumber", SourcePartNumber));
            parameters.Add(new SqlParameter("@SectionID", SectionID));
            parameters.Add(new SqlParameter("@VehiclePosition", VehiclePosition));
            parameters.Add(new SqlParameter("@Barcode", Barcode));
            parameters.Add(new SqlParameter("@Date_Entered", DateEntered));
            parameters.Add(new SqlParameter("@BettermentType", BettermentType));
            parameters.Add(new SqlParameter("@BettermentValue", BettermentValue));
            

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdateEstimationLineItem", parameters);
            if (result.Success)
            {
                ID = result.Value;
            }

            return new SaveResult(result);
        }

        public void Delete()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("DeleteEstimationLineItem", new SqlParameter("id", this.ID));
        }
      
    }
}
