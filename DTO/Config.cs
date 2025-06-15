namespace DTO
{
    public static class Config
    {
        public static double ReverseDirectionPenalty { get; set; } = 3.0;
        public static bool AllowReverseDirection { get; set; } = true;

        //תדפיס ללוג כדי שאני יראה תוצאות
        public static bool VerboseOnewayLogging { get; set; } = false;
    }
}
