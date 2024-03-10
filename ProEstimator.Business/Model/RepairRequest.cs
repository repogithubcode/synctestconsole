using System;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.LkqService;
using ProEstimator.Business.Type;

namespace ProEstimator.Business.Model
{
    public class RepairRequest : IMapTo<UserInformation>, IMapTo<RegisterRepairFacilityRequest>, IMapTo<UnRegisterRepairFacilityRequest>, IMapTo<PurchaseOrderRequest>
    {
        public string RepairFacilityID { get; set; }
        public string SampleInvoiceNumber { get; set; }
        public string SampleInvoiceTotalAmount { get; set; }
        public AccountNumberBusinessType BusinessTypeForAccountNumber { get; set; }
        public Guid VerificationCode { get; set; }

        public RegistrationType RegistrationType { get; set; }


        public UserInformation ToModel(UserInformation to)
        {
            return to;
        }

        public RegisterRepairFacilityRequest ToModel(RegisterRepairFacilityRequest to)
        {
            return to;
        }

        public UnRegisterRepairFacilityRequest ToModel(UnRegisterRepairFacilityRequest to)
        {
            return to;
        }

        public PurchaseOrderRequest ToModel(PurchaseOrderRequest to)
        {
            return to;
        }
    }
}
