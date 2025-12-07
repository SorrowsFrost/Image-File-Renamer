using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PhotoOrganizer
{
    public static class AuditLogger
    {
        public static void WriteLog(string targetFolder, string sourceFolder, string duplicateMode, List<PreviewItem> items,
            TimeSpan elapsed)
        {
            try
            {
                string logDir = Path.Combine(targetFolder, "Audit_Log");
                Directory.CreateDirectory(logDir);

                string logFile = Path.Combine(logDir, $"Audit_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                var sb = new StringBuilder();

                // Capture start time
                DateTime startTime = DateTime.Now;

                // === Audit Log Header ===
                sb.AppendLine("=== Photo Organizer Audit Log ===");
                sb.AppendLine($"Date | {startTime}");
                sb.AppendLine($"Source | {sourceFolder}");
                sb.AppendLine($"Target | {targetFolder}");
                sb.AppendLine($"Duplicate Mode | {duplicateMode}");
                sb.AppendLine();

                // === Batch Summary ===
                int renamed = items.Count(i => i.Status == "Renamed" || i.Status == "Appended");
                int skipped = items.Count(i => i.Status == "Skipped");
                int overwritten = items.Count(i => i.Status == "Overwritten");
                int appended = items.Count(i => i.Status == "Appended");
                int errors = items.Count(i => i.Status == "Error");

                // Calculate elapsed time
                DateTime endTime = DateTime.Now;
             //   TimeSpan elapsed = endTime - startTime;

                sb.AppendLine("===== Batch Summary =====");
                sb.AppendLine($"Total files processed | {items.Count}");
                sb.AppendLine($"Renamed successfully | {renamed}");
                sb.AppendLine($"Skipped (missing DateTaken) | {skipped}");
                sb.AppendLine($"Overwritten | {overwritten}");
                sb.AppendLine($"Appended | {appended}");
                sb.AppendLine($"Errors | {errors}");
                sb.AppendLine($"Elapsed time | {elapsed:hh\\:mm\\:ss\\:fff}");
                sb.AppendLine("=========================");
                sb.AppendLine();

                // === Column Headers ===
                sb.AppendLine("Timestamp | Level | Action | Details");

                // === Log Entries ===
                foreach (var item in items)
                {
                    string timestamp = DateTime.Now.ToString("HH:mm:ss");
                    string level = "INFO"; // could be adjusted per status
                    string action = item.Status;
                    string details = $"{item.Original} | {item.New}";
                    sb.AppendLine($"{timestamp} | {level} | {action} | {details}");
                }

                File.WriteAllText(logFile, sb.ToString());
            }
            catch (Exception ex)
            {
                // Fails gracefully
                Console.WriteLine($"Audit log error: {ex.Message}");
            }
        }
    }
}