using System;
using System.Collections.Generic;
using System.Text;

namespace RssFeeder.Domain.Entities
{
    public class Feed
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Link {  get; set; }
    }
}
