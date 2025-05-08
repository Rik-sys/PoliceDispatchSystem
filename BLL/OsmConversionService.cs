using System.Diagnostics;

namespace BLL
{
    public static class OsmConversionService
    {
        private static readonly string ConverterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "osmconvert.exe");

        public static string ConvertOsmToPbf(string inputOsmPath)
        {
            if (!File.Exists(ConverterPath))
                throw new FileNotFoundException("osmconvert.exe לא נמצא בנתיב Tools");

            string outputPbfPath = Path.ChangeExtension(inputOsmPath, ".pbf");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ConverterPath,
                    Arguments = $"\"{inputOsmPath}\" -o=\"{outputPbfPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string stdOut = process.StandardOutput.ReadToEnd();
            string stdErr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!File.Exists(outputPbfPath))
                throw new Exception($"המרה נכשלה: {stdErr}");

            return outputPbfPath;
        }
    }
}
