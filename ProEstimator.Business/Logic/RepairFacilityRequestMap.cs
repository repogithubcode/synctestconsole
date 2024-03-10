using AutoMapper;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.LkqService;
using model = ProEstimator.Business.Model;

namespace ProEstimator.Business.Logic
{
    public class RepairFacilityRequestMap : IMapper<model.RepairRequest>
    {
        private readonly IMapper _mapper;

        public RepairFacilityRequestMap()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<model.RepairRequest, UserInformation>()
                    .ForMember(dest => dest.BusinessTypeForAccountNumber, src => src.MapFrom(s => s.BusinessTypeForAccountNumber))
                    .ForMember(dest => dest.VerificationCode, src => src.MapFrom(s => s.VerificationCode));
                cfg.CreateMap<model.RepairRequest, RegisterRepairFacilityRequest>()
                    .ForMember(dest => dest.RepairFacilityId, src => src.MapFrom(s => s.RepairFacilityID))
                    .ForMember(dest => dest.SampleInvoiceNumber, src => src.MapFrom(s => s.SampleInvoiceNumber))
                    .ForMember(dest => dest.SampleInvoiceTotalAmount, src => src.MapFrom(s => s.SampleInvoiceNumber))
                    .ForMember(dest => dest.UserRequestInfo, src => src.MapFrom(s => s));
                cfg.CreateMap<model.RepairRequest, UnRegisterRepairFacilityRequest>()
                    .ForMember(dest => dest.RepairFacilityId, src => src.MapFrom(s => s.RepairFacilityID));
            });

            _mapper = config.CreateMapper();
        }
        public Y Convert<Y>(model.RepairRequest item) where Y : new()
        {
            var dest = new Y();
            _mapper.Map(item, dest);
            return dest;
        }

        public void MapTo<Y>(Y src, model.RepairRequest dest)
        {
            _mapper.Map(src, dest);
        }

        public void MapFrom<Y>(model.RepairRequest src, Y dest)
        {
            _mapper.Map(src, dest);
        }

        public model.RepairRequest Generate<Y>(Y item)
        {
            return _mapper.Map<model.RepairRequest>(item);
        }
    }
}
