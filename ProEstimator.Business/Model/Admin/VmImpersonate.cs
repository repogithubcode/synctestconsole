using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmImpersonate : IModelMap<VmImpersonate>
    {
        public string LoginId { get; set; }
        public string LoginName { get; set; }
        public string Organization { get; set; }
        public string Password { get; set; }
        public string Server { get;set; }

        public VmImpersonate ToModel(DataRow row)
        {
            var model = new VmImpersonate();
            model.LoginId = row[0].SafeString();
            model.LoginName = row[1].SafeString();
            model.Organization = row[2].SafeString();
            model.Password = row[3].SafeString();

            return model;
        }
    }
}
