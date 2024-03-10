using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;
using Newtonsoft.Json;


namespace ProEstimator.Business.Model.Admin
{
   public class VmLoginFailureReport : IModelMap<VmLoginFailureReport>
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

        public VmLoginFailureReport ToModel(DataRow row)
        {
            var model = new VmLoginFailureReport();
            model.ID = (int)row["ID"];
            model.LoginId = (int)row["LoginId"];
            model.LoginName = row["LoginName"].SafeString();
            model.Organization = row["Organization"].SafeString();
            model.Password = row["Password"].SafeString();
            model.TimeDate = row["TimeDate"].SafeDate();
            model.UserAddress = row["User_Addr"].SafeString();
            model.Reason = row["Reason"].SafeString();
            model.SalesRep = row["SalesRep"].SafeString();
            return model;
        }
    }
}
