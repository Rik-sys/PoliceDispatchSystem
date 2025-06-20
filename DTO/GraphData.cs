// משמשת כ־אובייקט עטיפה (container) שנועד לשמור את כל המידע הרלוונטי לגבי גרף התחבורה שנבנה מתוך קובץ OSM.
namespace DTO
{
    public class GraphData
    {
        public Dictionary<long, (double lat, double lon)> Nodes { get; set; }
        public Graph Graph { get; set; }
        public Dictionary<long, bool> NodesInOriginalBounds { get; set; }

        //מתי הגרף נוצר
        public DateTime CreatedAt { get; set; }
    }

}
