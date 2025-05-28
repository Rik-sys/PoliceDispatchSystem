using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class CallAssignment
{
    public int PoliceOfficerId { get; set; }

    public int CallId { get; set; }

    public DateTime AssignmentTime { get; set; }

    public virtual Call Call { get; set; } = null!;

    public virtual PoliceOfficer PoliceOfficer { get; set; } = null!;
}
