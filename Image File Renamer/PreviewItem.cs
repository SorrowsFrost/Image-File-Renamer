namespace PhotoOrganizer
{
    public class PreviewItem
    {
        public string Original { get; set; }
        public string New { get; set; }      // the new filename
        public string Status { get; set; }   // "Skipped", "Overwritten", "Appended", "Renamed"
    }
}