using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace RssFeeder.Domain.Entities
{
    [XmlRoot("Feeds")]
    public class FeedContainer
    {
        [XmlElement("RefreshTimeInSeconds")]
        public int RefreshTimeInSeconds { get; set; }
        [XmlElement("Feed")]
        public List<Feed> Feeds { get; set; }
    }
}
