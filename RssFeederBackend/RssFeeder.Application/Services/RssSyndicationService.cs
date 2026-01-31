using System.Net;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;
using RssFeeder.Application.Dtos;
using RssFeeder.Domain.Interfaces;

namespace RssFeeder.Application.Services
{
    public class RssSyndicationService
    {
        private readonly IResponseCaching _responseCaching;
        private readonly IFeedRepository _feedRepository;

        public RssSyndicationService(IResponseCaching responseCaching, IFeedRepository feedRepository)
        {
            _responseCaching = responseCaching;
            _feedRepository = feedRepository;
        }

        public async Task<RssChannelDto?> LoadFeedByNameAsync(string feedName)
        {
            if (string.IsNullOrWhiteSpace(feedName)) throw new ArgumentException("feedName required", nameof(feedName));

            try
            {
                var xml = await _responseCaching.GetFeedXmlAsync(feedName);
                if (string.IsNullOrWhiteSpace(xml)) return null;

                using var sr = new StringReader(xml);
                using var xr = XmlReader.Create(sr);

                var feed = SyndicationFeed.Load(xr);
                if (feed == null) return null;

                var channel = MapToDto(feed);
                channel.Name = feedName;
                return channel;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IEnumerable<RssChannelDto>> LoadAllEnabledFeedsAsync()
        {
            var container = await _feedRepository.GetFeedsAsync();
            var enabled = container?.Feeds?.Where(f => f.Enabled) ?? Enumerable.Empty<Domain.Entities.Feed>();

            var results = new List<RssChannelDto>();
            foreach (var feed in enabled)
            {
                var channel = await LoadFeedByNameAsync(feed.Name);
                if (channel != null) results.Add(channel);
            }

            return results;
        }

        private static RssChannelDto MapToDto(SyndicationFeed feed)
        {
            var dto = new RssChannelDto
            {
                Title = feed.Title?.Text ?? string.Empty,
                Link = feed.Links.FirstOrDefault()?.Uri?.ToString() ?? feed.Id ?? string.Empty,
                Description = feed.Description?.Text ?? string.Empty,
                Items = feed.Items.Select(MapItem).ToList()
            };
            return dto;
        }

        private static RssItemDto MapItem(SyndicationItem item)
        {
            string htmlDescription = item.Summary?.Text ?? string.Empty;
            string plain = ToPlainText(htmlDescription);

            var link = item.Links.FirstOrDefault()?.Uri?.ToString() ?? item.Id ?? string.Empty;
            var published = item.PublishDate == DateTimeOffset.MinValue ? (DateTimeOffset?)null : item.PublishDate;

            return new RssItemDto
            {
                Title = item.Title?.Text ?? string.Empty,
                Link = link,
                HtmlDescription = WebUtility.HtmlDecode(htmlDescription ?? string.Empty),
                PlainDescription = WebUtility.HtmlDecode(plain ?? string.Empty),
                PublishedAt = published,
                PublishedAtFormatted = published?.ToLocalTime().ToString("f"),
                Author = item.Authors?.FirstOrDefault()?.Name ?? string.Empty,
                Categories = item.Categories?.Select(c => c.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new List<string>()
            };
        }

        private static string ToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            html = Regex.Replace(html, @"<script[\s\S]*?>[\s\S]*?</script>", string.Empty, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<style[\s\S]*?>[\s\S]*?</style>", string.Empty, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<(br|p|div|li|tr|h[1-6])\b[^>]*>", "\n", RegexOptions.IgnoreCase);
            var text = Regex.Replace(html, "<.*?>", string.Empty);
            text = WebUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"\s+\n", "\n");
            text = Regex.Replace(text, @"\n\s+", "\n");
            text = Regex.Replace(text, @"[ \t]{2,}", " ");
            return text.Trim();
        }
    }
}
