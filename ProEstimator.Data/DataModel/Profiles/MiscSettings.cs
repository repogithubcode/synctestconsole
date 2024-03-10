using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Profiles
{
    public class MiscSettings : ProEstEntity
    {

        public int ID { get; private set; }
        public int CustomerProfilesID { get; set; }
        public double TaxRate { get; set; }
        public double SecondTaxRateStart { get; set; }
        public double SecondTaxRate { get; set; }
        public bool ChargeForRadiatorRefilling { get; set; }
        public decimal RadiatorChargeAmount	{ get; set; }
        public bool ChargeForACRefilling { get; set; }
        public int ACChargeHow { get; set; }
        public decimal ACChargeAmount { get; set; }
        public string LKQText { get; set; }
        public double TotalLossPerc { get; set; }
        public bool IncludeStructureInBody { get; set; }
        public bool ChargeForAimingHeadlights { get; set; }
        public bool ChargeForPowerUnits	{ get; set; }
        public bool ChargeForRefrigRecovery	{ get; set; }
        public bool SuppressAddRelatedPrompt { get; set; }
        public bool ChargeSuppliesOnAllOperations { get; set; }
        public int SuppLevel { get; set; }
        public bool DoNotMarkChanges { get; set; }
        public bool UseSepPartLaborTax { get; set; }
        public double PartTax { get; set; }
        public double LaborTax { get; set; }

        public MiscSettings() { }

        public MiscSettings(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            CustomerProfilesID = InputHelper.GetInteger(row["CustomerProfilesID"].ToString());
            TaxRate = InputHelper.GetDouble(row["TaxRate"].ToString());
            SecondTaxRateStart = InputHelper.GetDouble(row["SecondTaxRateStart"].ToString());
            SecondTaxRate = InputHelper.GetDouble(row["SecondTaxRate"].ToString());
            ChargeForRadiatorRefilling = InputHelper.GetBoolean(row["ChargeForRadiatorRefilling"].ToString());
            RadiatorChargeAmount = InputHelper.GetDecimal(row["RadiatorChargeAmount"].ToString());
            ChargeForACRefilling = InputHelper.GetBoolean(row["ChargeForACRefilling"].ToString());
            ACChargeHow = InputHelper.GetInteger(row["ACChargeHow"].ToString());
            ACChargeAmount = InputHelper.GetDecimal(row["ACChargeAmount"].ToString());
            LKQText = InputHelper.GetString(row["LKQText"].ToString());
            TotalLossPerc = InputHelper.GetDouble(row["TotalLossPerc"].ToString());
            IncludeStructureInBody = InputHelper.GetBoolean(row["IncludeStructureInBody"].ToString());
            ChargeForAimingHeadlights = InputHelper.GetBoolean(row["ChargeForAimingHeadlights"].ToString());
            ChargeForPowerUnits = InputHelper.GetBoolean(row["ChargeForPowerUnits"].ToString());
            ChargeForRefrigRecovery = InputHelper.GetBoolean(row["ChargeForRefrigRecovery"].ToString());
            SuppressAddRelatedPrompt = InputHelper.GetBoolean(row["SuppressAddRelatedPrompt"].ToString());
            ChargeSuppliesOnAllOperations = InputHelper.GetBoolean(row["ChargeSuppliesOnAllOperations"].ToString());
            SuppLevel = InputHelper.GetInteger(row["SuppLevel"].ToString());
            DoNotMarkChanges = InputHelper.GetBoolean(row["DoNotMarkChanges"].ToString());
            UseSepPartLaborTax = InputHelper.GetBoolean(row["UseSepPartLaborTax"].ToString());
            PartTax = InputHelper.GetDouble(row["PartTax"].ToString());
            LaborTax = InputHelper.GetDouble(row["LaborTax"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("CustomerProfilesID", CustomerProfilesID));
            parameters.Add(new SqlParameter("TaxRate", TaxRate));
            parameters.Add(new SqlParameter("SecondTaxRateStart", SecondTaxRateStart));
            parameters.Add(new SqlParameter("SecondTaxRate", SecondTaxRate));
            parameters.Add(new SqlParameter("ChargeForRadiatorRefilling", ChargeForRadiatorRefilling));
            parameters.Add(new SqlParameter("RadiatorChargeAmount", RadiatorChargeAmount));
            parameters.Add(new SqlParameter("ChargeForACRefilling", ChargeForACRefilling));
            parameters.Add(new SqlParameter("ACChargeHow", ACChargeHow));
            parameters.Add(new SqlParameter("ACChargeAmount", ACChargeAmount));
            parameters.Add(new SqlParameter("LKQText", LKQText));
            parameters.Add(new SqlParameter("TotalLossPerc", TotalLossPerc));
            parameters.Add(new SqlParameter("IncludeStructureInBody", IncludeStructureInBody));
            parameters.Add(new SqlParameter("ChargeForAimingHeadlights", ChargeForAimingHeadlights));
            parameters.Add(new SqlParameter("ChargeForPowerUnits", ChargeForPowerUnits));
            parameters.Add(new SqlParameter("ChargeForRefrigRecovery", ChargeForRefrigRecovery));
            parameters.Add(new SqlParameter("SuppressAddRelatedPrompt", SuppressAddRelatedPrompt));
            parameters.Add(new SqlParameter("ChargeSuppliesOnAllOperations", ChargeSuppliesOnAllOperations));
            parameters.Add(new SqlParameter("SuppLevel", SuppLevel));
            parameters.Add(new SqlParameter("DoNotMarkChanges", DoNotMarkChanges));
            parameters.Add(new SqlParameter("UseSepPartLaborTax", UseSepPartLaborTax));
            parameters.Add(new SqlParameter("PartTax", PartTax));
            parameters.Add(new SqlParameter("LaborTax", LaborTax));

            DBAccess db = new DBAccess();
            FunctionResult result = db.ExecuteNonQuery("CustomerProfilesMisc_Update", parameters);

            if(result.Success)
            {
                RateProfile profile = RateProfile.Get(CustomerProfilesID);
                ChangeLogManager.LogChange(activeLoginID, "MiscSettings", ID, profile.LoginID, parameters, RowAsLoaded, profile.Name);
            }

            return new SaveResult(result);
        }

        public static MiscSettings GetForProfile(int profileID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomerProfilesMisc_GetByProfile", new SqlParameter("CustomerProfilesID", profileID));

            if (tableResult.Success)
            {
                return new MiscSettings(tableResult.DataTable.Rows[0]);
            }

            return null;
        }
    }
}
