using ProEstimator.Business.ILogic;
using System.Data;
using ProEstimator.Business.Extension;

namespace ProEstimator.Business.Model.Admin.Reports
{
    public class RenewalReportDisplaySettings : IModelMap<RenewalReportDisplaySettings>
    {
        public int Id { get; set; }
        public int SalesRepId { get; set; }
        public string SettingsKey { get; set; }
        public string CreateDate { get; set; }
        public string ModifiedDate { get; set; }

        public RenewalReportDisplaySettings ToModel(DataRow row)
        {
            var model = new RenewalReportDisplaySettings();
            model.Id = (int)row["ID"];
            model.SalesRepId = (int)row["SalesRepId"];
            model.SettingsKey = row["SettingsKey"].SafeString();
            model.CreateDate = row["CreateDate"].SafeDate();
            model.ModifiedDate = row["ModifiedDate"].SafeDate();

            return model;
        }
    }
}
