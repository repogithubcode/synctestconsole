using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Model
{
    public class VmPnEnrollResponse : IModelMap<VmPnEnrollResponse>
    {
        public string RequestId { get; set; }
        public bool Success { get; set; }
        public string FailedReason { get; set; }
        public string ShopId { get; set; }
        public string ShopUri { get; set; }
        public int LoginId { get; set; }
        public bool Enrollment { get; internal set; }

        public VmPnEnrollResponse ToModel(DataRow row)
        {
            var model = new VmPnEnrollResponse();
            model.RequestId = row["RequestId"].SafeString();
            model.ShopId = row["ShopId"].SafeString();
            model.ShopUri = row["ShopUri"].SafeString();
            model.LoginId = (int) row["LoginId"];

            return model;
        }
    }
}