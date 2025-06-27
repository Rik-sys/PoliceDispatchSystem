namespace DTO
{
    public class DistributeWithStrategicRequest
    {
        public int K { get; set; }
        public List<StrategicZoneRequest> StrategicZones { get; set; } = new List<StrategicZoneRequest>();
    }
}