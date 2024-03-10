using System;
using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmCreateOrg : IModelMap<VmCreateOrg>
    {
        public decimal OrgId { get; set; }
        public string CompanyName { get; set; }
        public VmCreateOrg ToModel(DataRow row)
        {
            var model = new VmCreateOrg();

            try
            {
                model.OrgId = (decimal)row["OrgId"];
            }
            catch (Exception)
            {
                var myInt = (int)row["OrgId"];
                model.OrgId = myInt;
            }
            
            
            model.CompanyName = row["CompanyName"].SafeString();

            return model;
        }
    }
}
