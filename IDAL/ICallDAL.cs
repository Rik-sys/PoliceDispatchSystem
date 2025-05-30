using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ICallDAL
{
    int AddCall(Call call);
    List<Call> GetCallsByEventId(int eventId);
}