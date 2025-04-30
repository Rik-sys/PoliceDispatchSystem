using DTO;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PoliceDispatchSystem.Services
{
    public class GraphToImageConverter
    {
        public static string ConvertGraphToImage(Graph graph)
        {
            // שמירת קובץ DOT זמני
            var dotFilePath = Path.GetTempFileName() + ".dot";
            var imageFilePath = Path.GetTempFileName() + ".png";

            try
            {
                // יצירת קובץ DOT מהגרף
                StringBuilder dotContent = new StringBuilder();
                dotContent.AppendLine("graph G {");

                // הוספת הקוד לקובץ DOT
                foreach (var node in graph.Nodes.Values)
                {
                    dotContent.AppendLine($"    {node.Id} [label=\"{node.Id}\"];");
                    foreach (var edge in node.Edges)
                    {
                        dotContent.AppendLine($"    {node.Id} -- {edge.To.Id} [label=\"{edge.Weight}\"];");
                    }
                }

                dotContent.AppendLine("}");

                // כתיבת תוכן DOT לקובץ
                File.WriteAllText(dotFilePath, dotContent.ToString());

                // הפעלת פקודת dot של Graphviz להמיר DOT ל-PNG
                var process = new Process();
                process.StartInfo.FileName = "dot";
                process.StartInfo.Arguments = $"-Tpng \"{dotFilePath}\" -o \"{imageFilePath}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;

                process.Start();
                process.WaitForExit();

                // בדוק אם התמונה נוצרה
                if (File.Exists(imageFilePath))
                {
                    return imageFilePath; // החזרת המיקום של קובץ התמונה
                }
                else
                {
                    throw new InvalidOperationException("לא הצלחנו להמיר את הגרף לתמונה.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("שגיאה בהמרת הגרף לתמונה", ex);
            }
            finally
            {
                // מחיקת קובץ DOT זמני
                if (File.Exists(dotFilePath))
                {
                    File.Delete(dotFilePath);
                }
            }
        }
    }
}
