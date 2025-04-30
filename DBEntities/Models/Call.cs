using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Call
{
    public int CallId { get; set; }

    public int? EventId { get; set; }

    public string Address { get; set; } = null!;

    public int RequiredOfficers { get; set; }

    public string ContactPhone { get; set; } = null!;

    public int UrgencyLevel { get; set; }

    public DateTime CallTime { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<CallAssignment> CallAssignments { get; set; } = new List<CallAssignment>();

    public virtual Event? Event { get; set; }
}
