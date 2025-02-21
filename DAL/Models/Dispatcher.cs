using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Dispatcher
{
    public int DispatcherId { get; set; }

    public virtual User DispatcherNavigation { get; set; } = null!;
}
