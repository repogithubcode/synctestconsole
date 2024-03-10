using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmAddOn : IModelMap<VmAddOn>
    {
        public bool? HasFrameDataContract { get; set; }
        public bool? HasEMSContract { get; set; }
        public bool? HasPDRContract { get; set; }
        public bool? HasMultiContract { get; set; }
        public VmAddOn ToModel(DataRow row)
        {
            var model = new VmAddOn();
            model.HasFrameDataContract = row["HasFrameDataContract"].SafeBool();
            model.HasEMSContract = row["HasEMSContract"].SafeBool();
            model.HasPDRContract = row["HasPDRContract"].SafeBool();
            model.HasMultiContract = row["HasMultiContract"].SafeBool();

            return model;
        }
    }
}
