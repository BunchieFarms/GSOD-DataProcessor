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
                File.AppendAllText("log.txt", _log.ToString());
            }
        }
    }
}
