using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Text.RegularExpressions;

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
            Loaded += MainWindow_Loaded;
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

            string convention = GetSelectedConventionString();

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
                        convention,
                        (processed, total) =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                ProgressBar.Value = total == 0 ? 0 : (double)processed / total * 100;
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
- Filename format: Based on selected naming convention or custom variables";
            MessageBox.Show(rules, "Current Rules", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PreviewAllButton_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for future dry-run mode
            MessageBox.Show("Preview All feature coming soon!", "Preview All", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdatePreview()
        {
            if (PreviewText == null) return;

            string sampleExtension = ".jpg";
            DateTime sampleDate = DateTime.Now;
            string suffix = "_noexif";

            string convention = GetSelectedConventionString();

            // Live validation for custom variables
            if (IsCustomSelected())
            {
                var validVars = new[] { "year", "month", "day", "hour", "minute", "second", "suffix", "ext", "counter" };
                var matches = Regex.Matches(convention, @"{(.*?)}");

                var invalidVars = matches.Cast<Match>()
                                         .Select(m => m.Groups[1].Value)
                                         .Where(v => !validVars.Contains(v))
                                         .Distinct()
                                         .ToList();

                if (invalidVars.Any())
                {
                    InvalidMarker.Visibility = Visibility.Visible;
                    InvalidMarker.ToolTip = $"Invalid variables: {string.Join(", ", invalidVars)}\n" +
                                            $"Valid variables: {string.Join(", ", validVars)}";
                }
                else
                {
                    InvalidMarker.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                InvalidMarker.Visibility = Visibility.Collapsed;
            }

            // Build preview filename
            string baseName;
            if (IsCustomSelected())
            {
                baseName = ApplyCustomTokens(convention, sampleDate, suffix, sampleExtension, counter: 0);
            }
            else
            {
                baseName = ApplyPresetFormat(convention, sampleDate) + suffix + sampleExtension;
            }

            if (AppendCounterOption?.IsChecked == true)
            {
                var withCounter = IsCustomSelected()
                    ? ApplyCustomTokens(convention, sampleDate, suffix, sampleExtension, counter: 1)
                    : baseName.Replace(sampleExtension, $"_1{sampleExtension}");

                PreviewText.Text = $"{baseName}\n{withCounter}";
            }
            else if (SkipDuplicateOption?.IsChecked == true)
            {
                PreviewText.Text = $"{baseName}\n(Duplicate would be skipped)";
            }
            else if (OverwriteOption?.IsChecked == true)
            {
                PreviewText.Text = $"{baseName}\n(Duplicate would overwrite existing)";
            }
        }

        private void AppendCounterOption_Checked(object sender, RoutedEventArgs e) => UpdatePreview();
        private void SkipDuplicateOption_Checked(object sender, RoutedEventArgs e) => UpdatePreview();
        private void OverwriteOption_Checked(object sender, RoutedEventArgs e) => UpdatePreview();

        private void NamingConventionCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CustomFormatPanel == null || CustomHintText == null) return;

            bool isCustom = IsCustomSelected();
            CustomFormatPanel.Visibility = isCustom ? Visibility.Visible : Visibility.Collapsed;
            CustomHintText.Visibility = isCustom ? Visibility.Visible : Visibility.Collapsed;

            UpdatePreview();
        }

        private void CustomFormatTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdatePreview();
        }

        private bool IsCustomSelected()
        {
            if (NamingConventionCombo?.SelectedItem is System.Windows.Controls.ComboBoxItem item)
                return string.Equals(item.Content?.ToString(), "Custom", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        private string GetSelectedConventionString()
        {
            if (IsCustomSelected())
            {
                var custom = CustomFormatTextBox?.Text;
                return string.IsNullOrWhiteSpace(custom) ? "{year}{month}{day}_{hour}{minute}{second}{suffix}{ext}" : custom;
            }

            if (NamingConventionCombo?.SelectedItem is System.Windows.Controls.ComboBoxItem item)
                return item.Content?.ToString() ?? "yyyyMMdd_HHmmss";

            return "yyyyMMdd_HHmmss";
        }

        private string ApplyPresetFormat(string convention, DateTime dt)
        {
            switch (convention)
            {
                case "yyyy-MM-dd_HH-mm-ss":
                    return $"{dt:yyyy-MM-dd_HH-mm-ss}";
                case "yyyy_MM_dd_HH_mm_ss":
                    return $"{dt:yyyy_MM_dd_HH_mm_ss}";
                default:
                    return $"{dt:yyyyMMdd_HHmmss}";
            }
        }

        private string ApplyCustomTokens(string template, DateTime dt, string suffix, string ext, int counter)
        {
            string result = template;

            result = result.Replace("{year}", dt.ToString("yyyy"))
                           .Replace("{month}", dt.ToString("MM"))
                           .Replace("{day}", dt.ToString("dd"))
                           .Replace("{hour}", dt.ToString("HH"))
                           .Replace("{minute}", dt.ToString("mm"))
                           .Replace("{second}", dt.ToString("ss"))
                           .Replace("{suffix}", suffix)
                           .Replace("{ext}", ext);

            if (counter > 0)
            {
                result = result.Replace("{counter}", counter.ToString());
                if (!template.Contains("{counter}"))
                {
                    int idx = result.LastIndexOf(ext, StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0) result = result.Insert(idx, $"_{counter}");
                }
            }
            else
            {
                result = result.Replace("{counter}", string.Empty);
            }

            return result;
        }
    }
}