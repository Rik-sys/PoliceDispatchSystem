using DTO;
using IBL;
using IDAL;

namespace BLL
{
    public class PoliceOfficerService : IPoliceOfficerService
    {
        private readonly IPoliceOfficerDAL _officerDal;

        public PoliceOfficerService(IPoliceOfficerDAL officerDal)
        {
            _officerDal = officerDal;
        }

        public OfficerStatusDTO GetOfficerStatus(int officerId)
        {
            return _officerDal.GetOfficerStatus(officerId);
        }
    }

}
