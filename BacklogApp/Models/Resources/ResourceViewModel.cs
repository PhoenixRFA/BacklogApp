namespace BacklogApp.Models.Resources
{
    public record ResourceViewModel
    {
        public string FileName { get; set; } = default!;
        public string MimeType { get; set; } = default!;
        public Stream FileStream { get; set; } = default!;
    }
}
