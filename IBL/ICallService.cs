using DTO;

namespace IBL
{
    public interface ICallService
    {
        int CreateCall(CallDTO callDto);
        List<CallDTO> GetCallsByEvent(int eventId);
        List<CallDTO> GetAllCalls();

    }
}
