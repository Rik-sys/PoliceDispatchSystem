//ממשק לוגי לשוטר
using DTO;

namespace IBL
{
    public interface IPoliceOfficerService
    {
        OfficerStatusDTO GetOfficerStatus(int officerId);

    }
}
