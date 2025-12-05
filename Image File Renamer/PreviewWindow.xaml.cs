using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PhotoOrganizer
{
    public partial class PreviewWindow : Window
    {
        public PreviewWindow()
        {
            InitializeComponent();
        }

        public void LoadPreview(List<PreviewItem> items)
        {
            PreviewListView.ItemsSource = items;

            int renamed = items.Count(i => !(i.New == "Skipped" || i.New == "Overwritten"));
            int skipped = items.Count(i => i.New == "Skipped");
            int overwritten = items.Count(i => i.New == "Overwritten");
            int countered = items.Count(i => i.New != null && i.New.Contains("_1"));

            SummaryText.Text = $"Processed: {items.Count} | Renamed: {renamed} | " +
                               $"Skipped: {skipped} | Overwritten: {overwritten} | Countered: {countered}";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class PreviewItem
    {
        public string Original { get; set; }
        public string New { get; set; }
    }
}