using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SystemLogAnalyzer.Models;
using SystemLogAnalyzer.Services;

namespace SystemLogAnalyzer
{
    internal class Program
    {
        private static List<LogEntry> logEntries;
        private static LogParser logParser;
        private static LogAnalyzer logAnalyzer;
        private static ReportGenerator reportGenerator;
        private static string outputDirectory;

        static void Main(string[] args)
        {
            Console.Title = "System Log Analyzer";

            string baseDirectory =
                AppDomain.CurrentDomain.BaseDirectory;

            string dataDirectory = Path.Combine(
                baseDirectory,
                "Data"
            );

            string projectDirectory = Path.GetFullPath(
                Path.Combine(
                    baseDirectory,
                    "..",
                    ".."
                )
            );

            outputDirectory = Path.Combine(
                projectDirectory,
                "Output"
            );

            try
            {
                logParser = new LogParser();

                logEntries = logParser.ParseDirectory(
                    dataDirectory
                );

                logAnalyzer = new LogAnalyzer(
                    logEntries
                );

                reportGenerator = new ReportGenerator();

                ShowMainMenu();
            }
            catch (DirectoryNotFoundException exception)
            {
                ShowError(exception.Message);

                Console.WriteLine();
                Console.WriteLine(
                    "Expected Data directory:"
                );
                Console.WriteLine(dataDirectory);

                WaitForUser();
            }
            catch (FileNotFoundException exception)
            {
                ShowError(exception.Message);

                Console.WriteLine();
                Console.WriteLine(
                    "Expected Data directory:"
                );
                Console.WriteLine(dataDirectory);

                WaitForUser();
            }
            catch (UnauthorizedAccessException)
            {
                ShowError(
                    "The application does not have permission " +
                    "to access the required files."
                );

                WaitForUser();
            }
            catch (IOException exception)
            {
                ShowError(
                    "A file operation error occurred: " +
                    exception.Message
                );

                WaitForUser();
            }
            catch (Exception exception)
            {
                ShowError(
                    "An unexpected error occurred: " +
                    exception.Message
                );

                WaitForUser();
            }
        }

        private static void ShowMainMenu()
        {
            bool isRunning = true;

            while (isRunning)
            {
                Console.Clear();

                Console.ForegroundColor =
                    ConsoleColor.Cyan;

                Console.WriteLine(
                    "========================================"
                );

                Console.WriteLine(
                    "          SYSTEM LOG ANALYZER"
                );

                Console.WriteLine(
                    "========================================"
                );

                Console.ResetColor();
                Console.WriteLine();

                Console.WriteLine(
                    "Processed log files: " +
                    logParser.ProcessedFileCount
                );

                Console.WriteLine(
                    "Loaded valid records: " +
                    logAnalyzer.GetTotalRecordCount()
                );

                Console.WriteLine(
                    "Invalid records: " +
                    logParser.InvalidLines.Count
                );

                Console.WriteLine();
                Console.WriteLine("1 - Show all records");
                Console.WriteLine("2 - Show information records");
                Console.WriteLine("3 - Show warning records");
                Console.WriteLine("4 - Show error records");
                Console.WriteLine("5 - Search by keyword");
                Console.WriteLine("6 - Filter by date range");
                Console.WriteLine("7 - Show analysis summary");
                Console.WriteLine("8 - Create report files");
                Console.WriteLine("0 - Exit");
                Console.WriteLine();

                Console.Write("Select an option: ");

                string selection = Console.ReadLine();

                switch (selection)
                {
                    case "1":
                        ShowRecords(
                            logEntries,
                            "ALL LOG RECORDS"
                        );
                        break;

                    case "2":
                        ShowRecords(
                            logAnalyzer.GetRecordsByLevel("INFO"),
                            "INFORMATION RECORDS"
                        );
                        break;

                    case "3":
                        ShowRecords(
                            logAnalyzer.GetRecordsByLevel("WARNING"),
                            "WARNING RECORDS"
                        );
                        break;

                    case "4":
                        ShowRecords(
                            logAnalyzer.GetRecordsByLevel("ERROR"),
                            "ERROR RECORDS"
                        );
                        break;

                    case "5":
                        SearchRecords();
                        break;

                    case "6":
                        FilterByDateRange();
                        break;

                    case "7":
                        ShowAnalysisSummary();
                        break;

                    case "8":
                        CreateReportFiles();
                        break;

                    case "0":
                        isRunning = false;
                        break;

                    default:
                        ShowError(
                            "Please enter a valid menu option."
                        );

                        WaitForUser();
                        break;
                }
            }

            Console.Clear();

            Console.WriteLine(
                "System Log Analyzer has been closed."
            );
        }

        private static void ShowRecords(
            List<LogEntry> records,
            string title)
        {
            Console.Clear();
            ShowTitle(title);

            if (records == null ||
                records.Count == 0)
            {
                Console.WriteLine(
                    "No matching records were found."
                );

                WaitForUser();
                return;
            }

            foreach (LogEntry record in records)
            {
                SetColorByLevel(record.Level);
                Console.WriteLine(record);
                Console.ResetColor();
            }

            Console.WriteLine();

            Console.WriteLine(
                "Record count: " + records.Count
            );

            WaitForUser();
        }

        private static void SearchRecords()
        {
            Console.Clear();
            ShowTitle("SEARCH LOG RECORDS");

            Console.Write(
                "Enter a keyword to search: "
            );

            string keyword = Console.ReadLine();

            List<LogEntry> results =
                logAnalyzer.SearchByKeyword(keyword);

            Console.WriteLine();

            if (results.Count == 0)
            {
                Console.WriteLine(
                    "No records matched the keyword."
                );
            }
            else
            {
                foreach (LogEntry record in results)
                {
                    SetColorByLevel(record.Level);
                    Console.WriteLine(record);
                    Console.ResetColor();
                }

                Console.WriteLine();

                Console.WriteLine(
                    "Matching record count: " +
                    results.Count
                );
            }

            WaitForUser();
        }

