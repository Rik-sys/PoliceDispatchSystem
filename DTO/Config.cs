namespace DTO
{

    /// <summary>
    /// מחלקת קונפיגורציה גלובלית להגדרות תנועה חד־סטרית עונשים, הרשאות ולוגים-
    /// עונש על נסיעה הפוכה
    /// </summary>

    public static class Config
    {
        // מקדם עונש למרחק במקרה של נסיעה נגד כיוון החוקי
        public static double ReverseDirectionPenalty { get; set; } = 3.0;

        // האם לאפשר הוספת קשת הפוכה לדרך חד-כיוונית
        public static bool AllowReverseDirection { get; set; } = true;

        //תדפיס ללוג כדי שאני יראה תוצאות
        public static bool VerboseOnewayLogging { get; set; } = false;
    }
}
