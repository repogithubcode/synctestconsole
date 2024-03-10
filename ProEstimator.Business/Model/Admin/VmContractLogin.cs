using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmContractLogin : IModelMap<VmContractLogin>
    {
        public VmContractLogin ToModel(DataRow row)
        {
            var model = new VmContractLogin();
            model.Id = row["LoginId"].SafeInt();
            model.LoginName = row["LoginName"].SafeString();
            model.FullName = row["FullName"].SafeString();
            model.PrimaryLogin = (bool)row["PrimaryLogin"];
            model.ContractLoginID = row["ContractLoginID"].SafeInt();


            return model;
        }

        public bool PrimaryLogin { get; set; }

        public string FullName { get; set; }

        public string LoginName { get; set; }

        public int? ContractLoginID { get; set; }
        public int? Id { get; set; }
    }
}
