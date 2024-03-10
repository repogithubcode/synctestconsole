using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmOrganizationSearchResult : IModelMap<VmOrganizationSearchResult>
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string CustomerNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityStateZip { get; set; }
        public bool DemoContract { get; set; }
        public VmOrganizationSearchResult ToModel(DataRow row)
        {
            var model = new VmOrganizationSearchResult();
            model.Id = (int) row["id"];
            model.CompanyName = row["CompanyName"].SafeString();
            model.CustomerNumber = row["CustomerNumber"].SafeString();
            model.AddressLine1 = row["AddressLine1"].SafeString();
            model.AddressLine2 = row["AddressLine2"].SafeString();
            model.CityStateZip = row["CityStateZip"].SafeString();
            model.DemoContract = (bool)row["hasActiveWebEstContract"];

            return model;
        }
    }
}
