using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class PaymentInfoData : ProEstEntity
    {
        public int PaymentId { get; set; }
	    public int AdminInfoID { get; set; }
	    public int ContactsID { get; set; }
	    public string PayeeName { get; set; }
	    public string PaymentType { get; set; } 
	    public string PaymentDate { get; set; }  
	    public string CheckNumber { get; set; }
	    public decimal Amount { get; set; }
	    public string Memo { get; set; }
        public string WhoPays { get; set; }
        public int CreditCardPaymentID { get; set; }

        public PaymentInfoData()
        {
            PayeeName = "";
	        PaymentType = "";
	        PaymentDate = ""; 
	        CheckNumber = "";
	        Amount = 0;
	        Memo = "";
            WhoPays = "";
        }

        public PaymentInfoData(DataRow row) 
        {
            PaymentId = InputHelper.GetInteger(row["PaymentId"].ToString());
            AdminInfoID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            ContactsID = InputHelper.GetInteger(row["ContactsID"].ToString());
            PayeeName = row["PayeeName"].ToString();
            PaymentType = row["PaymentType"].ToString();
            PaymentDate = row["PaymentDate"].ToString();
            CheckNumber = row["CheckNumber"].ToString();
            Amount = InputHelper.GetDecimal(row["Amount"].ToString());
            Memo = row["Memo"].ToString();
            WhoPays = row["WhoPays"].ToString();
            CreditCardPaymentID = InputHelper.GetInteger(row["CreditCardPaymentID"].ToString());

            RowAsLoaded = row;
        }

        public static PaymentInfoData GetPaymentInfo(int paymentID)
        {

            if (paymentID > 0)
            {
                try
                {
                    DBAccess db = new DBAccess();
                    DBAccessTableResult tableResult = db.ExecuteWithTable("GetPaymentInfoByID", new SqlParameter("PaymentID", paymentID));
                    if (tableResult.Success)
                    {
                        return new PaymentInfoData(tableResult.DataTable.Rows[0]);
                    }
                }
                catch (System.Exception ex) 
                {
                    ErrorLogger.LogError(ex, 0, "PaymentInfoData GetPaymentInfo");
                }
            }            

            return null;
        }

        public static List<PaymentInfoData> GetAllPaymentInfo(int estimateID)
        {
            List<PaymentInfoData> paymentInfoList = new List<PaymentInfoData>();

            try
            {
                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("GetPaymentInfo", new SqlParameter("AdminInfoID", estimateID));

                if (tableResult.Success)
                {
                    foreach(DataRow row in tableResult.DataTable.Rows)
                    {
                        paymentInfoList.Add(new PaymentInfoData(row));
                    }
                }
            }
            catch (System.Exception ex) 
            {
                ErrorLogger.LogError(ex, 0, "PaymentInfoData GetAllPaymentInfo");
            }

            return paymentInfoList;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("PaymentId", PaymentId));
                parameters.Add(new SqlParameter("AdminInfoID", AdminInfoID));
                parameters.Add(new SqlParameter("ContactsID", ContactsID));
                parameters.Add(new SqlParameter("PayeeName", GetString(PayeeName)));
                parameters.Add(new SqlParameter("PaymentType", GetString(PaymentType)));
                parameters.Add(new SqlParameter("PaymentDate", GetString(PaymentDate)));
                parameters.Add(new SqlParameter("CheckNumber", GetString(CheckNumber)));
                parameters.Add(new SqlParameter("Amount", Amount));
                parameters.Add(new SqlParameter("Memo", GetString(Memo)));
                parameters.Add(new SqlParameter("WhoPays", GetString(WhoPays)));
                parameters.Add(new SqlParameter("CreditCardPaymentID", CreditCardPaymentID));

                DBAccess db = new DBAccess();
                DBAccessIntResult intResult = db.ExecuteWithIntReturn("AddOrUpdatePaymentInfo", parameters);
                if (intResult.Success)
                {
                    PaymentId = intResult.Value;
                    ChangeLogManager.LogChange(activeLoginID, "PaymentInfoData", PaymentId, ChangeLogManager.GetLoginIDFromEstimate(AdminInfoID), parameters, RowAsLoaded);
                }
                return new SaveResult(intResult.ErrorMessage);
            }
            catch (System.Exception ex)
            {
                return new SaveResult("Error saving payment information: " + ex.Message);
            }
        }

        public void Delete()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("DeletePaymentItem", new SqlParameter("PaymentID", PaymentId));
        }
    }
}