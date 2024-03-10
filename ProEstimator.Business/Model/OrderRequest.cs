using System;
using System.Collections.Generic;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.LkqService;

namespace ProEstimator.Business.Model
{
    public class OrderRequest : IMapTo<UserRequest>, IMapTo<PurchaseOrderRequest>
    {
        public string EstimateId { get; set; }
        public AccountNumberBusinessType BusinessType { get; set; }
        public Guid VerificationCode { get; set; }
        public string PartnerKey { get; set; }
        public string ClaimNumber { get; set; }
        public string InsuranceCompanyParent { get; set; }
        public List<PurchaseOrderPart> Parts { get; set; }
        public List<PurchaseDetails> Details { get; set; }
        public string Year { get; set; }
        public string Model { get; set; }
        public string Vin { get; set; }
        public string RepairFacilityId { get; set; }
        public DateTime? RequestedDeliveryDate { get; set; }
        public int RetryCount { get; set; }
        public string RepairFacility { get; set; }
        public Vehicle Vehicle { get; set; }
        public UserRequest ToModel(UserRequest to)
        {
            return to;
        }

        public PurchaseOrderRequest ToModel(PurchaseOrderRequest to)
        {
            return to;
        }
    }
}
