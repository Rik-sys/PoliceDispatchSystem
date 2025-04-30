using OsmSharp.Streams;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public static class OsmFileReader
    {
        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges) LoadOsmData(string filePath)
        {
            var nodes = new Dictionary<long, (double lat, double lon)>();
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
                            nodes[node.Id.Value] = ((double)node.Latitude, (double)node.Longitude);
                        }
                    }
                    else if (element.Type == OsmGeoType.Way)
                    {
                        var way = (Way)element;
                        if (way.Nodes != null && way.Nodes.Length > 1)
                        {
                            for (int i = 0; i < way.Nodes.Length - 1; i++)
                            {
                                edges.Add((way.Nodes[i], way.Nodes[i + 1]));
                            }
                        }
                    }
                }
            }

            return (nodes, edges);
        }
    }
}