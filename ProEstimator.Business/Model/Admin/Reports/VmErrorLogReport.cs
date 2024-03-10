using Newtonsoft.Json;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Model.Admin.Reports
{
   
    public class VmErrorLogReport : IModelMap<VmErrorLogReport>
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("loginID")]
        public int LoginID { get; set; }
        [JsonProperty("adminInfoID")]
        public int AdminInfoID { get; set; }
        [JsonProperty("errorText")]
        public string ErrorText { get; set; }
        [JsonProperty("timeOccurred")]
        public string TimeOccurred { get; set; }
        [JsonProperty("sessionVars")]
        public string SessionVars { get; set; }
        [JsonProperty("errorFixed")]
        public bool ErrorFixed { get; set; }
        [JsonProperty("fixNote")]
        public string FixNote { get; set; }
        [JsonProperty("app")]
        public string App { get; set; }


        public VmErrorLogReport ToModel(DataRow row)
        {
            var model = new VmErrorLogReport()
            {
                Id = row["id"].SafeInt().GetValueOrDefault(),
                LoginID = row["LoginID"].SafeInt().GetValueOrDefault(),
                AdminInfoID = row["AdminInfoID"].SafeInt().GetValueOrDefault(),
                ErrorText = row["ErrorText"].SafeString(),
                ErrorFixed = row["ErrorFixed"].SafeBool().GetValueOrDefault(),
                SessionVars = row["SessionVars"].SafeString(),
                App = row["App"].SafeString(),
                TimeOccurred = row["TimeOccurred"].SafeDate()
            };
            return model;
        }
    }

    public class ErrorLogReportRequest
    {
        public Nullable<int> LoginID { get; set; }
        public Nullable<int> AdminInfoID { get; set; }
        public string ErrorTag { get; set; }
        public Nullable<DateTime> RangeStart { get; set; }
        public Nullable<DateTime> RangeEnd { get; set; }
    }
}
