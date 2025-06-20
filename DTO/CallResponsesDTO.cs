namespace DTO
{
    //מחלקות של תגובת השרת ליצירת קריאות
    public class CallResponsesDTO
    {

        // מייצגת את הפלט המרכזי של תהליך שיוך קריאה – כולל נתוני הקריאה, שוטרים ששובצו, והודעות
        public class CallCreationResponse
        {
            public int CallId { get; set; }
            public int AssignedToCall { get; set; }
            public int ReassignedOfficers { get; set; }
            public int TotalAvailableOfficers { get; set; }
            public string Message { get; set; } = string.Empty;
            public CallInfoResponse CallInfo { get; set; } = new();
            public List<AssignedOfficerResponse> AssignedOfficersList { get; set; } = new();
            public List<ReassignedOfficerResponse> ReassignedOfficersList { get; set; } = new();
        }

        // מכילה מידע כללי על הקריאה שנוצרה – מזהה, מיקום, מספר נדרש ומספר שובץ בפועל
        public class CallInfoResponse
        {
            public int Id { get; set; }
            public LocationResponse CallLocation { get; set; } = new();
            public int RequiredOfficers { get; set; }
            public int ActualAssigned { get; set; }
        }


        // מייצגת שוטר ששובץ לקריאה – כולל מזהה, מיקום נוכחי ומרחק מהקריאה
        public class AssignedOfficerResponse
        {
            public int OfficerId { get; set; }
            public LocationResponse OfficerLocation { get; set; } = new();
            public double DistanceToCall { get; set; }
        }

        public class ReassignedOfficerResponse
        {
            public int OfficerId { get; set; }
            public LocationResponse NewOfficerLocation { get; set; } = new();
        }

        public class LocationResponse
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}
