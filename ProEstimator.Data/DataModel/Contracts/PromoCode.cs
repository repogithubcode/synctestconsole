using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Contracts
{
    public class PromoCode
    {

        public int ID { get; private set; }
        public int ContractPriceLevelID	{ get; set; }
        public string Code { get; set; }
        public decimal PromoAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate	{ get; set; }
        public DateTime DateCreated { get; set; }

        public static PromoCode Search(int contractPriceLevelID, string promoCode, bool ignoreDateRange = false)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("contractPriceLevelID", contractPriceLevelID));
            parameters.Add(new SqlParameter("promoCode", promoCode));
            parameters.Add(new SqlParameter("filterDate", !ignoreDateRange));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("CheckPromoCode", parameters);

            if (result.Success)
            {
                return new PromoCode(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static PromoCode GetByID(int promoID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetPromoCode", new SqlParameter("promoID", promoID));

            if (result.Success)
            {
                return new PromoCode(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<PromoCode> GetAvailableForContract(int contractID)
        {
            List<PromoCode> promoCodes = new List<PromoCode>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("PromoCode_GetAvailableForContract", new SqlParameter("ContractID", contractID));

            if (result.Success)
            {
                foreach(DataRow row in result.DataTable.Rows)
                {
                    promoCodes.Add(new PromoCode(row));
                }
            }

            return promoCodes;
        }

        public PromoCode(DataRow row)
        {
            ID = InputHelper.GetInteger(row["PromoID"].ToString());
            ContractPriceLevelID = InputHelper.GetInteger(row["ContractPriceLevelID"].ToString());
            Code = row["PromoCode"].ToString();
            PromoAmount = InputHelper.GetDecimal(row["PromoAmount"].ToString());
            StartDate = InputHelper.GetDateTime(row["StartDate"].ToString());
            EndDate = InputHelper.GetDateTime(row["EndDate"].ToString());
            DateCreated = InputHelper.GetDateTime(row["DateCreated"].ToString());
        }

    }
}
