using System;
using System.Collections.Generic;
using System.Text;

namespace RssFeeder.Application.Dtos
{
    public class FeedDto
    {
        //public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public bool Enabled { get; set; }
    }
}
