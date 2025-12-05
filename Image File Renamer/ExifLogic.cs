using System;
using System.IO;
using IO = System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace PhotoOrganizer
{
    public class ExifLogic
    {
        public void RenameAndOrganizeImages(
            string sourceFolder,
            string targetFolder,
            int duplicateMode,
            Action<int, int> progressCallback)
        {
            var files = IO.Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories).ToList();

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

                        // Try EXIF DateTimeOriginal
                        var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                        if (subIfdDirectory != null &&
                            subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime exifDate))
                        {
                            dateTaken = exifDate;
                        }
                        else
                        {
                            // Try QuickTime/MP4/HEIC creation tags
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

                    string newFileName = $"{dateTaken:yyyyMMdd_HHmmss}{suffix}{extension}";
                    string yearFolder = IO.Path.Combine(targetFolder, dateTaken.Year.ToString());
                    string monthFolder = IO.Path.Combine(yearFolder, dateTaken.Month.ToString("D2"));
                    IO.Directory.CreateDirectory(monthFolder);

                    string newFilePath = IO.Path.Combine(monthFolder, newFileName);

                    if (IO.File.Exists(newFilePath))
                    {
                        switch (duplicateMode)
                        {
                            case 0: // Append counter
                                int counter = 1;
                                while (IO.File.Exists(newFilePath))
                                {
                                    newFileName = $"{dateTaken:yyyyMMdd_HHmmss}{suffix}_{counter}{extension}";
                                    newFilePath = IO.Path.Combine(monthFolder, newFileName);
                                    counter++;
                                }
                                break;

                            case 1: // Skip duplicate
                                continue;

                            case 2: // Overwrite existing
                                IO.File.Delete(newFilePath);
                                break;
                        }
                    }

                    IO.File.Copy(file, newFilePath, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {file}: {ex.Message}");
                }

                processed++;
                progressCallback(processed, total);
            }
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