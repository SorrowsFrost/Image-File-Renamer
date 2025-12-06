using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PhotoOrganizer
{
    public static class AuditLogger
    {
        public static void WriteLog(string targetFolder, string sourceFolder, string duplicateMode, List<PreviewItem> items)
        {
            try
            {
                string logDir = Path.Combine(targetFolder, "Audit_Log");
                Directory.CreateDirectory(logDir);

                string logFile = Path.Combine(logDir, $"Audit_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                var sb = new StringBuilder();
                sb.AppendLine("=== Photo Organizer Audit Log ===");
                sb.AppendLine($"Date: {DateTime.Now}");
                sb.AppendLine($"Source: {sourceFolder}");
                sb.AppendLine($"Target: {targetFolder}");
                sb.AppendLine($"Duplicate Mode: {duplicateMode}");
                sb.AppendLine();

                foreach (var item in items)
                {
                    string action = $"[{item.Status}]";
                    sb.AppendLine($"{item.Original} → {item.New} {action}");
                }

                sb.AppendLine();
                int renamed = items.Count(i => i.Status == "Renamed" || i.Status == "Appended");
                int skipped = items.Count(i => i.Status == "Skipped");
                int overwritten = items.Count(i => i.Status == "Overwritten");
                int appended = items.Count(i => i.Status == "Appended");

                sb.AppendLine($"Summary: Processed {items.Count} | Renamed {renamed} | Skipped {skipped} | Overwritten {overwritten} | Appended {appended}");

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