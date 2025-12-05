using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

// Alias System.IO for clarity
using IO = System.IO;

namespace PhotoOrganizer
{
    public partial class MainWindow : Window
    {
        private string sourceFolder = string.Empty;
        private string targetFolder = string.Empty;
        private readonly ExifLogic exifLogic = new ExifLogic();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded; // safe place to initialize preview
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void SourceButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                sourceFolder = dialog.FileName;
                SourcePathText.Text = $"Source: {sourceFolder}";
                StatusText.Text = "Source folder selected.";
            }
        }

        private void TargetButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                targetFolder = dialog.FileName;
                TargetPathText.Text = $"Target: {targetFolder}";
                StatusText.Text = "Target folder selected.";
            }
        }

        private async void OrganizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(sourceFolder) || string.IsNullOrEmpty(targetFolder))
            {
                StatusText.Text = "Please select both source and target folders.";
                return;
            }

            int duplicateMode = 0;
            if (SkipDuplicateOption.IsChecked == true) duplicateMode = 1;
            else if (OverwriteOption.IsChecked == true) duplicateMode = 2;

            try
            {
                StatusText.Text = "Processing...";
                ProgressBar.Value = 0;
                ProgressText.Text = "Progress: 0/0";

                await Task.Run(() =>
                {
                    exifLogic.RenameAndOrganizeImages(
                        sourceFolder,
                        targetFolder,
                        duplicateMode,
                        (processed, total) =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                ProgressBar.Value = (double)processed / total * 100;
                                ProgressText.Text = $"Progress: {processed}/{total}";
                            });
                        });
                });

                StatusText.Text = "Photos organized successfully!";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
        }

        private void ShowRulesButton_Click(object sender, RoutedEventArgs e)
        {
            string rules =
@"File Renaming Rules:
- Metadata priority: EXIF DateTimeOriginal → QuickTime/MP4/HEIC creation tags → LastWriteTime → CreationTime
- Fallback: '_noexif' suffix if metadata unavailable
- Folder structure: Year/Month
- Duplicate handling: Append counter, Skip, or Overwrite (based on selection)
- Filename format: yyyyMMdd_HHmmss[_noexif][_counter].ext";

            MessageBox.Show(rules, "Current Rules", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdatePreview()
        {
            if (PreviewText == null) return; // safety guard

            string sampleExtension = ".jpg";
            DateTime sampleDate = DateTime.Now;
            string suffix = "_noexif"; // simulate fallback

            string newFileName = $"{sampleDate:yyyyMMdd_HHmmss}{suffix}{sampleExtension}";

            if (AppendCounterOption?.IsChecked == true)
            {
                PreviewText.Text = $"{newFileName}\n{sampleDate:yyyyMMdd_HHmmss}{suffix}_1{sampleExtension}";
            }
            else if (SkipDuplicateOption?.IsChecked == true)
            {
                PreviewText.Text = $"{newFileName}\n(Duplicate would be skipped)";
            }
            else if (OverwriteOption?.IsChecked == true)
            {
                PreviewText.Text = $"{newFileName}\n(Duplicate would overwrite existing)";
            }
        }

        private void AppendCounterOption_Checked(object sender, RoutedEventArgs e) => UpdatePreview();
        private void SkipDuplicateOption_Checked(object sender, RoutedEventArgs e) => UpdatePreview();
        private void OverwriteOption_Checked(object sender, RoutedEventArgs e) => UpdatePreview();
    }
}