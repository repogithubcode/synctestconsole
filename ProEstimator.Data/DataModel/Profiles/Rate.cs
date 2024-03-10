using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace ProEstimatorData.DataModel.Profiles
{
    public class Rate : ProEstEntity
    {
        public int ID { get; private set; }
        public int ProfileID { get; private set; }
        public RateType RateType { get; private set; }
        public double RateAmount { get; set; }
        public CapType	CapType {get; set; }
        public double Cap { get; set; }
        public bool Taxable { get; set; }
        public double DiscountMarkup { get; set; }
        public RateType IncludeIn
        {
            get 
            { 
                if (_includeIn == 0)
                {
                    return null;
                }
                else
                {
                    return RateType.GetByID(_includeIn); 
                }                
            }
            set
            {
                if (value == null)
                {
                    _includeIn = 0;
                }
                else
                {
                    _includeIn = value.ID;
                }
            }
        }
        private int _includeIn = 0;

        public Rate(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            ProfileID = InputHelper.GetInteger(row["CustomerProfilesID"].ToString());
            RateType = RateType.GetByID(InputHelper.GetInteger(row["RateType"].ToString()));
            RateAmount = InputHelper.GetDouble(row["Rate"].ToString());
            Cap = InputHelper.GetDouble(row["Cap"].ToString());
            Taxable = InputHelper.GetBoolean(row["Taxable"].ToString());
            DiscountMarkup = InputHelper.GetDouble(row["DiscountMarkup"].ToString());
            IncludeIn = RateType.GetByID(InputHelper.GetInteger(row["IncludeIn"].ToString()));

            int capType = InputHelper.GetInteger(row["CapType"].ToString(), 99);
            if (capType == 2)
            {
                capType = 99;
            }
            CapType = (Profiles.CapType)capType;

            RowAsLoaded = row;
        }

        public SaveResult Save(int activeLoginID, int loginID = 0, string name = "")
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("CustomerProfilesID", ProfileID));
            parameters.Add(new SqlParameter("RateType", RateType != null ? RateType.ID : 0));
            parameters.Add(new SqlParameter("Rate", RateAmount));
            parameters.Add(new SqlParameter("CapType", (int)CapType));
            parameters.Add(new SqlParameter("Cap", Cap));
            parameters.Add(new SqlParameter("Taxable", Taxable));
            parameters.Add(new SqlParameter("DiscountMarkup", DiscountMarkup));
            parameters.Add(new SqlParameter("IncludeIn", IncludeIn != null ? IncludeIn.ID : 0));

            DBAccess db = new DBAccess();
            FunctionResult result = db.ExecuteNonQuery("CustomerProfileRates_Update", parameters);

            if (result.Success)
            {
                ChangeLogManager.LogChange(activeLoginID, "Rate", ID, loginID, parameters, RowAsLoaded, name);
            }
      
            return new SaveResult(result);
        }

        public static List<Rate> GetForProfile(int profileID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomerProfileRates_GetForProfile", new SqlParameter("ProfileID", profileID));

            List<Rate> rates = new List<Rate>();

            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    rates.Add(new Rate(row));
                }
            }

            return rates;
        }
    }
}
