using System;
using System.Collections.Generic;
using System.Linq;
using SystemLogAnalyzer.Models;

namespace SystemLogAnalyzer.Services
{
    public class LogAnalyzer
    {
        private readonly List<LogEntry> logEntries;

        public LogAnalyzer(List<LogEntry> logEntries)
        {
            this.logEntries = logEntries ?? new List<LogEntry>();
        }

        public int GetTotalRecordCount()
        {
            return logEntries.Count;
        }

        public int GetInformationCount()
        {
            return logEntries.Count(
                log => log.Level == "INFO"
            );
        }

        public int GetWarningCount()
        {
            return logEntries.Count(
                log => log.Level == "WARNING"
            );
        }

        public int GetErrorCount()
        {
            return logEntries.Count(
                log => log.Level == "ERROR"
            );
        }

        public double GetErrorRate()
        {
            if (logEntries.Count == 0)
            {
                return 0;
            }

            return (double)GetErrorCount()
                   / logEntries.Count
                   * 100;
        }

        public string GetMostFrequentError()
        {
            LogEntry mostFrequentError = logEntries
                .Where(log => log.Level == "ERROR")
                .GroupBy(log => log.Message)
                .OrderByDescending(group => group.Count())
                .Select(group => group.First())
                .FirstOrDefault();

            if (mostFrequentError == null)
            {
                return "No error records found.";
            }

            return mostFrequentError.Message;
        }

        public string GetBusiestErrorHour()
        {
            var busiestHour = logEntries
                .Where(log => log.Level == "ERROR")
                .GroupBy(log => log.Timestamp.Hour)
                .OrderByDescending(group => group.Count())
                .Select(group => new
                {
                    Hour = group.Key,
                    Count = group.Count()
                })
                .FirstOrDefault();

            if (busiestHour == null)
            {
                return "No error records found.";
            }

            return string.Format(
                "{0:00}:00 - {0:00}:59 ({1} errors)",
                busiestHour.Hour,
                busiestHour.Count
            );
        }

        public DateTime? GetFirstRecordTime()
        {
            if (logEntries.Count == 0)
            {
                return null;
            }

            return logEntries.Min(log => log.Timestamp);
        }

        public DateTime? GetLastRecordTime()
        {
            if (logEntries.Count == 0)
            {
                return null;
            }

            return logEntries.Max(log => log.Timestamp);
        }

        public List<LogEntry> GetRecordsByLevel(string level)
        {
            return logEntries
                .Where(log =>
                    log.Level.Equals(
                        level,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .OrderBy(log => log.Timestamp)
                .ToList();
        }

        public List<LogEntry> GetRecordsByDateRange(
            DateTime startDate,
            DateTime endDate)
        {
            DateTime start = startDate.Date;

            DateTime end = endDate.Date
                .AddDays(1)
                .AddTicks(-1);

            return logEntries
                .Where(log =>
                    log.Timestamp >= start &&
                    log.Timestamp <= end
                )
                .OrderBy(log => log.Timestamp)
                .ToList();
        }

        public List<LogEntry> SearchByKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<LogEntry>();
            }

            return logEntries
                .Where(log =>
                    log.Message.IndexOf(
                        keyword,
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0
                )
                .OrderBy(log => log.Timestamp)
                .ToList();
        }
    }
}