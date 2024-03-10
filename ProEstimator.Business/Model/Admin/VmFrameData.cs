using System.Data;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmFrameData : IModelMap<VmFrameData>
    {
        public VmFrameData ToModel(DataRow row)
        {
            var result = new VmFrameData();
            result.ContractId = (int)row["ContractID"];

            return result;
        }

        public int ContractId { get; set; }
        public int NewestContractId { get; set; }
        public string EffectiveDate { get; set; }
        public bool Trial { get; set; }
    }
}