        private static void FilterByDateRange()
        {
            Console.Clear();
            ShowTitle("FILTER LOGS BY DATE RANGE");

            Console.Write(
                "Enter start date (dd.MM.yyyy): "
            );

            string startDateText = Console.ReadLine();

            Console.Write(
                "Enter end date (dd.MM.yyyy): "
            );

            string endDateText = Console.ReadLine();

            DateTime startDate;
            DateTime endDate;

            bool isStartDateValid =
                DateTime.TryParseExact(
                    startDateText,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out startDate
                );

            bool isEndDateValid =
                DateTime.TryParseExact(
                    endDateText,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out endDate
                );

            if (!isStartDateValid ||
                !isEndDateValid)
            {
                ShowError(
                    "Please enter dates in dd.MM.yyyy format."
                );

                WaitForUser();
                return;
            }

            if (endDate < startDate)
            {
                ShowError(
                    "The end date cannot be earlier " +
                    "than the start date."
                );

                WaitForUser();
                return;
            }

            List<LogEntry> results =
                logAnalyzer.GetRecordsByDateRange(
                    startDate,
                    endDate
                );

            ShowRecords(
                results,
                "LOG RECORDS BETWEEN " +
                startDate.ToString("dd.MM.yyyy") +
                " AND " +
                endDate.ToString("dd.MM.yyyy")
            );
        }

        private static void ShowAnalysisSummary()
        {
            Console.Clear();
            ShowTitle("LOG ANALYSIS SUMMARY");

            Console.WriteLine(
                "Processed Log Files : " +
                logParser.ProcessedFileCount
            );

            Console.WriteLine(
                "Total Valid Records : " +
                logAnalyzer.GetTotalRecordCount()
            );

            Console.WriteLine(
                "Information Records : " +
                logAnalyzer.GetInformationCount()
            );

            Console.WriteLine(
                "Warning Records     : " +
                logAnalyzer.GetWarningCount()
            );

            Console.WriteLine(
                "Error Records       : " +
                logAnalyzer.GetErrorCount()
            );

            Console.WriteLine(
                "Invalid Records     : " +
                logParser.InvalidLines.Count
            );

            Console.WriteLine(
                "Error Rate          : " +
                logAnalyzer
                    .GetErrorRate()
                    .ToString("F2") +
                "%"
            );

            Console.WriteLine();

            Console.WriteLine(
                "Most Frequent Error : " +
                logAnalyzer.GetMostFrequentError()
            );

            Console.WriteLine(
                "Busiest Error Hour  : " +
                logAnalyzer.GetBusiestErrorHour()
            );

            Console.WriteLine();

            DateTime? firstRecord =
                logAnalyzer.GetFirstRecordTime();

            DateTime? lastRecord =
                logAnalyzer.GetLastRecordTime();

            Console.WriteLine(
                "First Record        : " +
                FormatDate(firstRecord)
            );

            Console.WriteLine(
                "Last Record         : " +
                FormatDate(lastRecord)
            );

            WaitForUser();
        }

        private static void CreateReportFiles()
        {
            try
            {
                string reportPath =
                    reportGenerator.GenerateReport(
                        outputDirectory,
                        logAnalyzer,
                        logParser.InvalidLines.Count
                    );

                string invalidLogsPath =
                    reportGenerator.SaveInvalidLines(
                        outputDirectory,
                        logParser.InvalidLines
                    );

                string csvReportPath =
                    reportGenerator.GenerateCsvReport(
                        outputDirectory,
                        logEntries
                    );

                Console.Clear();
                ShowTitle("REPORT FILES CREATED");

                Console.ForegroundColor =
                    ConsoleColor.Green;

                Console.WriteLine(
                    "The report files were created successfully."
                );

                Console.ResetColor();
                Console.WriteLine();

                Console.WriteLine("Analysis report:");
                Console.WriteLine(reportPath);
                Console.WriteLine();

                Console.WriteLine("Invalid log records:");
                Console.WriteLine(invalidLogsPath);
                Console.WriteLine();

                Console.WriteLine("CSV log records:");
                Console.WriteLine(csvReportPath);
            }
            catch (Exception exception)
            {
                ShowError(
                    "The report files could not be created: " +
                    exception.Message
                );
            }

            WaitForUser();
        }

        private static void ShowTitle(string title)
        {
            Console.ForegroundColor =
                ConsoleColor.Cyan;

            Console.WriteLine(title);

            Console.WriteLine(
                new string('=', title.Length)
            );

            Console.ResetColor();
            Console.WriteLine();
        }

        private static void SetColorByLevel(
            string level)
        {
            switch (level)
            {
                case "INFO":
                    Console.ForegroundColor =
                        ConsoleColor.Green;
                    break;

                case "WARNING":
                    Console.ForegroundColor =
                        ConsoleColor.Yellow;
                    break;

                case "ERROR":
                    Console.ForegroundColor =
                        ConsoleColor.Red;
                    break;

                default:
                    Console.ResetColor();
                    break;
            }
        }

        private static string FormatDate(
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

        private static void ShowError(
            string message)
        {
            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine();
            Console.WriteLine("ERROR: " + message);

            Console.ResetColor();
        }

        private static void WaitForUser()
        {
            Console.WriteLine();

            Console.WriteLine(
                "Press any key to continue..."
            );

            Console.ReadKey();
        }
    }
}