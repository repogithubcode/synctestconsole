using AutoMapper;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.LkqService;
using model = ProEstimator.Business.Model;

namespace ProEstimator.Business.Logic
{
    public class OrderRequestMap : IMapper<model.OrderRequest>
    {
        private readonly IMapper _mapper;

        public OrderRequestMap()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<model.OrderRequest, UserRequest>()
                    .ForMember(dest => dest.UserRequestInfo, src => src.MapFrom(s => s));
                cfg.CreateMap<model.OrderRequest, UserInformation>()
                    .ForMember(dest => dest.BusinessTypeForAccountNumber, src => src.MapFrom(s => s.BusinessType))
                    .ForMember(dest => dest.VerificationCode, src => src.MapFrom(s => s.VerificationCode))
                    .ForMember(dest => dest.AccountNumber, src => src.MapFrom(s => s.PartnerKey));
                cfg.CreateMap<model.OrderRequest, Vehicle>()
                    .ForMember(dest => dest.Year, src => src.MapFrom(s => s.Year))
                    .ForMember(dest => dest.Model, src => src.MapFrom(s => s.Model))
                    .ForMember(dest => dest.VIN, src => src.MapFrom(s => s.Vin));
                cfg.CreateMap<model.OrderRequest, PurchaseOrderRequest>()
                    .ForMember(dest => dest.ClaimNumber, src => src.MapFrom(s => s.ClaimNumber))
                    .ForMember(dest => dest.InsuranceCompanyParent, src => src.MapFrom(s => s.InsuranceCompanyParent))
                    .ForMember(dest => dest.PartnerKey, src => src.MapFrom(s => s.PartnerKey))
                    .ForMember(dest => dest.Parts, src => src.MapFrom(s => s.Parts))
                    .ForMember(dest => dest.RepairFacilityId, src => src.MapFrom(s => s.RepairFacilityId))
                    .ForMember(dest => dest.RequestedDeliveryDate, src => src.MapFrom(s => s.RequestedDeliveryDate))
                    .ForMember(dest => dest.RetryCount, src => src.MapFrom(s => s.RetryCount))
                    .ForMember(dest => dest.Vehicle, src => src.MapFrom(s => s));
            });

            _mapper = config.CreateMapper();
        }
        public Y Convert<Y>(model.OrderRequest item) where Y : new()
        {
            var dest = new Y();
            _mapper.Map(item, dest);
            return dest;
        }

        public void MapTo<Y>(Y src, model.OrderRequest dest)
        {
            _mapper.Map(src, dest);
        }

        public void MapFrom<Y>(model.OrderRequest src, Y dest)
        {
            _mapper.Map(src, dest);
        }

        public model.OrderRequest Generate<Y>(Y item)
        {
            return _mapper.Map<model.OrderRequest>(item);
        }
    }
}
