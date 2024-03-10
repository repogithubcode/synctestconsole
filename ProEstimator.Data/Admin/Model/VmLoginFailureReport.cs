using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProEstimatorData;

namespace ProEstimatorData.Admin.Model
{
   public class VmLoginFailureReport 
   {
       [JsonProperty("id")]
       public int ID { get; set; }
       [JsonProperty("LoginId")]
       public int LoginId { get; set; }
       [JsonProperty("loginName")]
        public string LoginName { get; set; }
       [JsonProperty("organization")]
       public string Organization { get; set; }
       [JsonProperty("password")]
        public string Password { get; set; }
       [JsonProperty("timeDate")]
        public string TimeDate { get; set; }
       [JsonProperty("userAddress")]
        public string UserAddress { get; set; }
       [JsonProperty("reason")]
        public string Reason { get; set; }
       [JsonProperty("salesRep")]
        public string SalesRep { get; set; }

        public VmLoginFailureReport(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginId = InputHelper.GetInteger(row["LoginId"].ToString());
            LoginName = InputHelper.GetString(row["LoginName"].ToString());
            Organization = InputHelper.GetString(row["Organization"].ToString());
            Password = InputHelper.GetString(row["Password"].ToString());
            TimeDate = InputHelper.GetString(row["TimeDate"].ToString());
            UserAddress = InputHelper.GetString(row["User_Addr"].ToString());
            Reason = InputHelper.GetString(row["Reason"].ToString());
            SalesRep = InputHelper.GetString(row["SalesRep"].ToString());
        }
    }
}
