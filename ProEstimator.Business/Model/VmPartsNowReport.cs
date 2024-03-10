using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Model
{
    public class VmPartsNowReport
    {
        public string Id { get; set; }
        public string LoginId { get; set; }
        public string EstimateUri { get; set; }
        public string EstimateId { get; set; }
        public string Status { get; set; }
        public string SupercededId { get; set; }
    }

    public class Query
    {
        public DateTime from { get; set; }
        public DateTime to { get; set; }
    }

    public class Summary
    {
        public string id { get; set; }
        public string buyerId { get; set; }
        public DateTime created { get; set; }
        public string referenceNumber { get; set; }
        public string status { get; set; }
        public string customerName { get; set; }
        public string vin { get; set; }
        public string orderId { get; set; }
        public string supercededByEstimateId { get; set; }
    }

    public class Result
    {
        public string estimateId { get; set; }
        public string estimateUri { get; set; }
        public Summary summary { get; set; }
    }

    public class PnApiResponse
    {
        public Query query { get; set; }
        public List<Result> results { get; set; }
        public string requestId { get; set; }
        public bool success { get; set; }
    }
}
