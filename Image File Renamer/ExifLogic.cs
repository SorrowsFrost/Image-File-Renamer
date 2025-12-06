using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

// Alias System.IO for clarity
using IO = System.IO;

namespace PhotoOrganizer
{
    public class ExifLogic
    {
        public List<PreviewItem> RenameAndOrganizeImages(
            string sourceFolder,
            string targetFolder,
            int duplicateMode,
            string namingConvention,
            Action<int, int> progressCallback)
        {
            var files = IO.Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories).ToList();
            var previewItems = new List<PreviewItem>();

            int total = files.Count;
            int processed = 0;

            foreach (var file in files)
            {
                try
                {
                    DateTime dateTaken;
                    string suffix = "";
                    string extension = IO.Path.GetExtension(file).ToLower();

                    try
                    {
                        var directories = ImageMetadataReader.ReadMetadata(file);
                        var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

                        if (subIfdDirectory != null &&
                            subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime exifDate))
                        {
                            dateTaken = exifDate;
                        }
                        else
                        {
                            var quickTimeDir = directories.FirstOrDefault(d =>
                                d.Name.Contains("QuickTime") || d.Name.Contains("MP4") || d.Name.Contains("HEIC"));

                            if (quickTimeDir != null && quickTimeDir.Tags.Any(t => t.Name.Contains("Created")))
                            {
                                var tag = quickTimeDir.Tags.FirstOrDefault(t => t.Name.Contains("Created"));
                                if (DateTime.TryParse(tag?.Description, out DateTime videoDate))
                                {
                                    dateTaken = videoDate;
                                }
                                else
                                {
                                    dateTaken = FallbackTimestamp(file);
                                    suffix = "_noexif";
                                }
                            }
                            else
                            {
                                dateTaken = FallbackTimestamp(file);
                                suffix = "_noexif";
                            }
                        }
                    }
                    catch
                    {
                        dateTaken = FallbackTimestamp(file);
                        suffix = "_noexif";
                    }
                                        // Apply naming convention (preset or custom)
                    string newFileName = ApplyNamingConvention(dateTaken, suffix, extension, namingConvention);
                    string yearFolder = IO.Path.Combine(targetFolder, dateTaken.Year.ToString());
                    string monthFolder = IO.Path.Combine(yearFolder, dateTaken.Month.ToString("D2"));
                    IO.Directory.CreateDirectory(monthFolder);

                    string newFilePath = IO.Path.Combine(monthFolder, newFileName);

                    // ✅ Create a PreviewItem for this file
                    var item = new PreviewItem
                    {
                        Original = IO.Path.GetFileName(file),
                        New = newFileName,
                        Status = "Renamed" // default
                    };

                    if (IO.File.Exists(newFilePath))
                    {
                        switch (duplicateMode)
                        {
                            case 0: // Append counter
                                int counter = 1;
                                while (IO.File.Exists(newFilePath))
                                {
                                    newFileName = ApplyNamingConvention(dateTaken, suffix, extension, namingConvention, counter);
                                    newFilePath = IO.Path.Combine(monthFolder, newFileName);
                                    counter++;
                                }
                                item.New = newFileName;
                                item.Status = "Appended"; // ✅ green
                                break;

                            case 1: // Skip duplicate
                                item.Status = "Skipped"; // ✅ grey
                                previewItems.Add(item);
                                continue;

                            case 2: // Overwrite existing
                                IO.File.Delete(newFilePath);
                                item.Status = "Overwritten"; // ✅ red
                                break;
                        }
                    }

                    IO.File.Copy(file, newFilePath, true);
                    previewItems.Add(item);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {file}: {ex.Message}");
                }

                processed++;
                progressCallback(processed, total);
            }

            return previewItems;
        }
                public void OrganizePhotos(string sourceFolder, string targetFolder)
        {
            // TODO: implement the actual file moving/renaming logic
            // For now, just a placeholder so MainWindow compiles
        }

        public string ApplyNamingConvention(DateTime dateTaken, string suffix, string extension, string convention, int counter = 0)
        {
            string result;

            // Detect custom variables
            if (convention.Contains("{year}") || convention.Contains("{month}") || convention.Contains("{day}") ||
                convention.Contains("{hour}") || convention.Contains("{minute}") || convention.Contains("{second}") ||
                convention.Contains("{suffix}") || convention.Contains("{ext}") || convention.Contains("{counter}"))
            {
                result = convention.Replace("{year}", dateTaken.ToString("yyyy"))
                                   .Replace("{month}", dateTaken.ToString("MM"))
                                   .Replace("{day}", dateTaken.ToString("dd"))
                                   .Replace("{hour}", dateTaken.ToString("HH"))
                                   .Replace("{minute}", dateTaken.ToString("mm"))
                                   .Replace("{second}", dateTaken.ToString("ss"))
                                   .Replace("{suffix}", suffix)
                                   .Replace("{ext}", extension);

                if (counter > 0)
                {
                    result = result.Replace("{counter}", counter.ToString());
                    if (!convention.Contains("{counter}"))
                    {
                        int idx = result.LastIndexOf(extension, StringComparison.OrdinalIgnoreCase);
                        if (idx >= 0) result = result.Insert(idx, $"_{counter}");
                    }
                }
                else
                {
                    result = result.Replace("{counter}", string.Empty);
                }
            }
            else
            {
                // Preset formats
                switch (convention)
                {
                    case "yyyy-MM-dd_HH-mm-ss":
                        result = $"{dateTaken:yyyy-MM-dd_HH-mm-ss}{suffix}{extension}";
                        break;
                    case "yyyy_MM_dd_HH_mm_ss":
                        result = $"{dateTaken:yyyy_MM_dd_HH_mm_ss}{suffix}{extension}";
                        break;
                    default: // yyyyMMdd_HHmmss
                        result = $"{dateTaken:yyyyMMdd_HHmmss}{suffix}{extension}";
                        break;
                }

                if (counter > 0)
                {
                    result = result.Replace(extension, $"_{counter}{extension}");
                }
            }

            // Sanitize filename
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                result = result.Replace(c, '_');
            }

            return result;
        }

        private DateTime FallbackTimestamp(string file)
        {
            DateTime modified = IO.File.GetLastWriteTime(file);
            if (modified != DateTime.MinValue && modified.Year >= 1980)
                return modified;

            DateTime created = IO.File.GetCreationTime(file);
            return (created != DateTime.MinValue && created.Year >= 1980) ? created : DateTime.Now;
        }
    }
}