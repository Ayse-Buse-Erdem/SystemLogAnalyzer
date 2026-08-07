using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SystemLogAnalyzer.Models;

namespace SystemLogAnalyzer.Services
{
    public class LogParser
    {
        public List<string> InvalidLines { get; private set; }

        public int ProcessedFileCount { get; private set; }

        public LogParser()
        {
            InvalidLines = new List<string>();
        }

        public List<LogEntry> ParseDirectory(
            string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException(
                    "The log data directory could not be found: " +
                    directoryPath
                );
            }

            List<string> filePaths = Directory
                .GetFiles(directoryPath, "*.txt")
                .Concat(
                    Directory.GetFiles(
                        directoryPath,
                        "*.log"
                    )
                )
                .Distinct()
                .OrderBy(path => path)
                .ToList();

            if (filePaths.Count == 0)
            {
                throw new FileNotFoundException(
                    "No .txt or .log files were found " +
                    "in the Data directory."
                );
            }

            List<LogEntry> allLogEntries =
                new List<LogEntry>();

            InvalidLines.Clear();
            ProcessedFileCount = 0;

            foreach (string filePath in filePaths)
            {
                List<LogEntry> fileEntries =
                    ParseSingleFile(filePath);

                allLogEntries.AddRange(fileEntries);
                ProcessedFileCount++;
            }

            return allLogEntries
                .OrderBy(log => log.Timestamp)
                .ToList();
        }

        public List<LogEntry> ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The log file could not be found.",
                    filePath
                );
            }

            InvalidLines.Clear();
            ProcessedFileCount = 1;

            return ParseSingleFile(filePath)
                .OrderBy(log => log.Timestamp)
                .ToList();
        }

        private List<LogEntry> ParseSingleFile(
            string filePath)
        {
            List<LogEntry> logEntries =
                new List<LogEntry>();

            string[] lines =
                File.ReadAllLines(filePath);

            string fileName =
                Path.GetFileName(filePath);

            foreach (string line in lines)
            {
                LogEntry logEntry;

                if (TryParseLine(line, out logEntry))
                {
                    logEntries.Add(logEntry);
                }
                else
                {
                    InvalidLines.Add(
                        "[" + fileName + "] " + line
                    );
                }
            }

            return logEntries;
        }

        private bool TryParseLine(
            string line,
            out LogEntry logEntry)
        {
            logEntry = null;

            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            string[] parts = line.Split(
                new[] { ' ' },
                4,
                StringSplitOptions.RemoveEmptyEntries
            );

            if (parts.Length != 4)
            {
                return false;
            }

            string dateTimeText =
                parts[0] + " " + parts[1];

            DateTime timestamp;

            bool isValidDate =
                DateTime.TryParseExact(
                    dateTimeText,
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out timestamp
                );

            if (!isValidDate)
            {
                return false;
            }

            string level = parts[2].ToUpper();

            if (level != "INFO" &&
                level != "WARNING" &&
                level != "ERROR")
            {
                return false;
            }

            string message = parts[3].Trim();

            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            logEntry = new LogEntry
            {
                Timestamp = timestamp,
                Level = level,
                Message = message,
                OriginalLine = line
            };

            return true;
        }
    }
}