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
                    string action;
                    if (item.New == "Skipped") action = "[Skipped]";
                    else if (item.New == "Overwritten") action = "[Overwritten]";
                    else if (item.New != null && item.New.Contains("_1")) action = "[Countered]";
                    else action = "[Renamed]";

                    sb.AppendLine($"{item.Original} → {item.New} {action}");
                }

                sb.AppendLine();
                int renamed = items.Count(i => !(i.New == "Skipped" || i.New == "Overwritten"));
                int skipped = items.Count(i => i.New == "Skipped");
                int overwritten = items.Count(i => i.New == "Overwritten");
                int countered = items.Count(i => i.New != null && i.New.Contains("_1"));

                sb.AppendLine($"Summary: Processed {items.Count} | Renamed {renamed} | Skipped {skipped} | Overwritten {overwritten} | Countered {countered}");

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