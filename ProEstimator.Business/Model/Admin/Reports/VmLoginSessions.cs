using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;
using Newtonsoft.Json;

namespace ProEstimator.Business.Model.Admin
{
    public class VmLoginSessions : IModelMap<VmLoginSessions>
    {
        [JsonProperty("loginName")]
        public string LoginName { get; set; }
        [JsonProperty("organization")]
        public string Organization { get; set; }
        [JsonProperty("sessionStarted")]
        public string  SessionStarted { get; set; }
        [JsonProperty("lastActivity")]
        public string  LastActivity { get; set; }
        [JsonProperty("estimateId")]
        public int? EstimateId { get; set; }
        [JsonProperty("salesRep")]
        public string SalesRep { get; set; }

        public VmLoginSessions ToModel(DataRow row)
        {
            var model = new VmLoginSessions();
            model.LoginName = row["LoginName"].SafeString();
            model.Organization = row["Login Org"].SafeString();
            model.SessionStarted =row["Started"].SafeDate();
            model.LastActivity = row["Last Active"].SafeDate();
            model.EstimateId = row["Est. ID"].SafeInt();
            model.SalesRep = row["Sales Rep"].SafeString();
            return model;
        }
    }
}
