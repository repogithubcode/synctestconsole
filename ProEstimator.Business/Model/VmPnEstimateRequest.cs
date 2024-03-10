using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Model
{
    public class VmPnEstimateRequest
    {
        public VmPnEstimateRequest()
        {
            Lines = new List<VmPnRequestLineItem>();
        }

        public string WebestVersion { get; set; }
        public string ReferenceNumber { get; set; }
        public string SupplementNumber { get; set; }
        public string CreateDate { get; set; }
        public string Vin { get; set; }
        public string Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string CustomerName { get; set; }
        public string InsurerName { get; set; }
        public string EstimaterName { get; set; }
        public Guid ShopId { get; set; }
        public List<VmPnRequestLineItem> Lines { get; set; }
    }
}
