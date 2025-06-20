using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DBEntities.Models;
using DTO;

namespace IDAL
{
    public interface IPoliceOfficerDAL
    {
        List<PoliceOfficer> GetAvailableOfficersWithUsers(DateOnly date, TimeOnly start, TimeOnly end);
        List<PoliceOfficer> GetAllOfficersWithUsers();
        PoliceOfficer GetOfficerWithUserById(int officerId);
        OfficerStatusDTO GetOfficerStatus(int officerId);

    }
}
