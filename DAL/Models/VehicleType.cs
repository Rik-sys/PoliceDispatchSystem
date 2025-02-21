using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class VehicleType
{
    public int VehicleTypeId { get; set; }

    public string VehicleName { get; set; } = null!;

    public virtual ICollection<PoliceOfficer> PoliceOfficers { get; set; } = new List<PoliceOfficer>();
}
