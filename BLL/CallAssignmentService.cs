using AutoMapper;
using DBEntities.Models;
using DTO;
using IBL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class CallAssignmentService : ICallAssignmentService
    {
        private readonly ICallAssignmentDAL _dal;
        private readonly IMapper _mapper;

        public CallAssignmentService(ICallAssignmentDAL dal)
        {
            _dal = dal;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CallAssignmentDTO, CallAssignment>().ReverseMap();
            });
            _mapper = config.CreateMapper();
        }

        public void AssignOfficersToCall(List<CallAssignmentDTO> assignments)
        {
            var entities = _mapper.Map<List<CallAssignment>>(assignments);
            _dal.AddAssignments(entities);
        }

        public List<CallAssignmentDTO> GetAssignmentsByCall(int callId)
        {
            var entities = _dal.GetAssignmentsByCallId(callId);
            return _mapper.Map<List<CallAssignmentDTO>>(entities);
        }
    }
}
