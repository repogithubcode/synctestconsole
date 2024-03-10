using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class StripeInfo : ProEstEntity 
    {

        public int ID { get; set; }
        public int LoginID { get; set; }
        public string StripeCustomerID { get; set; }
        public string StripeCardID { get; set; }
        public string CardLast4	{ get; set; }
        public DateTime CardExpiration { get; set; }
        public string PlaidAccessToken { get; set; }
        public string PlaidItemID { get; set; }
        public string StripeBankAccountToken { get; set; }
        public bool DeleteFlag { get; set; }
        public bool CardError { get; set; }
        public string ErrorMessage { get; set; }
        public bool AutoPay { get; private set; }

        public StripeInfo()
        {
            ID = 0;
            LoginID = 0;
            StripeCustomerID = "";
            StripeCardID = "";
            CardLast4 = "";
            CardExpiration = new DateTime(2017, 1, 1);
            DeleteFlag = false;
            CardError = false;
            ErrorMessage = "";
            PlaidAccessToken = "";
            PlaidItemID = "";
            StripeBankAccountToken = "";
            AutoPay = false;
        }

        /// <summary>
        /// Set the Auto Pay value.  The only code that should call this are the TurnOn and TurnOff commands.  If you need to set this value, use one of those commands.
        /// </summary>
        public void SetAutoPay(bool value)
        {
            AutoPay = value;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("id", ID));
            parameters.Add(new SqlParameter("loginID", LoginID));
            parameters.Add(new SqlParameter("stripeCustomerID", GetString(StripeCustomerID)));
            parameters.Add(new SqlParameter("stripeCardID", GetString(StripeCardID)));
            parameters.Add(new SqlParameter("cardLast4", GetString(CardLast4)));
            parameters.Add(new SqlParameter("cardExpiration", CardExpiration));
            parameters.Add(new SqlParameter("deleteFlag", DeleteFlag));
            parameters.Add(new SqlParameter("cardError", CardError));
            parameters.Add(new SqlParameter("errorMessage", GetString(ErrorMessage)));
            parameters.Add(new SqlParameter("plaidAccessToken", GetString(PlaidAccessToken)));
            parameters.Add(new SqlParameter("plaidItemID", GetString(PlaidItemID)));
            parameters.Add(new SqlParameter("stripeBankAccountToken", GetString(StripeBankAccountToken)));
            parameters.Add(new SqlParameter("autoPay", AutoPay));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("StripeInfoAddOrUpdate", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "StripeInfo", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        #region Static Get Functions

        /// <summary>
        /// Get a Stripe Info record by database ID
        /// </summary>
        public static StripeInfo GetStripeInfo(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("StripeInfoGet", new SqlParameter("id", id));

            if (result.Success && result.DataTable.Rows.Count > 0)
            {
                StripeInfo stripeInfo = new StripeInfo();
                stripeInfo.LoadData(result.DataTable.Rows[0]);
                return stripeInfo;
            }

            return null;
        }

        /// <summary>
        /// Get a stripe info record for the passed login.
        /// </summary>
        public static StripeInfo GetForLogin(int loginID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("StripeInfoGetByLogin", new SqlParameter("loginID", loginID));

            if (result.Success && result.DataTable.Rows.Count > 0)
            {
                StripeInfo stripeInfo = new StripeInfo();
                stripeInfo.LoadData(result.DataTable.Rows[0]);
                return stripeInfo;
            }

            return null;
        }

        /// <summary>
        /// Get a stripe info record for the passed stripe customer ID
        /// </summary>
        public static StripeInfo GetForStripeCustomerID(string customerID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("StripeInfoGetByStripeID", new SqlParameter("stripeID", customerID));

            if (result.Success && result.DataTable.Rows.Count > 0)
            {
                StripeInfo stripeInfo = new StripeInfo();
                stripeInfo.LoadData(result.DataTable.Rows[0]);
                return stripeInfo;
            }

            return null;
        }

        /// <summary>
        /// For a long time we created a new Stripe Customer ID for every card we saved, which was a mistake.  Now we do one per account.  Before adding one for an account, 
        /// use this to get the last Stripe Customer ID assigned to a card for this customer.
        /// </summary>
        public static string GetLastStripeCustomerIDForLogin(int loginID)
        {
            DBAccess db = new DBAccess();
            var result = db.ExecuteWithStringReturn("StripeInfoGetMostRecentCustomerIDForLogin", new SqlParameter("loginID", loginID));

            if (result.Success)
            {
                return result.Value;
            }

            return "";
        }

        private void LoadData(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            StripeCustomerID = row["StripeCustomerID"].ToString();
            StripeCardID = row["StripeCardID"].ToString();
            CardLast4 = row["CardLast4"].ToString();
            CardExpiration = InputHelper.GetDateTime(row["CardExpiration"].ToString());
            DeleteFlag = InputHelper.GetBoolean(row["DeleteFlag"].ToString());
            CardError = InputHelper.GetBoolean(row["CardError"].ToString());
            ErrorMessage = row["ErrorMessage"].ToString();
            PlaidAccessToken = row["PlaidAccessToken"].ToString();
            PlaidItemID = row["PlaidItemID"].ToString();
            StripeBankAccountToken = row["StripeBankAccountToken"].ToString();
            AutoPay = InputHelper.GetBoolean(row["AutoPay"].ToString());

            RowAsLoaded = row;
        }

        #endregion               

    }
}