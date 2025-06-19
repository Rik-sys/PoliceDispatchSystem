
using DTO;
using static DTO.CallResponsesDTO;

namespace IBL
{
    public interface ICallManagementService
    {
        CallCreationResponse CreateCall(CallDTO request);
    }
}
