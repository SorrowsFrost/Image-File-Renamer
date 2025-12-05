using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

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
    }
}