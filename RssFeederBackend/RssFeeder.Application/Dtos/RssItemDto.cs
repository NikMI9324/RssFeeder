using System;
using System.Collections.Generic;

namespace RssFeeder.Application.Dtos
{
    public class RssItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string HtmlDescription { get; set; } = string.Empty;
        public string PlainDescription { get; set; } = string.Empty;
        public DateTimeOffset? PublishedAt { get; set; }
        public string? PublishedAtFormatted { get; set; }
        public string Author { get; set; } = string.Empty;
        public List<string> Categories { get; set; } = new();
    }
}
