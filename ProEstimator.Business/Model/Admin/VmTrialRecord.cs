using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;

namespace ProEstimator.Business.Model.Admin
{
    public class VmTrialRecord : IModelMap<VmTrialRecord>
    {
        public int Id { get; set; }
        public int SalesRepID { get; set; }
        public string CompanyName { get; set; }
        public string Name { get; set; }
        public int ContractID { get; set; }
        public string TrialStartDate { get; set; }
        public string TrialEndDate { get; set; }
        public int EstimateCount { get; set; }
        public string ContractType { get; set; }
        public bool Trial { get; set; }
        public bool HasConverted { get; set; }

        public VmTrialRecord ToModel(System.Data.DataRow row)
        {
            var model = new VmTrialRecord();
            model.Id = (int)row["Id"];
            model.SalesRepID = (int)row["SalesRepID"];
            model.CompanyName = row["CompanyName"].SafeString();
            model.Name = row["Name"].SafeString();
            model.ContractID = (int)row["ContractID"];
            model.TrialStartDate = row["TrialStartDate"].SafeDate();
            model.TrialEndDate = row["TrialEndDate"].SafeDate();
            model.EstimateCount = (int)row["EstimateCount"];
            model.ContractType = row["ContractType"].SafeString();
            model.Trial = (int)row["Trial"] == 1 ? true : false;

            return model;
        }
    }
}
