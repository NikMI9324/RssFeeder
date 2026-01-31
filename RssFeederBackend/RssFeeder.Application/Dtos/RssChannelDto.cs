using System.Collections.Generic;

namespace RssFeeder.Application.Dtos
{
    public class RssChannelDto
    {
        public string Title { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<RssItemDto> Items { get; set; } = new();
        public string Name { get; set; } = string.Empty; 
    }
}
