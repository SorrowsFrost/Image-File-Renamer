using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Microsoft.WindowsAPICodePack.Dialogs; // CommonOpenFileDialog
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Windows.Controls;
using System.Windows.Input; // for MouseButtonEventArgs and MouseButtonState
using System.Windows.Shell; // add this at the top

// Alias for brevity
using IO = System.IO;

namespace PhotoOrganizer
{
    public partial class MainWindow : Window
    {
        // Fields
        private string sourceFolder = string.Empty;
        private string targetFolder = string.Empty;
        private readonly ExifLogic exifLogic = new ExifLogic();
        private List<PreviewItem> previewItems = new List<PreviewItem>();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            // Add this instead:
            var chrome = new System.Windows.Shell.WindowChrome
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(6),
                GlassFrameThickness = new Thickness(0),
                UseAeroCaptionButtons = false
            };
            System.Windows.Shell.WindowChrome.SetWindowChrome(this, chrome);

        }

        private void EnableDarkTitleBar()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int useDarkMode = 1;

            // Try attribute 19 first
            int attribute = 19; // DWMWA_USE_IMMERSIVE_DARK_MODE (older builds)
            DwmSetWindowAttribute(hwnd, attribute, ref useDarkMode, sizeof(int));

            // Try attribute 20 as fallback
            attribute = 20; // DWMWA_USE_IMMERSIVE_DARK_MODE (newer builds)
            DwmSetWindowAttribute(hwnd, attribute, ref useDarkMode, sizeof(int));
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void AppendCounterOption_Checked(object sender, RoutedEventArgs e)
        {
            if (StatusText != null)
                StatusText.Text = "Duplicate mode: Append Counter";
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Maximized)
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void SkipDuplicateOption_Checked(object sender, RoutedEventArgs e)
        {
            if (StatusText != null)
                StatusText.Text = "Duplicate mode: Skip Duplicate";
        }

        private void OverwriteOption_Checked(object sender, RoutedEventArgs e)
        {
            if (StatusText != null)
                StatusText.Text = "Duplicate mode: Overwrite Existing";
        }

        private void NamingConventionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NamingConventionCombo.SelectedItem is ComboBoxItem item)
            {
                if (item.Content?.ToString() == "Custom")
                {
                    if (CustomFormatPanel != null) CustomFormatPanel.Visibility = Visibility.Visible;
                    if (CustomHintText != null) CustomHintText.Visibility = Visibility.Visible;
                }
                else
                {
                    if (CustomFormatPanel != null) CustomFormatPanel.Visibility = Visibility.Collapsed;
                    if (CustomHintText != null) CustomHintText.Visibility = Visibility.Collapsed;
                    if (PreviewText != null) PreviewText.Text = $"Format: {item.Content}";
                }
            }
        }

        private void CustomFormatTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            PreviewText.Text = $"Custom format: {CustomFormatTextBox.Text}";
        }

        private void ShowRulesButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Duplicate Handling Rules:\n\n" +
                "- Append Counter: Adds a number if a duplicate filename exists.\n" +
                "- Skip Duplicate: Skips files with duplicate names.\n" +
                "- Overwrite Existing: Replaces files with duplicate names.",
                "Rules",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void UpdatePreview()
        {
            ProgressBar.Value = 0;
            ProgressText.Text = "Ready";
            StatusText.Text = "Select source and target folders to begin.";
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

        private async void PreviewAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(sourceFolder) || string.IsNullOrEmpty(targetFolder))
            {
                StatusText.Text = "Please select both source and target folders.";
                return;
            }

            // Duplicate mode: 0=Append, 1=Skip, 2=Overwrite
            int duplicateMode = 0;
            if (SkipDuplicateOption.IsChecked == true) duplicateMode = 1;
            else if (OverwriteOption.IsChecked == true) duplicateMode = 2;

            // ✅ Naming convention logic
            string convention;
            if (NamingConventionCombo.SelectedItem is ComboBoxItem item && item.Content.ToString() == "Custom")
            {
                convention = string.IsNullOrWhiteSpace(CustomFormatTextBox.Text)
                    ? "yyyyMMdd_HHmmss"
                    : CustomFormatTextBox.Text;
            }
            else
            {
                convention = (NamingConventionCombo.SelectedItem as ComboBoxItem)?.Content.ToString() ??
                             "yyyyMMdd_HHmmss";
            }

            StatusText.Text = "Generating preview...";
            previewItems = new List<PreviewItem>();

            var files = IO.Directory.GetFiles(sourceFolder, "*.*", IO.SearchOption.AllDirectories);
            var seenNames = new Dictionary<string, int>();

            await Task.Run(() =>
            {
                int processed = 0;
                int total = files.Length;

                foreach (var file in files)
                {
                    try
                    {
                        DateTime dateTaken;
                        string suffix = "";
                        string extension = IO.Path.GetExtension(file).ToLower();

                        // Try EXIF date; fall back to file modified time
                        try
                        {
                            var directories = ImageMetadataReader.ReadMetadata(file);
                            var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

                            if (subIfd != null &&
                                subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime exifDate))
                            {
                                dateTaken = exifDate;
                            }
                            else
                            {
                                dateTaken = IO.File.GetLastWriteTime(file);
                                suffix = "_noexif";
                            }
                        }
                        catch
                        {
                            dateTaken = IO.File.GetLastWriteTime(file);
                            suffix = "_noexif";
                        }

                        string newFileName = exifLogic.ApplyNamingConvention(dateTaken, suffix, extension, convention);

                        if (seenNames.ContainsKey(newFileName))
                        {
                            switch (duplicateMode)
                            {
                                case 0: // Append counter
                                    int counter = ++seenNames[newFileName];
                                    int idx = newFileName.LastIndexOf(extension, StringComparison.OrdinalIgnoreCase);
                                    if (idx > 0) newFileName = newFileName.Insert(idx, $"_{counter}");

                                    previewItems.Add(new PreviewItem
                                    {
                                        Original = IO.Path.GetFileName(file),
                                        New = newFileName,
                                        Status = "Appended"
                                    });
                                    break;

                                case 1: // Skip
                                    previewItems.Add(new PreviewItem
                                    {
                                        Original = IO.Path.GetFileName(file),
                                        New = "Skipped",
                                        Status = "Skipped"
                                    });
                                    break;

                                case 2: // Overwrite
                                    previewItems.Add(new PreviewItem
                                    {
                                        Original = IO.Path.GetFileName(file),
                                        New = "Overwritten",
                                        Status = "Overwritten"
                                    });
                                    break;
                            }
                        }
                        else
                        {
                            seenNames[newFileName] = 0;

                            previewItems.Add(new PreviewItem
                            {
                                Original = IO.Path.GetFileName(file),
                                New = newFileName,
                                Status = "Renamed"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        previewItems.Add(new PreviewItem
                        {
                            Original = IO.Path.GetFileName(file),
                            New = $"ERROR: {ex.Message}",
                            Status = "Error"
                        });
                    }

                    processed++;
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar.Value = total == 0 ? 0 : (double)processed / total * 100;
                        ProgressText.Text = $"Preview: {processed}/{total}";
                    });
                }
            });

            // Show preview window
            var previewWindow = new PreviewWindow();
            previewWindow.LoadPreview(previewItems);
            previewWindow.ShowDialog();

            StatusText.Text = "Preview complete.";
        }

        private async void OrganizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(sourceFolder) || string.IsNullOrEmpty(targetFolder))
            {
                StatusText.Text = "Please select both source and target folders.";
                return;
            }

            // Duplicate mode: 0=Append, 1=Skip, 2=Overwrite
            int duplicateMode = 0;
            if (SkipDuplicateOption.IsChecked == true) duplicateMode = 1;
            else if (OverwriteOption.IsChecked == true) duplicateMode = 2;

            string duplicateModeLabel = duplicateMode switch
            {
                0 => "Append",
                1 => "Skip",
                2 => "Overwrite",
                _ => "Append"
            };

            // ✅ Naming convention logic
            string convention;
            if (NamingConventionCombo.SelectedItem is ComboBoxItem item && item.Content.ToString() == "Custom")
            {
                convention = string.IsNullOrWhiteSpace(CustomFormatTextBox.Text)
                    ? "yyyyMMdd_HHmmss"
                    : CustomFormatTextBox.Text;
            }
            else
            {
                convention = (NamingConventionCombo.SelectedItem as ComboBoxItem)?.Content.ToString() ??
                             "yyyyMMdd_HHmmss";
            }

            StatusText.Text = "Organizing photos...";
            previewItems = new List<PreviewItem>();

            await Task.Run(() =>
            {
                previewItems = exifLogic.RenameAndOrganizeImages(
                    sourceFolder,
                    targetFolder,
                    duplicateMode,
                    convention,
                    (processed, total) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ProgressBar.Value = total == 0 ? 0 : (double)processed / total * 100;
                            ProgressText.Text = $"Organize: {processed}/{total}";
                        });
                    });
            });

            // Write audit log
            try
            {
                AuditLogger.WriteLog(targetFolder, sourceFolder, duplicateModeLabel, previewItems);
                StatusText.Text = "Organization complete. Audit log created.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Organization complete. Audit log failed: {ex.Message}";
            }
        }
    }
}