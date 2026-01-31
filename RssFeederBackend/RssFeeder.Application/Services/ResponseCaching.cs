using Microsoft.Extensions.Caching.Distributed;
using RssFeeder.Domain.Interfaces;

namespace RssFeeder.Application.Services
{
    public class ResponseCaching : IResponseCaching
    {
        private readonly IDistributedCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IFeedRepository _feedRepository;

        public ResponseCaching(IDistributedCache cache, 
            IHttpClientFactory httpClientFactory, 
            IFeedRepository feedRepository)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _feedRepository = feedRepository;
        }

        private static string BuildKey(string feedName) => $"rss:{feedName}";

        public async Task<string> GetFeedXmlAsync(string feedName)
        {
            if (string.IsNullOrWhiteSpace(feedName))
                throw new ArgumentException("feedName is required", nameof(feedName));

            var key = BuildKey(feedName);

            try
            {
                var cached = await _cache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(cached))
                    return cached;

                return await FetchAndCacheAsync(feedName);
            }
            catch (Exception ex)
            {
                return await FetchAndCacheAsync(feedName);
            }
        }

        public async Task<string> ForceRefreshAsync(string feedName)
        {
            if (string.IsNullOrWhiteSpace(feedName))
                throw new ArgumentException("feedName is required", nameof(feedName));

            return await FetchAndCacheAsync(feedName);
        }

        public async Task<bool> RemoveAsync(string feedName)
        {
            if (string.IsNullOrWhiteSpace(feedName))
                throw new ArgumentException("feedName is required", nameof(feedName));

            var key = BuildKey(feedName);
            try
            {
                await _cache.RemoveAsync(key);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<string> FetchAndCacheAsync(string feedName)
        {
            var feed = await _feedRepository.GetFeedByNameAsync(feedName);
            if (feed == null)
                throw new InvalidOperationException($"Feed '{feedName}' не найден");

            var container = await _feedRepository.GetFeedsAsync();
            var refreshSeconds = Math.Max(30, container?.RefreshTimeInSeconds ?? 300);

            var client = _httpClientFactory.CreateClient();
            string content;
            try
            {
                content = await client.GetStringAsync(feed.Link);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to download RSS feed from {feed.Link}", ex);
            }

            var key = BuildKey(feedName);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(refreshSeconds)
            };

            await _cache.SetStringAsync(key, content, options);

            return content;
        }
    }
}
