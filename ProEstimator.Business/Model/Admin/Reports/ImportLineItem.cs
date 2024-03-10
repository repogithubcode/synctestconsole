using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin.Reports
{
    public class ImportLineItem : IModelMap<ImportLineItem>
    {
        public int LoginId { get; set; }
        public string LoginMoved { get; set; }
        public string CompanyName { get; set; }
        public string MovedBy { get; set; }
        public string SalesRep { get; set; }
        public string SelfImport { get; set; }
        public string ConversionComplete { get; set; }
        public bool Trial { get; set; }
        public bool ActiveTrial { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpirationDate { get; set; }
        public ImportLineItem ToModel(DataRow row)
        {
            var model = new ImportLineItem();
            model.LoginId = (int)row["LoginID"];
            model.LoginMoved = row["LoginMoved"].SafeString();
            model.CompanyName = row["companyName"].SafeString();
            model.MovedBy = row["Moved_By"].SafeString();
            model.SelfImport = row["SelfImport"].SafeString();
            model.ConversionComplete = row["Conversion_complete"].SafeString();
            model.SalesRep = row["Sales_Rep"].SafeString();
            model.EffectiveDate = row["effectiveDate"].SafeDate();
            model.EffectiveDate = row["expirationDate"].SafeDate();
            model.Trial = (int)row["Trial"] == 1;
            model.ActiveTrial = (int)row["ActiveTrial"] == 1;
            return model;
        }
    }
}