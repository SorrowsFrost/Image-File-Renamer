using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;


namespace PhotoOrganizer
{
    public partial class PreviewWindow : Window
    {
        public PreviewWindow()
        {
            InitializeComponent();
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
        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e) => this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        public void LoadPreview(List<PreviewItem> items)
        {
            // Bind items to the list view
            PreviewListView.ItemsSource = items;

            // Summary counts
            int renamed = items.Count(i => i.Status == "Renamed" || i.Status == "Appended");
            int skipped = items.Count(i => i.Status == "Skipped");
            int overwritten = items.Count(i => i.Status == "Overwritten");
            int appended = items.Count(i => i.Status == "Appended");

            SummaryText.Text = $"Processed: {items.Count} | Renamed: {renamed} | " +
                               $"Skipped: {skipped} | Overwritten: {overwritten} | Appended: {appended}";
        }

        //private void Close_Click(object sender, RoutedEventArgs e)
        
            //this.Close();
        }
    }
