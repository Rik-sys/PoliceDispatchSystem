﻿using DBEntities.Models;
using IDAL;

namespace DAL
{
    public class OfficerAssignmentDAL : IOfficerAssignmentDAL
    {
        private readonly PoliceDispatchSystemContext _context;

        public OfficerAssignmentDAL(PoliceDispatchSystemContext context)
        {
            _context = context;
        }

        public void AddAssignments(List<OfficerAssignment> list)
        {
            foreach (var assignment in list)
            {
                if (!_context.OfficerAssignments.Any(a =>
                    a.PoliceOfficerId == assignment.PoliceOfficerId &&
                    a.EventId == assignment.EventId))
                {
                    _context.OfficerAssignments.Add(assignment);
                }
            }
            _context.SaveChanges();

        }

        public List<OfficerAssignment> GetAssignmentsByEventId(int eventId)
        {
            return _context.OfficerAssignments
                .Where(oa => oa.EventId == eventId)
                .ToList();
        }

        public List<OfficerAssignment> GetAssignmentsByOfficerId(int officerId)
        {
            return _context.OfficerAssignments
                .Where(oa => oa.PoliceOfficerId == officerId)
                .ToList();
        }
        public List<OfficerAssignment> GetAllAssignments()
        {
            return _context.OfficerAssignments.ToList();
        }

    }
}