using AutoMapper;
using DBEntities.Models;
using DTO;
using IBL;

namespace BLL
{
    public class CallService : ICallService
    {
        private readonly ICallDAL _callDal;
        private readonly IMapper _mapper;

        public CallService(ICallDAL callDal)
        {
            _callDal = callDal;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CallDTO, Call>().ReverseMap();
            });
            _mapper = config.CreateMapper();
        }

        public int CreateCall(CallDTO callDto)
        {
            var entity = _mapper.Map<Call>(callDto);
            return _callDal.AddCall(entity);
        }

        public List<CallDTO> GetCallsByEvent(int eventId)
        {
            var calls = _callDal.GetCallsByEventId(eventId);
            return _mapper.Map<List<CallDTO>>(calls);
        }
        public List<CallDTO> GetAllCalls()
        {
            var calls = _callDal.GetAllCalls();
            return _mapper.Map<List<CallDTO>>(calls);
        }

    }
}