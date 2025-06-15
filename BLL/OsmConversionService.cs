// שימוש בספרייה System.Diagnostics לצורך הרצת תהליך חיצוני (כמו הפעלת קובץ exe)
using System.Diagnostics;

namespace BLL
{
    // מחלקה סטטית שאחראית על המרת קובץ OSM לפורמט PBF בעזרת osmconvert.exe
    public static class OsmConversionService
    {
        // נתיב לקובץ ההמרה osmconvert.exe הנמצא בתוך תיקיית Tools של הפרויקט
        private static readonly string ConverterPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, // תיקיית ההרצה הנוכחית של האפליקציה
            "Tools",                               // תת-תיקייה בשם "Tools"
            "osmconvert.exe"                       // שם הקובץ עצמו
        );

        // פונקציה שמבצעת את ההמרה: מקבלת נתיב לקובץ OSM ומחזירה את הנתיב לקובץ PBF שהתקבל
        public static string ConvertOsmToPbf(string inputOsmPath)
        {
            // בדיקה האם הקובץ osmconvert.exe קיים – אם לא, נזרוק שגיאה
            if (!File.Exists(ConverterPath))
                throw new FileNotFoundException("osmconvert.exe לא נמצא בנתיב Tools");

            // קביעת הנתיב לקובץ הפלט – אותו נתיב כמו קובץ הקלט, אך עם סיומת .pbf
            string outputPbfPath = Path.ChangeExtension(inputOsmPath, ".pbf");

            // יצירת תהליך להרצת קובץ osmconvert.exe עם הפרמטרים הדרושים
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ConverterPath,                          // קובץ ההרצה (osmconvert.exe)
                    Arguments = $"\"{inputOsmPath}\" -o=\"{outputPbfPath}\"", // פרמטרים – קובץ קלט וקובץ פלט
                    RedirectStandardOutput = true,                    // ניתוב הפלט הסטנדרטי לקוד
                    RedirectStandardError = true,                     // ניתוב שגיאות לקוד
                    UseShellExecute = false,                          // לא להשתמש ב־CMD חיצוני
                    CreateNoWindow = true                             // לא לפתוח חלון קונסולה
                }
            };

            // התחלת ההרצה של התהליך
            process.Start();

            // קריאת הפלט שהחזירה התוכנה (אם יש)
            string stdOut = process.StandardOutput.ReadToEnd();

            // קריאת הודעות שגיאה (אם יש)
            string stdErr = process.StandardError.ReadToEnd();

            // המתנה לסיום התהליך
            process.WaitForExit();

            // אם לא נוצר קובץ הפלט – כנראה שההמרה נכשלה, נזרוק שגיאה עם פרטי השגיאה מהתהליך
            if (!File.Exists(outputPbfPath))
                throw new Exception($"המרה נכשלה: {stdErr}");

            // נחזיר את הנתיב לקובץ pbf שנוצר בהצלחה
            return outputPbfPath;
        }
    }
}
