# 🚓 Police Dispatch System - Smart Patrol Allocation

מערכת מתקדמת לפיזור שוטרים בשטח בזמן אמת תוך אופטימיזציה של זמני תגובה. הפרויקט עושה שימוש בנתוני מפה אמיתיים (OSM), מבצע המרה לגרף תחבורתי, ומפזר שוטרים באופן חכם על בסיס אלגוריתם **k-Center**.

 מטרות המערכת

- לאפשר למוקדניות לבחור אזור גיאוגרפי רלוונטי מהמפה.
- לטעון נתוני כבישים אמיתיים מתוך OSM ולבנות מהם גרף קשיר.
- לפתור את בעיית ה־k-Center ולחשב מיקום אופטימלי לשוטרים כך שזמן התגובה המקסימלי לכל קריאה יהיה מינימלי.
- להציג את מיקומי השוטרים ואת רדיוסי הכיסוי שלהם על המפה בצורה ברורה, עם התחשבות במהירות נסיעה.

---

 טכנולוגיות

| שכבה | טכנולוגיה | תיאור |
|------|------------|--------|
| Frontend | React + TypeScript + Leaflet | ממשק משתמש אינטראקטיבי עם מפת OSM וכלים גרפיים |
| Backend | ASP.NET Core Web API | שרת API לעיבוד קבצים, בניית גרפים והרצת אלגוריתמים |
| Algorithm | Hochbaum & Shmoys | אלגוריתם קירוב לבעיית k-Center עם יחס אופטימלי מוכח |
| Map Data | OpenStreetMap + Overpass API | נתוני דרכים וכבישים אמיתיים |
| Data Structure | Custom Graph + Dijkstra | בניית גרף תחבורתי לצורך חישובי מרחקים |

---

 מבנה המערכת

- **בחירת אזור במפה** ע״י ציור מלבן
- **הורדת נתוני OSM** מאזור הבחירה (כבישים בלבד)
- **שליחה לשרת** לבניית גרף תחבורתי קשיר (כולל תיקונים אם צריך)
- **הרצת אלגוריתם K-Center** לפיזור שוטרים אופטימלי
- **הצגת השוטרים במפה** כולל טווח תגובה לפי זמן (מהירות רכב)

---

 אלגוריתם K-Center

המערכת משתמשת באלגוריתם של **Hochbaum & Shmoys**, שהוא אלגוריתם קירוב (approximation algorithm) לבעיית ה־k-Center. האלגוריתם מספק יחס קירוב של 2, כלומר הוא מבטיח שהמרחק המקסימלי לאחד המרכזים (שוטרים) לא יעלה על פי 2 מהפתרון האופטימלי המינימלי.

---

תפעול

1. בחירת אזור במפה
2. לחיצה על "סיום ושליחה לשרת"
3. בחירה במספר שוטרים
4. לחיצה על "פזר שוטרים על המפה"
5. קבלת תצוגה מלאה של מיקומי השוטרים וזמן התגובה המקסימלי

---

התקנה והרצה מקומית

### דרישות:
- Node.js + pnpm / npm
- .NET 6 או 7
- Overpass API (או חיבור אינטרנטי אליו)
- גישה ל־localhost ב־7163

### הוראות:
```bash
git clone https://github.com/your-username/police-dispatch-system.git
cd police-dispatch-system

# התקנת צד לקוח
cd client
pnpm install
pnpm dev

# התקנת צד שרת
cd ../server
dotnet build
dotnet run
```

---

 דוגמת תוצאה

- 5 שוטרים פוזרו באזור תל אביב
- זמן תגובה מקסימלי: 11.3 שניות
- כל נקודה באזור מכוסה ע"י שוטר בטווח זמן זה

---

 הרחבות עתידיות

- שילוב סוגי כלי רכב שונים ומהירויות שונות
- הצגת היסטוריית פיזורים ואירועים
- תכנון מסלולי תגובה חכמים בזמן אמת

---

פיתוח ותרומה

מוזמנים להוסיף יכולות חדשות, לשפר את הקוד או להציע פיצ'רים דרך Issues ו־Pull Requests!

---

 רישוי

MIT License
