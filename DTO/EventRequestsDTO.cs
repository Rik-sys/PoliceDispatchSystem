namespace DTO
{
    //מחלקה ראשית לבקשות ותגובות הקשורות ליצירת אירוע והצגת פרטים של האירוע
    public class EventRequestsDTO
    {
        //בקשת יצירת אירוע הכוללת פרטים בסיסיים ותחום גאוגרפי, עם אזורים אסטרטגיים
        public class CreateEventRequestDTO
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string Priority { get; set; } = "";
            public string StartDate { get; set; } = "";
            public string EndDate { get; set; } = "";
            public string StartTime { get; set; } = "";
            public string EndTime { get; set; } = "";
            public int RequiredOfficers { get; set; }
            public List<List<double>> SelectedArea { get; set; } = new();
            public List<StrategicZoneDTO> StrategicZones { get; set; } = new();
        }


        //בקשת יצירת אירוע הכוללת גם מיקומים מחושבים מראש לפיזור השוטרים
        public class CreateEventWithPositionsRequestDTO
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string Priority { get; set; } = "";
            public string StartDate { get; set; } = "";
            public string EndDate { get; set; } = "";
            public string StartTime { get; set; } = "";
            public string EndTime { get; set; } = "";
            public int RequiredOfficers { get; set; }
            public List<List<double>> SelectedArea { get; set; } = new();
            public List<StrategicZoneDTO> StrategicZones { get; set; } = new();
            public List<PreCalculatedPositionDTO> PreCalculatedPositions { get; set; } = new();
        }

        //מייצג מיקום שוטר מחושב מראש כולל מידע על היותו אסטרטגי ומזהה צומת.
        public class PreCalculatedPositionDTO
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public bool IsStrategic { get; set; }
            public long NodeId { get; set; }
        }

        // תוצאת יצירת אירוע כולל סטטוס, מזהה האירוע, וסיכום נתונים
        public class EventCreationResultDTO
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
            public int? EventId { get; set; }
            public int OfficerCount { get; set; }
            public int StrategicOfficers { get; set; }
            public int RegularOfficers { get; set; }
            public int NodesCreatedOnRealRoads { get; set; }
            public EventCreationDebugInfoDTO? DebugInfo { get; set; }
            public List<string> Errors { get; set; } = new();
        }


        //מידע דיבאג פנימי על תהליך יצירת אירוע ופיזור השוטרים
        public class EventCreationDebugInfoDTO
        {
            public int OriginalStrategicZones { get; set; }
            public int FoundStrategicNodes { get; set; }
            public int TotalNodesInBounds { get; set; }
            public int TotalWaySegments { get; set; }
            public int SelectedNodes { get; set; }
            public List<long> StrategicNodeIds { get; set; } = new();
            public List<long> MissingStrategicNodes { get; set; } = new();
        }


        //מייצג אירוע עם כל פרטיו המלאים כולל אזור, אזורים אסטרטגיים, ושיבוץ שוטרים
        public class EventWithDetailsDTO
        {
            public EventDTO Event { get; set; } = new();
            public EventZoneDTO? Zone { get; set; }
            public List<StrategicZoneDTO> StrategicZones { get; set; } = new();
            public List<OfficerAssignmentDTO> OfficerAssignments { get; set; } = new();
            public List<PoliceOfficerDTO> AssignedOfficers { get; set; } = new();
        }


        //תוצאת הרצת אלגוריתם פיזור (K-Center) הכוללת את המרכזים והרדיוס המקסימלי.
        public class KCenterDistributionResultDTO
        {
            public List<long> CenterNodes { get; set; } = new();
            public double MaxDistance { get; set; }
            public bool Success { get; set; } = true;
            public string ErrorMessage { get; set; } = "";
        }
    }
}
