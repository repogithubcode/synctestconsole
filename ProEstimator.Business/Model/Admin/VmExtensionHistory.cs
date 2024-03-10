using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmExtensionHistory : IModelMap<VmExtensionHistory>
    {
        public int Loginid { get; set; }	
        public string Extendfrom { get;set; }	
        public string Extendto { get; set; }	
        public string SalesRep { get; set; }	
        public string Extendeddate { get; set; }
        public VmExtensionHistory ToModel(DataRow row)
        {
            var model = new VmExtensionHistory();
            model.Loginid = (int)row["loginid"];
            model.Extendfrom = row["extendfrom"].SafeDate();
            model.Extendto = row["extendto"].SafeDate();
            model.SalesRep = row["SalesRep"].SafeString();
            model.Extendeddate = row["extendeddate"].SafeDate();

            return model;
        }
    }
}
