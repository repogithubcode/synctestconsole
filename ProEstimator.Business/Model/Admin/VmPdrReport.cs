using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmPdrReport : IModelMap<VmPdrReport>
    {
        public int LoginId { get; set; }
        public string CompanyName { get; set; }
        public string SalesRep { get; set; }
        public int NumberOfEstimates { get; set; }
        public string Date { get; set; }
        public VmPdrReport ToModel(DataRow row)
        {
            var model = new VmPdrReport();
            model.LoginId = (int)row["LoginId"];
            model.CompanyName = row["CompanyName"].SafeString();
            model.SalesRep = row["SalesRep"].SafeString();
            model.NumberOfEstimates = (int)row["NumberOfEstimates"];
            model.Date = row["Date"].SafeDate();

            return model;
        }
    }
}
