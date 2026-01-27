namespace RssFeeder.Domain.Entities
{
    public class RssItem
    {
        public string Title { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string HtmlDescription { get; set; } = string.Empty;
        public string PlainDescription { get; set; } = string.Empty;
        public DateTimeOffset? PublishedAt { get; set; }
        public string Author { get; set; } = string.Empty;
        public List<string> Categories { get; set; } = new();
    }
}