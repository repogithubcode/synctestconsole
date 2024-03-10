using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmFrameDataContract : IModelMap<VmFrameDataContract>
    {
        public VmFrameDataContract ToModel(DataRow row)
        {
            var result = new VmFrameDataContract();
            result.ContractId = (int) row["ContractID"];
            result.EffectiveDate = row["EffectiveDate"].SafeString();
            result.TrialContract = (bool)row["isTrialContract"];
            result.NewestContract = (bool) row["isNewestContract"];

            return result;
        }

        public bool TrialContract { get; set; }
        public string EffectiveDate { get; set; }
        public int ContractId { get; set; }
        public bool NewestContract { get; set; }
    }
}
