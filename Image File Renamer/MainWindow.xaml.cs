using System.IO;
using System.Windows;
using Microsoft.Win32; // WPF dialog namespace

namespace ImageFileRenamer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = FolderPathBox.Text;
            if (Directory.Exists(folderPath))
            {
                StatusText.Text = $"Renaming files in {folderPath}...";
                // TODO: Add EXIF renaming logic here
            }
            else
            {
                StatusText.Text = "Please select a valid folder.";
            }
        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // WPF doesn't have a built-in folder picker, only file picker
            var dialog = new OpenFileDialog
            {
                Filter = "Folders|*.none", // hacky filter, but usually you'd pick files
                CheckFileExists = false,
                ValidateNames = false
            };

            if (dialog.ShowDialog() == true)
            {
                string folderPath = Path.GetDirectoryName(dialog.FileName);
                FolderPathBox.Text = folderPath;
                StatusText.Text = $"Selected folder: {folderPath}";
            }
        }
    }
}