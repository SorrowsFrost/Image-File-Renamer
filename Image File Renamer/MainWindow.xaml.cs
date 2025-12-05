using System;
using System.Windows;
using Microsoft.Win32; // For Folder Picker
using Microsoft.WindowsAPICodePack.Dialogs; // Requires reference to System.Windows.Forms
using PhotoOrganizer;

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
                StatusText.Text = $"Source: {sourceFolder}";
            }
        }

        private void TargetButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                targetFolder = dialog.FileName;
                StatusText.Text = $"Target: {targetFolder}";
            }
        }



        private void OrganizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(sourceFolder) || string.IsNullOrEmpty(targetFolder))
            {
                StatusText.Text = "Please select both source and target folders.";
                return;
            }

            try
            {
                exifLogic.RenameAndOrganizeImages(sourceFolder, targetFolder);
                StatusText.Text = "Photos organized successfully!";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
        }
    }
}