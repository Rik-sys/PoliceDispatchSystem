using System;
using System.Collections.Generic;

namespace DBEntities.DBEntities.Models;

public partial class Event
{
    public int EventId { get; set; }

    public string EventName { get; set; } = null!;

    public DateOnly EventDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int RequiredOfficers { get; set; }

    public string? Description { get; set; }

    public string? Priority { get; set; }

    public virtual ICollection<Call> Calls { get; set; } = new List<Call>();

    public virtual ICollection<EventZone> EventZones { get; set; } = new List<EventZone>();

    public virtual ICollection<OfficerAssignment> OfficerAssignments { get; set; } = new List<OfficerAssignment>();

    public virtual ICollection<StrategicZone> StrategicZones { get; set; } = new List<StrategicZone>();
}
