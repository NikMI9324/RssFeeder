using RssFeeder.Application.Dtos;
using RssFeeder.Application.Interfaces;
using RssFeeder.Domain.Entities;
using RssFeeder.Domain.Interfaces;

namespace RssFeeder.Application.Services
{
    public class FeedService : IFeedService
    {
        private readonly IFeedRepository _feedRepo;
        public FeedService(IFeedRepository feedRepo)
        {
            _feedRepo = feedRepo;
        }
        private static FeedDto Map(Feed feed)
        {
            return new FeedDto
            {
                Name = feed.Name,
                Enabled = feed.Enabled,
                Link = feed.Link
            };
        }
        public async Task AddFeedAsync(FeedDto feedDto)
        {
            var container = await _feedRepo.GetFeedsAsync();
            if (container.Feeds.Any(f => f.Name.Equals(feedDto.Name)))
                throw new InvalidOperationException($"Feed с именем '{feedDto.Name}' существует");
            int nextId = container.Feeds.Any() ? container.Feeds.Max(f => f.Id) + 1 : 1;
            var feed = new Feed
            {
                Id = nextId,
                Name = feedDto.Name,
                Link = feedDto.Link,
                Enabled = feedDto.Enabled   
            };
            await _feedRepo.AddFeedAsync(feed);
        }

        public Task DeleteFeedByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<FeedDto>> GetAllFeedsAsync()
        {
            var container = await _feedRepo.GetFeedsAsync();
            return container.Feeds.Select(Map).ToList();
        }

        public async Task<FeedDto> GetFeedByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var feed = await _feedRepo.GetFeedByIdAsync(id);
            if (feed == null) throw new KeyNotFoundException($"Feed id={id} not found");
            return Map(feed);
        }

        public async Task ToggleFeedEnabledAsync(string name, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name required", nameof(name));
            var feed = await _feedRepo.GetFeedByNameAsync(name);
            if (feed == null) throw new KeyNotFoundException($"Feed '{name}' not found");
            feed.Enabled = enabled;
            await _feedRepo.UpdateFeedAsync(feed, feed.Name);
        }

        public async Task UpdateFeedAsync(int id, FeedDto feedDto)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var existing = await _feedRepo.GetFeedByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Feed id={id} not found");

            if (!existing.Name.Equals(feedDto.Name))
            {
                var container = await _feedRepo.GetFeedsAsync();
                if (container.Feeds.Any(f => f.Name.Equals(feedDto.Name)))
                    throw new InvalidOperationException($"Feed с именем '{feedDto.Name}' существует");
            }

            var updated = new Feed
            {
                Id = existing.Id,
                Name = feedDto.Name,
                Link = feedDto.Link,
                Enabled = feedDto.Enabled
            };

            await _feedRepo.UpdateFeedAsync(updated, existing.Name);
        }

        public async Task UpdateRefreshTimeAsync(int refreshTimeSeconds)
        {
            if(refreshTimeSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(refreshTimeSeconds),
                    "RefreshTimeInSeconds должен быть положительным");
            await _feedRepo.UpdateRefreshTimeAsync(refreshTimeSeconds);
        }
    }
}
