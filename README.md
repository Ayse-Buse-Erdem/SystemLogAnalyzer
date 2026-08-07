# System Log Analyzer

System Log Analyzer is a C# console application developed to parse, validate, analyze, filter, and export system log records.

## Features

- Reads multiple `.log` and `.txt` files
- Parses structured system log records
- Separates valid and invalid records
- Filters logs by severity level
- Searches records by keyword
- Filters records by date range
- Generates an analysis summary
- Exports results as TXT and CSV files

## Technologies

- C#
- .NET Framework 4.8
- Visual Studio
- File handling
- Object-oriented programming

## Project Structure

- `Data/` – Sample system log files
- `Models/` – Log record data model
- `Services/` – Parsing, analysis, and report generation services
- `Output/` – Generated analysis, invalid-record, and CSV reports
- `Program.cs` – Console menu and application entry point

## Generated Outputs

The application creates the following files:

- `analysis_report.txt`
- `invalid_logs.txt`
- `log_records.csv`

## How to Run

1. Clone or download the repository.
2. Open `SystemLogAnalyzer.sln` in Visual Studio.
3. Build the solution.
4. Run the project with `Ctrl + F5`.
5. Select an operation from the console menu.

## Purpose

This project was developed to practice file processing, data validation, log analysis, reporting, and object-oriented programming with C#.
