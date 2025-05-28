using System;
using System.Collections.Generic;

namespace DBEntities.DBEntities.Models;

public partial class PoliceOfficer
{
    public int PoliceOfficerId { get; set; }

    public int? VehicleTypeId { get; set; }

    public virtual ICollection<CallAssignment> CallAssignments { get; set; } = new List<CallAssignment>();

    public virtual ICollection<OfficerAssignment> OfficerAssignments { get; set; } = new List<OfficerAssignment>();

    public virtual User PoliceOfficerNavigation { get; set; } = null!;

    public virtual VehicleType? VehicleType { get; set; }
}
