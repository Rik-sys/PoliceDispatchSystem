using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface ICallService
    {
        int CreateCall(CallDTO callDto);
        List<CallDTO> GetCallsByEvent(int eventId);
    }
}
