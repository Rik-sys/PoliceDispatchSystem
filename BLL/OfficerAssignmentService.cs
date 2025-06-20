using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using DBEntities.Models;
using DTO;
using IBL;
using IDAL;

namespace BLL
{
    public class OfficerAssignmentService : IOfficerAssignmentService
    {
        private readonly IOfficerAssignmentDAL _officerAssignmentDal;
        private readonly IMapper _mapper;

        public OfficerAssignmentService(IOfficerAssignmentDAL officerAssignmentDal)
        {
            _officerAssignmentDal = officerAssignmentDal;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OfficerAssignmentDTO, OfficerAssignment>().ReverseMap();
            });
            _mapper = config.CreateMapper();
        }

        public void AddOfficerAssignments(List<OfficerAssignmentDTO> assignments)
        {
            // המרה מ-DTO ל-Entity
            var entities = _mapper.Map<List<OfficerAssignment>>(assignments);

            // שמירה דרך שכבת ה-DAL
            _officerAssignmentDal.AddAssignments(entities);
        }

        public List<OfficerAssignmentDTO> GetAssignmentsByEventId(int eventId)
        {
            var entities = _officerAssignmentDal.GetAssignmentsByEventId(eventId);
            return _mapper.Map<List<OfficerAssignmentDTO>>(entities);
        }

        public List<OfficerAssignmentDTO> GetAssignmentsByOfficerId(int officerId)
        {
            var entities = _officerAssignmentDal.GetAssignmentsByOfficerId(officerId);
            return _mapper.Map<List<OfficerAssignmentDTO>>(entities);
        }
        public List<OfficerAssignmentDTO> GetAllAssignments()
        {
            var entities = _officerAssignmentDal.GetAllAssignments();
            return _mapper.Map<List<OfficerAssignmentDTO>>(entities);
        }

    }
}

