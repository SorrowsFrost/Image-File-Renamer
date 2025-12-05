using System;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace PhotoOrganizer
{
    public class ExifLogic
    {
        public void RenameAndOrganizeImages(string sourceFolder, string targetFolder)
        {
            var files = System.IO.Directory.GetFiles(sourceFolder, "*.jpg");

            foreach (var file in files)
            {
                try
                {
                    var directories = ImageMetadataReader.ReadMetadata(file);
                    var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

                    if (subIfdDirectory != null && subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime dateTaken))
                    {
                        string extension = System.IO.Path.GetExtension(file);
                        string newFileName = $"{dateTaken:yyyyMMdd_HHmmss}{extension}";

                        string yearFolder = System.IO.Path.Combine(targetFolder, dateTaken.Year.ToString());
                        string monthFolder = System.IO.Path.Combine(yearFolder, dateTaken.Month.ToString("D2"));
                        System.IO.Directory.CreateDirectory(monthFolder);

                        string newFilePath = System.IO.Path.Combine(monthFolder, newFileName);

                        int counter = 1;
                        while (File.Exists(newFilePath))
                        {
                            newFileName = $"{dateTaken:yyyyMMdd_HHmmss}_{counter}{extension}";
                            newFilePath = System.IO.Path.Combine(monthFolder, newFileName);
                            counter++;
                        }

                        File.Copy(file, newFilePath, true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {file}: {ex.Message}");
                }
            }
        }
    }
}