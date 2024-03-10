using System.Collections.Generic;
using ProEstimator.Business.LkqService;

namespace ProEstimator.Business.Model
{
    public class OrderResponse
    {
        public List<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
    }
}
