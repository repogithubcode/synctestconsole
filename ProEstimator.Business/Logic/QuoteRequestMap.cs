using AutoMapper;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.LkqService;
using model = ProEstimator.Business.Model;

namespace ProEstimator.Business.Logic
{
    public class QuoteRequestMap : IMapper<model.QuoteRequest>
    {
        private readonly IMapper _mapper;

        public QuoteRequestMap()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<model.QuoteRequest, UserInformation>()
                    .ForMember(dest => dest.BusinessTypeForAccountNumber, src => src.MapFrom(s => s.BusinessType))
                    .ForMember(dest => dest.VerificationCode, src => src.MapFrom(s => s.VerificationCode))
                    .ForMember(dest => dest.AccountNumber, src => src.MapFrom(s => s.PartnerKey));
                cfg.CreateMap<model.QuoteRequest, Vehicle>()
                    .ForMember(dest => dest.Year, src => src.MapFrom(s => s.Year))
                    .ForMember(dest => dest.Model, src => src.MapFrom(s => s.Model))
                    .ForMember(dest => dest.VIN, src => src.MapFrom(s => s.Vin));
                cfg.CreateMap<model.QuoteRequest, Estimate>()
                    .ForMember(dest => dest.ClaimNumber, src => src.MapFrom(s => s.ClaimNumber))
                    .ForMember(dest => dest.CreatedDate, src => src.MapFrom(s => s.CreatedDate))
                    .ForMember(dest => dest.InsuranceCompanyParent, src => src.MapFrom(s => s.InsuranceCompanyParent))
                    .ForMember(dest => dest.IsInsuranceQuote, src => src.MapFrom(s => s.InsuranceQuote))
                    .ForMember(dest => dest.LineItems, src => src.MapFrom(s => s.Parts))
                    .ForMember(dest => dest.PartnerKey, src => src.MapFrom(s => s.PartnerKey))
                    .ForMember(dest => dest.PostalCode, src => src.MapFrom(s => s.PostalCode))
                    .ForMember(dest => dest.RepairFacilityId, src => src.MapFrom(s => s.RepairFacility))
                    .ForMember(dest => dest.Vehicle, src => src.MapFrom(s => s));
                cfg.CreateMap<model.QuoteRequest, UserRequest>()
                    .ForMember(dest => dest.UserRequestInfo, src => src.MapFrom(s => s));
            });

            _mapper = config.CreateMapper();
        }
        public Y Convert<Y>(model.QuoteRequest item) where Y : new()
        {
            var dest = new Y();
            _mapper.Map(item, dest);
            return dest;
        }

        public void MapTo<Y>(Y src, model.QuoteRequest dest)
        {
            _mapper.Map(src, dest);
        }

        public void MapFrom<Y>(model.QuoteRequest src, Y dest)
        {
            _mapper.Map(src, dest);
        }

        public model.QuoteRequest Generate<Y>(Y item)
        {
            return _mapper.Map<model.QuoteRequest>(item);
        }
    }
}
