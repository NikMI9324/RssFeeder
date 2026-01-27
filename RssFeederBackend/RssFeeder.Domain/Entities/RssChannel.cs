using System;
using System.Collections.Generic;
using System.Net.ServerSentEvents;
using System.Text;

namespace RssFeeder.Domain.Entities
{
    public class RssChannel
    {
        public string Title { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<RssItem> Items { get; set; } = new();
    }
}
