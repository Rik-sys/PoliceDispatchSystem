
//ממשק לניהול מידע של שוטרים
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
        PoliceOfficer? GetOfficerById(int officerId);
    }
}
