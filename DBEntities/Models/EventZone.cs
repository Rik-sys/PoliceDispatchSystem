using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class EventZone
{
    public int ZoneId { get; set; }

    public int? EventId { get; set; }

    public double Latitude1 { get; set; }

    public double Longitude1 { get; set; }

    public double Latitude2 { get; set; }

    public double Longitude2 { get; set; }

    public double Latitude3 { get; set; }

    public double Longitude3 { get; set; }

    public double Latitude4 { get; set; }

    public double Longitude4 { get; set; }

    public virtual Event? Event { get; set; }
}
