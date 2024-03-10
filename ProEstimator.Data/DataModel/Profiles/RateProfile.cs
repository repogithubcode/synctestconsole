using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.Profiles
{
    public class RateProfile : ProEstEntity
    {
        public int ID { get; private set; }
        public int LoginID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; private set; }
        public bool IsDefaultPresets { get; set; }
        public DateTime CreationStamp { get; private set; }
        public int OriginalID { get; set; }
        public int EstimateID { get; set; }
        public bool IsDeleted { get; set; }
        public bool CapRatesAfterInclude { get; set; }
        public bool CapSuppliesAfterInclude { get; set; }
        public bool IsFullEstimateDiscountMarkup { get; set; }
        public double FullEstimateDiscountMarkupValue { get; set; }
        public decimal CreditCardFeePercentage { get; set; }
        public bool ApplyCreditCardFee { get; set; }
        public bool TaxedCreditCardFee { get; set; }

        public RateProfile(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            LoginID = InputHelper.GetInteger(row["OwnerID"].ToString());
            Name = row["ProfileName"].ToString();
            Description = row["Description"].ToString();
            IsDefault = InputHelper.GetBoolean(row["DefaultFlag"].ToString());
            IsDefaultPresets = InputHelper.GetBoolean(row["DefaultPreset"].ToString());
            CreationStamp = InputHelper.GetDateTime(row["CreationDate"].ToString());
            OriginalID = InputHelper.GetInteger(row["OriginalID"].ToString());
            EstimateID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["Deleted"].ToString());
            CapRatesAfterInclude = InputHelper.GetBoolean(row["CapRatesAfterInclude"].ToString());
            CapSuppliesAfterInclude = InputHelper.GetBoolean(row["CapSuppliesAfterInclude"].ToString());
            IsFullEstimateDiscountMarkup = InputHelper.GetBoolean(row["IsFullEstimateDiscountMarkup"].ToString());
            FullEstimateDiscountMarkupValue = InputHelper.GetDouble(row["FullEstimateDiscountMarkupValue"].ToString());
            CreditCardFeePercentage = InputHelper.GetDecimal(row["CreditCardFeePercentage"].ToString());
            ApplyCreditCardFee = InputHelper.GetBoolean(row["ApplyCreditCardFee"].ToString());
            TaxedCreditCardFee = InputHelper.GetBoolean(row["TaxedCreditCardFee"].ToString());

            RowAsLoaded = row;
        }

        public static List<RateProfile> GetAllForLogin(int loginID, bool deleted = false)
        {
            List<RateProfile> profiles = new List<RateProfile>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("Deleted", deleted));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomerProfile_GetForLogin", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    profiles.Add(new RateProfile(row));
                }
            }

            return profiles;
        }

        /// <summary>
        /// Override to omit imported estimate level rate profiles
        /// </summary>
        /// <param name="loginID"></param>
        /// <param name="imported"></param>
        /// <param name="deleted"></param>
        /// <returns></returns>
        public static List<RateProfile> GetAllForLogin(int loginID, bool omitImported, bool deleted = false)
        {
            List<RateProfile> profiles = new List<RateProfile>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("Deleted", deleted));
            parameters.Add(new SqlParameter("OmitImported", omitImported));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomerProfile_GetForLogin", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    profiles.Add(new RateProfile(row));
                }
            }

            return profiles;
        }

        public static RateProfile Get(int profileID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomerProfile_Get", new SqlParameter("ID", profileID));

            if (tableResult.Success)
            {
                return new RateProfile(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("Name", GetString(Name)));
            parameters.Add(new SqlParameter("Description", GetString(Description)));
            parameters.Add(new SqlParameter("DefaultFlag", IsDefault));
            parameters.Add(new SqlParameter("OriginalID", OriginalID));
            parameters.Add(new SqlParameter("EstimateID", EstimateID));
            parameters.Add(new SqlParameter("DefaultPreset", IsDefaultPresets));
            parameters.Add(new SqlParameter("Deleted", IsDeleted));
            parameters.Add(new SqlParameter("CapRatesAfterInclude", CapRatesAfterInclude));
            parameters.Add(new SqlParameter("CapSuppliesAfterInclude", CapSuppliesAfterInclude));
            parameters.Add(new SqlParameter("IsFullEstimateDiscountMarkup", IsFullEstimateDiscountMarkup));
            parameters.Add(new SqlParameter("FullEstimateDiscountMarkupValue", FullEstimateDiscountMarkupValue));
            parameters.Add(new SqlParameter("CreditCardFeePercentage", CreditCardFeePercentage));
            parameters.Add(new SqlParameter("ApplyCreditCardFee", ApplyCreditCardFee));
            parameters.Add(new SqlParameter("TaxedCreditCardFee", TaxedCreditCardFee));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("CustomerProfile_AddOrUpdate", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "RateProfile", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public void SetAsDefaultProfile()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("CustomerProfile_SetAsDefaultProfile", new System.Data.SqlClient.SqlParameter("profileID", ID));
        }

        public void SetAsDefaultPresets()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("CustomerProfile_SetAsDefaultPreset", new System.Data.SqlClient.SqlParameter("profileID", ID));
        }

        public static int GetOriginalProfileID(int estimateID)
        {
            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("CustomerProfile_GetOriginalID", new SqlParameter("AdminInfoID", estimateID));
            if (result.Success)
            {
                return result.Value;
            }

            return 0;
        }

        public List<ChangeLogRowSummary> GetHistory()
        {
            List<ChangeLogRowSummary> history = ChangeLogRowSummary.GetByItem("RateProfile", ID);
            history.AddRange(ChangeLogRowSummary.GetByItem("Rate", ID));

            PaintSettings paintRates = PaintSettings.GetForProfile(ID);
            if (paintRates != null)
            {
                history.AddRange(ChangeLogRowSummary.GetByItem("PaintSettings", paintRates.ID));
            }

            PrintSettings printSettings = PrintSettings.GetForProfile(ID);
            if (printSettings != null)
            {
                history.AddRange(ChangeLogRowSummary.GetByItem("PrintSettings", printSettings.ID));
            }

            MiscSettings miscSettings = MiscSettings.GetForProfile(ID);
            if(miscSettings != null)
            {
                history.AddRange(ChangeLogRowSummary.GetByItem("MiscSettings", miscSettings.ID));
            }

            history.AddRange(ChangeLogRowSummary.GetByItem("PresetSettings", ID));

            return history.OrderBy(o => o.TimeStamp).ToList();
        }
    }
}
