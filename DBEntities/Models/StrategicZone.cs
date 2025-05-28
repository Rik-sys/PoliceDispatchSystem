using System;
using System.Collections.Generic;

namespace DBEntities.DBEntities.Models;

public partial class StrategicZone
{
    public int StrategicZoneId { get; set; }

    public int? EventId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int StrategyLevel { get; set; }

    public virtual Event? Event { get; set; }
}
