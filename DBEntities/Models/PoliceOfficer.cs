using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class PoliceOfficer
{
    public int PoliceOfficerId { get; set; }

    public int? VehicleTypeId { get; set; }

    public virtual ICollection<CallAssignment> CallAssignments { get; set; } = new List<CallAssignment>();

    public virtual User PoliceOfficerNavigation { get; set; } = null!;

    public virtual VehicleType? VehicleType { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
