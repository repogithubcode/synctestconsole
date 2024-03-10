using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmCustomerSearch : IModelMap<VmCustomerSearch>
    {
        public int? LoginId { get; set; }
        public string LoginName { get; set; }
        public string Company { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public VmCustomerSearch ToModel(DataRow row)
        {
            var model = new VmCustomerSearch();
            model.LoginId = (int?)row["LoginId"];
            model.LoginName = row["LoginName"].SafeString();
            model.Company = row["CompanyName"].SafeString();
            model.FirstName = row["FirstName"].SafeString();
            model.LastName = row["LastName"].SafeString();

            return model;
        }
    }
}
