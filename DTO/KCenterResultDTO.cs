namespace DTO
{
    public class KCenterResultDTO
    {
        public List<OfficerAssignmentDTO> PolicePositions { get; set; } = new List<OfficerAssignmentDTO>();
        public double MaxDistance { get; set; } // מטרים
        public double MaxDistanceInKilometers { get; set; } // קילומטרים
        public int StrategicOfficers { get; set; }
        public int RegularOfficers { get; set; }
        public int NodesCreatedOnRoads { get; set; }
        public string Message { get; set; }
        public int? EventId { get; set; }
        public List<long> StrategicNodeIds { get; set; } = new List<long>(); // כדי לדעת אילו מהצמתים הם אסטרטגיים
    }
}