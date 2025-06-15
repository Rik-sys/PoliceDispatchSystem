using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//מייצגת קטע דרך בודד (קשת בגרף) שמקורו ב-way של OSM
namespace DTO
{
    //מחלקה בשימוש במספר מקומות, המרכזיים שהשתמשתי זה:
    //1.הקשת והצומת הרגילים לא שומרים את הקואורדינטות של כל הקטע דרך ואם לא זה, זה היה מקשה על מציאת דרכים קרובות לנקודה אסטרטגית
    //2.שמוסיפים נקודה אסטרטגית נדרש פיצול דרכים. ולכן חובה
    //3. לסינון דרכים שלא בתוך התחום
    //בגדול זה מחלקה שעוזרת לי לפשט בין הדרכים ב OSM למחלקות של הגרף שבניתי
    public class WaySegment
    {
        //osmהמקורי ב way
        public long WayId { get; set; }

        //מאיזה צומת -במילון
        public long FromNodeId { get; set; }

        //לאיזה צומת-במילון
        public long ToNodeId { get; set; }

        //הקואורדינטות הגיאוגרפיות של הקצוות, לשימוש בחישובים גיאומטריים
        //השתמשתי ב-tuple
        public (double lat, double lon) FromCoord { get; set; }
        public (double lat, double lon) ToCoord { get; set; }

        //סוג הדרך-משמש לסינון וכדומה
        public string HighwayType { get; set; } = "";
    }
}
