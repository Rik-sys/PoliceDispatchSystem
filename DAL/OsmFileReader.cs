


//משולב גרף לאודר וזה עצמו אבל הגרף לא ממש מייצג את הרחובות,הוא לוקח קצה והתחלה של רחוב
using OsmSharp.Streams;
using OsmSharp;


namespace DAL
{
    public static class OsmFileReader
    {
        private static readonly HashSet<string> allowedHighwayTypes = new HashSet<string>
        {
            "residential", "primary", "secondary", "tertiary",
            "unclassified", "service", "living_street", "pedestrian",
            "footway", "path", "cycleway", "track"
        };

        /// <summary>
        /// קריאת קובץ OSM עם אפשרות סינון לפי delegate מותאם
        /// </summary>
        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges)
            LoadOsmData(string filePath, Func<(double lat, double lon), bool> isInBounds)
        {
            var allNodes = new Dictionary<long, (double lat, double lon)>();
            var edges = new List<(long from, long to)>();
            using (var fileStream = File.OpenRead(filePath))
            {
                var source = new PBFOsmStreamSource(fileStream);
                foreach (var element in source)
                {
                    if (element.Type == OsmGeoType.Node)
                    {
                        var node = (Node)element;
                        if (node.Latitude != null && node.Longitude != null)
                        {
                            var coord = ((double)node.Latitude, (double)node.Longitude);
                            if (isInBounds(coord))
                                allNodes[node.Id.Value] = coord;
                        }
                    }
                }
                fileStream.Position = 0;
                var waySource = new PBFOsmStreamSource(fileStream);
                foreach (var element in waySource)
                {
                    if (element.Type == OsmGeoType.Way)
                    {
                        var way = (Way)element;
                        if (way.Tags != null &&
                            way.Tags.TryGetValue("highway", out string highwayValue) &&
                            allowedHighwayTypes.Contains(highwayValue) &&
                            way.Nodes != null && way.Nodes.Length > 1)
                        {
                            for (int i = 0; i < way.Nodes.Length - 1; i++)
                            {
                                var from = way.Nodes[i];
                                var to = way.Nodes[i + 1];
                                if (allNodes.ContainsKey(from) && allNodes.ContainsKey(to))
                                    edges.Add((from, to));
                            }
                        }
                    }
                }
            }
            return (allNodes, edges);
        }

        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges)
            LoadOsmData(
                string filePath,
                double? minLat = null, double? maxLat = null,
                double? minLon = null, double? maxLon = null)
        {
            return LoadOsmData(filePath, coord =>
            {
                double lat = coord.lat;
                double lon = coord.lon;

                if (minLat.HasValue && lat < minLat.Value) return false;
                if (maxLat.HasValue && lat > maxLat.Value) return false;
                if (minLon.HasValue && lon < minLon.Value) return false;
                if (maxLon.HasValue && lon > maxLon.Value) return false;

                return true;
            });
        }
    }
}

