using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class OfficerAssignment
{
    public int PoliceOfficerId { get; set; }

    public int EventId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual PoliceOfficer PoliceOfficer { get; set; } = null!;
}
