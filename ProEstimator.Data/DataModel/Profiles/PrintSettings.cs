using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Profiles
{
    public class PrintSettings : ProEstEntity
    {

        public int ID { get; private set; }
        public int CustomerProfilesID { get; set; }
        public int GraphicsQuality { get; set; }
        public bool NoHeaderLogo { get; set; }
        public bool NoInsuranceSection { get; set; }
        public bool NoPhotos { get; set; }
        public string FooterText { get; set; }
        public bool PrintPrivateNotes { get; set; }
        public bool PrintPublicNotes { get; set; }
        public string ContactOption	{ get; set; }
        public string SupplementOption { get; set; }
        public string OrderBy { get; set; }
        public bool UseBigLetters { get; set; }
        public bool SeparateLabor { get; set; }
        public bool EstimateNumber { get; set; }
        public bool AdminInfoID	{ get; set; }
        public bool Dollars	{ get; set; }
        public bool GreyBars { get; set; }
        public string NoVehicleAccessories { get; set; }
        public bool PrintPaymentInfo { get; set; }
        public bool PrintEstimator { get; set; }
        public bool PrintVendors { get; set; }
        public bool NoFooterDateTimeStamp { get; set; }

        public bool PrintTechnicians { get; set; }
        public PrintSettings() { }
	
        public PrintSettings(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            CustomerProfilesID = InputHelper.GetInteger(row["CustomerProfilesID"].ToString());
            GraphicsQuality = InputHelper.GetInteger(row["GraphicsQuality"].ToString());
            NoHeaderLogo = InputHelper.GetBoolean(row["NoHeaderLogo"].ToString());
            NoInsuranceSection = InputHelper.GetBoolean(row["NoInsuranceSection"].ToString());
            NoPhotos = InputHelper.GetBoolean(row["NoPhotos"].ToString());
            FooterText = InputHelper.GetString(row["FooterText"].ToString());
            PrintPrivateNotes = InputHelper.GetBoolean(row["PrintPrivateNotes"].ToString());
            PrintPublicNotes = InputHelper.GetBoolean(row["PrintPublicNotes"].ToString());
            ContactOption = InputHelper.GetString(row["ContactOption"].ToString());
            SupplementOption = InputHelper.GetString(row["SupplementOption"].ToString());
            OrderBy = InputHelper.GetString(row["OrderBy"].ToString());
            UseBigLetters = InputHelper.GetBoolean(row["UseBigLetters"].ToString());
            SeparateLabor = InputHelper.GetBoolean(row["SeparateLabor"].ToString());
            EstimateNumber = InputHelper.GetBoolean(row["EstimateNumber"].ToString());
            AdminInfoID = InputHelper.GetBoolean(row["AdminInfoID"].ToString());
            Dollars = InputHelper.GetBoolean(row["Dollars"].ToString());
            GreyBars = InputHelper.GetBoolean(row["GreyBars"].ToString());
            NoVehicleAccessories = InputHelper.GetString(row["NoVehicleAccessories"].ToString());
            PrintPaymentInfo = InputHelper.GetBoolean(row["PrintPaymentInfo"].ToString());
            PrintEstimator = InputHelper.GetBoolean(row["PrintEstimator"].ToString());
            PrintVendors = InputHelper.GetBoolean(row["PrintVendors"].ToString());
            NoFooterDateTimeStamp = InputHelper.GetBoolean(row["NoFooterDateTimeStamp"].ToString());
            PrintTechnicians = InputHelper.GetBoolean(row["PrintTechnicians"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("CustomerProfilesID", CustomerProfilesID));
            parameters.Add(new SqlParameter("GraphicsQuality", GraphicsQuality));
            parameters.Add(new SqlParameter("NoHeaderLogo", NoHeaderLogo));
            parameters.Add(new SqlParameter("NoInsuranceSection", NoInsuranceSection));
            parameters.Add(new SqlParameter("NoPhotos", NoPhotos));
            parameters.Add(new SqlParameter("FooterText", FooterText));
            parameters.Add(new SqlParameter("PrintPrivateNotes", PrintPrivateNotes));
            parameters.Add(new SqlParameter("PrintPublicNotes", PrintPublicNotes));
            parameters.Add(new SqlParameter("ContactOption", ContactOption));
            parameters.Add(new SqlParameter("SupplementOption", SupplementOption));
            parameters.Add(new SqlParameter("OrderBy", OrderBy));
            parameters.Add(new SqlParameter("UseBigLetters", UseBigLetters));
            parameters.Add(new SqlParameter("SeparateLabor", SeparateLabor));
            parameters.Add(new SqlParameter("EstimateNumber", EstimateNumber));
            parameters.Add(new SqlParameter("AdminInfoID", AdminInfoID));
            parameters.Add(new SqlParameter("Dollars", Dollars));
            parameters.Add(new SqlParameter("GreyBars", GreyBars));
            parameters.Add(new SqlParameter("NoVehicleAccessories", NoVehicleAccessories));
            parameters.Add(new SqlParameter("PrintPaymentInfo", PrintPaymentInfo));
            parameters.Add(new SqlParameter("PrintEstimator", PrintEstimator));
            parameters.Add(new SqlParameter("PrintVendors", PrintVendors));
            parameters.Add(new SqlParameter("NoFooterDateTimeStamp", NoFooterDateTimeStamp));
            parameters.Add(new SqlParameter("PrintTechnicians", PrintTechnicians));

            DBAccess db = new DBAccess();
            FunctionResult result = db.ExecuteNonQuery("CustomerProfilesPrint_Update", parameters);

            if (result.Success)
            {
                RateProfile profile = RateProfile.Get(CustomerProfilesID);
                ChangeLogManager.LogChange(activeLoginID, "PrintSettings", ID, profile.LoginID, parameters, RowAsLoaded, profile.Name);
            }

            return new SaveResult(result);
        }

        public static PrintSettings GetForProfile(int profileID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomerProfilesPrint_GetByProfile", new SqlParameter("CustomerProfilesID", profileID));

            if (tableResult.Success)
            {
                return new PrintSettings(tableResult.DataTable.Rows[0]);
            }

            return null;
        }
    }
}
