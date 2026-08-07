using System;

namespace SystemLogAnalyzer.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }

        public string Level { get; set; }

        public string Message { get; set; }

        public string OriginalLine { get; set; }

        public override string ToString()
        {
            return $"{Timestamp:yyyy-MM-dd HH:mm:ss} {Level,-7} {Message}";
        }
    }
}