using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SystemLogAnalyzer.Models;

namespace SystemLogAnalyzer.Services
{
    public class ReportGenerator
    {
        public string GenerateReport(
            string outputDirectory,
            LogAnalyzer analyzer,
            int invalidRecordCount)
        {
            Directory.CreateDirectory(outputDirectory);

            string reportPath = Path.Combine(
                outputDirectory,
                "analysis_report.txt"
            );

            DateTime? firstRecordTime =
                analyzer.GetFirstRecordTime();

            DateTime? lastRecordTime =
                analyzer.GetLastRecordTime();

            List<string> reportLines = new List<string>
            {
                "SYSTEM LOG ANALYSIS REPORT",
                "========================================",
                "Generated: " +
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
                "",
                "RECORD SUMMARY",
                "----------------------------------------",
                "Total Valid Records: " +
                    analyzer.GetTotalRecordCount(),
                "Information Records: " +
                    analyzer.GetInformationCount(),
                "Warning Records: " +
                    analyzer.GetWarningCount(),
                "Error Records: " +
                    analyzer.GetErrorCount(),
                "Invalid Records: " +
                    invalidRecordCount,
                "Error Rate: " +
                    analyzer.GetErrorRate().ToString("F2") + "%",
                "",
                "ERROR ANALYSIS",
                "----------------------------------------",
                "Most Frequent Error: " +
                    analyzer.GetMostFrequentError(),
                "Busiest Error Hour: " +
                    analyzer.GetBusiestErrorHour(),
                "",
                "TIME RANGE",
                "----------------------------------------",
                "First Record: " +
                    FormatNullableDate(firstRecordTime),
                "Last Record: " +
                    FormatNullableDate(lastRecordTime)
            };

            File.WriteAllLines(
                reportPath,
                reportLines
            );

            return reportPath;
        }

        public string SaveInvalidLines(
            string outputDirectory,
            List<string> invalidLines)
        {
            Directory.CreateDirectory(outputDirectory);

            string invalidFilePath = Path.Combine(
                outputDirectory,
                "invalid_logs.txt"
            );

            if (invalidLines == null ||
                invalidLines.Count == 0)
            {
                File.WriteAllText(
                    invalidFilePath,
                    "No invalid log records found."
                );
            }
            else
            {
                File.WriteAllLines(
                    invalidFilePath,
                    invalidLines
                );
            }

            return invalidFilePath;
        }

        public string GenerateCsvReport(
            string outputDirectory,
            List<LogEntry> logEntries)
        {
            Directory.CreateDirectory(outputDirectory);

            string csvFilePath = Path.Combine(
                outputDirectory,
                "log_records.csv"
            );

            List<string> csvLines = new List<string>
            {
                "Timestamp,Level,Message"
            };

            if (logEntries != null)
            {
                foreach (LogEntry logEntry in logEntries)
                {
                    string csvLine =
                        EscapeCsv(
                            logEntry.Timestamp.ToString(
                                "yyyy-MM-dd HH:mm:ss"
                            )
                        ) +
                        "," +
                        EscapeCsv(logEntry.Level) +
                        "," +
                        EscapeCsv(logEntry.Message);

                    csvLines.Add(csvLine);
                }
            }

            File.WriteAllLines(
                csvFilePath,
                csvLines,
                new UTF8Encoding(true)
            );

            return csvFilePath;
        }

        private string EscapeCsv(string value)
        {
            if (value == null)
            {
                return "";
            }

            bool requiresQuotes =
                value.Contains(",") ||
                value.Contains("\"") ||
                value.Contains("\n") ||
                value.Contains("\r");

            string escapedValue =
                value.Replace("\"", "\"\"");

            if (requiresQuotes)
            {
                return "\"" + escapedValue + "\"";
            }

            return escapedValue;
        }

        private string FormatNullableDate(
            DateTime? date)
        {
            if (!date.HasValue)
            {
                return "No record found.";
            }

            return date.Value.ToString(
                "dd.MM.yyyy HH:mm:ss"
            );
        }
    }
}