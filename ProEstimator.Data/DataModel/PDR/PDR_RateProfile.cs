using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.PDR
{
    public class PDR_RateProfile : ProEstEntity 
    {

        public int ID { get; private set; }
        public string ProfileName { get; set; }
        public int LoginID { get; set; }
        public int AdminInfoID { get; set; }
        public bool IsDefault { get; set; }
        public PDR_RateProfileType ProfileType { get; set; }
        public bool Deleted	{ get; set; }
        public PDR_GridType GridType { get; set; }
        public bool HideDentCounts { get; set; }
        public bool TieredOversizedDents { get; set; }
        public int OriginalID { get; set; }
        public bool Taxable { get; set; }

        private List<PDR_Rate> _rates = null;
        public DateTime? CreationStamp { get; private set; }
        private void CacheRatesList()
        {
            if (_rates == null)
            {
                _rates = PDR_Rate.GetByProfile(ID);
            }
        }

        public PDR_RateProfile()
        {
            ID = 0;
            ProfileName = "";
            LoginID = 0;
            AdminInfoID = 0;
            IsDefault = false;
            ProfileType = PDR_RateProfileType.Retail;
            Deleted = false;
            GridType = PDR_GridType.US;
            HideDentCounts = false;
            TieredOversizedDents = false;
            OriginalID = 0;
            Taxable = false;
        }

        public static PDR_RateProfile GetByID(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("PDR_RateProfile_Get", new SqlParameter("RateProfileID", id));
            if (result.Success)
            {
                PDR_RateProfile rateProfile = new PDR_RateProfile();
                rateProfile.LoadData(result.DataTable.Rows[0]);
                return rateProfile;
            }

            return null;
        }

        public static List<PDR_RateProfile> GetByLogin(int loginID, bool deleted = false)
        {
            List<PDR_RateProfile> results = new List<PDR_RateProfile>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("Deleted", deleted));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("PDR_RateProfile_Get", parameters);
            if (result.Success)
            {
                foreach(DataRow row in result.DataTable.Rows)
                {
                    PDR_RateProfile rateProfile = new PDR_RateProfile();
                    rateProfile.LoadData(row);
                    results.Add(rateProfile);
                }
            }

            return results;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("ProfileName", GetString(ProfileName)));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("AdminInfoID", AdminInfoID));
            parameters.Add(new SqlParameter("IsDefault", IsDefault));
            parameters.Add(new SqlParameter("RateTypeID", (int)ProfileType));
            parameters.Add(new SqlParameter("Deleted", Deleted));
            parameters.Add(new SqlParameter("GridType", (int)GridType));
            parameters.Add(new SqlParameter("HideDentCounts", HideDentCounts));
            parameters.Add(new SqlParameter("TieredOversizedDents", TieredOversizedDents));
            parameters.Add(new SqlParameter("OriginalID", OriginalID));
            parameters.Add(new SqlParameter("Taxable", Taxable));
            if (ID == 0)
            {
                parameters.Add(new SqlParameter("CreationStamp", DateTime.Now));
            }
            else
            {
                parameters.Add(new SqlParameter("CreationStamp", CreationStamp));
            }

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdatePDRRateProfile", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "PDRRateProfile", ID, LoginID, parameters, RowAsLoaded, GetString(ProfileName));
            }

            return new SaveResult(result);
        }

        public SaveResult Delete(int activeLoginID)
        {
            Deleted = true;
            return Save(activeLoginID);
        }

        public void LoadData(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            ProfileName = row["ProfileName"].ToString();
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            AdminInfoID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            IsDefault = InputHelper.GetBoolean(row["IsDefault"].ToString());
            ProfileType = (PDR_RateProfileType)Enum.Parse(typeof(PDR_RateProfileType), row["RateTypeID"].ToString());
            Deleted = InputHelper.GetBoolean(row["Deleted"].ToString());
            GridType = (PDR_GridType)Enum.Parse(typeof(PDR_GridType), row["GridType"].ToString());
            HideDentCounts = InputHelper.GetBoolean(row["HideDentCounts"].ToString());
            TieredOversizedDents = InputHelper.GetBoolean(row["TieredOversizedDents"].ToString());
            OriginalID = InputHelper.GetInteger(row["OriginalID"].ToString());
            Taxable = InputHelper.GetBoolean(row["Taxable"].ToString());
            CreationStamp = InputHelper.GetDateTimeNullable(row["CreationStamp"].ToString());

            RowAsLoaded = row;
        }

        public List<PDR_Rate> GetAllRates()
        {
            CacheRatesList();
            return _rates;
        }

        public List<PDR_Rate> GetRatesBySize(PDR_Size size)
        {
            CacheRatesList();
            return _rates.Where(o => o.Size == size).ToList();
        }

        public List<ChangeLogRowSummary> GetHistory()
        {
            List<ChangeLogRowSummary> history = ChangeLogRowSummary.GetByItem("PDRRateProfile", ID);
            history.AddRange(ChangeLogRowSummary.GetByItem("PDRRate", ID));
            history.AddRange(ChangeLogRowSummary.GetByItem("PDRMultiplier", ID));

            return history.OrderBy(o => o.TimeStamp).ToList();
        }

    }
}
