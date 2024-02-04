using System.Text;

namespace GSOD_DataProcessor.Business
{
    public static class Logging
    {
        private static readonly StringBuilder _log = new();
        public static void Log(string method, string message, bool complete = false)
        {
            _log.AppendLine($"{DateTime.Now,-30}{method,-40}{message}");
            if (complete)
            {
                _log.AppendLine();
                if (!Directory.Exists("logs"))
                    Directory.CreateDirectory("logs");
                 File.AppendAllText($"logs/{DateTime.Now.ToShortDateString().ToString().Replace('/','-')}.txt", _log.ToString());
                DeleteOldLogs();
            }
        }

        private static void DeleteOldLogs()
        {
            var allLogs = Directory.EnumerateFiles("logs");
            foreach (var file in allLogs)
            {
                var fileNameDate = Path.GetFileNameWithoutExtension(file);
                if (DateTime.Parse(fileNameDate) < DateTime.Now.AddDays(-7))
                    File.Delete(file);
            }
        }
    }
}
