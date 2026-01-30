using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;



[XmlRoot("Feeds")]
public class FeedsContainer
{
    [XmlElement("RefreshTimeInSeconds")]
    public int RefreshTimeInSeconds { get; set; }

    [XmlElement("Feed")]
    public List<Feed> Feeds { get; set; } = new();
}
public class Feed
{
    public string Name { get; set; }
    public string Link { get; set; }
    public bool Enabled { get; set; }
}

public class Program
{
    public static void AddNewFeedToFile(string filePath, Feed newFeed)
    {
        FeedsContainer container;

        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FeedsContainer));
            container = (FeedsContainer)serializer.Deserialize(fs);
        }

        container.Feeds.Add(newFeed);

        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FeedsContainer));
            serializer.Serialize(fs, container);
        }

        Console.WriteLine($"Добавлен новый Feed: {newFeed.Name}");
    }

    public static async Task Main()
    {
        //using HttpClient client = new HttpClient();
        //var response = await client.GetStringAsync("https://habr.com/rss/interesting/");
        //using StringReader reader = new StringReader(response);
        //using XmlReader xmlReader = XmlReader.Create(reader);
        //SyndicationFeed feed = SyndicationFeed.Load(xmlReader);
        //Console.Write(feed.Title?.Text);
        string filePath = "C:\\Users\\Мирон\\source\\repos\\RssFeeder\\RssFeederBackend\\Testing\\Testing\\feeds.xml";
        Feed newFeed = new Feed()
        { 
            Enabled = false,
            Link = "http://example.com/rss",
            Name = "Example"
        };
        AddNewFeedToFile(filePath, newFeed);


    }
}
