using System;
using System.Data;

namespace ProEstimatorData.DataModel.Financing
{
    // Data returned from stored proc "Wisetack_MerchantInfo_Get" for a given LoginID
    public class WisetackMerchantInfo
    {
        public string MerchantID { get; set; }
        public string SignupLink { get; set; }
        public string Status { get; set; }
        public DateTime CallbackDate { get; set; }
        public string EventType { get; set; }
        public string Reasons { get; set; }
        public string ExternalID { get; set; }

        public WisetackMerchantInfo(DataRow row)
        {
            MerchantID = InputHelper.GetString(row["MerchantID"].ToString());
            SignupLink = InputHelper.GetString(row["SignupLink"].ToString());
            Status = InputHelper.GetString(row["Status"].ToString());
            CallbackDate = InputHelper.GetDateTime(row["CallbackDate"].ToString());
            EventType = InputHelper.GetString(row["EventType"].ToString());
            Reasons = InputHelper.GetString(row["Reasons"].ToString());
            ExternalID = InputHelper.GetString(row["ExternalID"].ToString());
        }
    }
}
