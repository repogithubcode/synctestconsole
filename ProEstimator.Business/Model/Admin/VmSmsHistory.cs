using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmSmsHistory : IModelMap<VmSmsHistory>
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public string DateSent { get; set; }
        public string SalesRep { get; set; }
        public int? SalesRepId { get; set; }
        public int? LoginId { get; set; }
        public VmSmsHistory ToModel(DataRow row)
        {
            var model = new VmSmsHistory();
            model.Id = (int)row["Id"];
            model.Message = row["Message"].SafeString();
            model.SalesRep = row["UserName"].SafeString();
            model.PhoneNumber = row["PhoneNumber"].SafeString();
            model.DateSent = row["CreateDate"].SafeDate();

            return model;
        }
    }
}
