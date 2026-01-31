using RssFeeder.Domain.Entities;
using RssFeeder.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace RssFeeder.Infrastructure.Repository
{
    public class FeedRepository : IFeedRepository
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1,1);
        public FeedRepository(string filePath)
        {
            _filePath = filePath;
            EnsureFileExistsAsync().Wait();
            
        }
        private async Task EnsureFileExistsAsync()
        {
            string directory = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if(!File.Exists(_filePath))
            {
                var feedContainer = new FeedContainer
                {
                    RefreshTimeInSeconds = 300,
                    Feeds = new List<Feed>()
                    {
                        new Feed()
                        {
                            Id = 1,
                            Name = "Habr",
                            Enabled = true,
                            Link = "https://habr.com/rss/interesting/"
                        }
                    }
                };
                await SaveContainerAsync(feedContainer);
            }
        }
        private async Task SaveContainerAsync(FeedContainer container)
        {
            using (FileStream fs = new FileStream(_filePath, FileMode.Create, 
                FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(FeedContainer));
                serializer.Serialize(fs, container);
                await fs.FlushAsync();
            }
        }
        public async Task AddFeedAsync(Feed feed)
        {
            await _fileLock.WaitAsync();
            try
            {
                var feedContainer = await LoadFeedsAsync();
                if (feedContainer.Feeds.Any(f => f.Name.Equals(feed.Name, StringComparison.OrdinalIgnoreCase)))
                    throw new Exception($"Feed с именем {feed.Name} уже существует");
                feedContainer.Feeds.Add(feed);
                await SaveContainerAsync(feedContainer);

            }
            finally
            {
                _fileLock.Release();
            }

        }

        public Task DeleteFeedByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Feed> GetFeedByIdAsync(int id)
        {
            await _fileLock.WaitAsync();
            try
            {
                var feedContainer = await LoadFeedsAsync();
                return feedContainer.Feeds.FirstOrDefault(f => f.Id == id);
            }
            finally
            {
                _fileLock.Release(); 
            }
        }

        public async Task<FeedContainer> GetFeedsAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                using FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read,
                    FileShare.Read, 4096, FileOptions.Asynchronous);
                XmlSerializer serializer = new XmlSerializer(typeof(FeedContainer));
                return (FeedContainer)serializer.Deserialize(fs);
            }
            finally
            {
                _fileLock.Release();
            }
        }
        private async Task<FeedContainer> LoadFeedsAsync()
        {
            using FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read,
                FileShare.Read, 4096, FileOptions.Asynchronous);
            XmlSerializer serializer = new XmlSerializer(typeof(FeedContainer));
            return (FeedContainer)serializer.Deserialize(fs);
        }
        public async Task UpdateFeedAsync(Feed feed, string name)
        {
            await _fileLock.WaitAsync();
            try
            {
                var feedContainer = await LoadFeedsAsync();
                var existingFeed = feedContainer.Feeds.FirstOrDefault(f => f.Name.Equals(name));
                if (existingFeed == null)
                    throw new KeyNotFoundException($"Feed '{name}' не найден");
                existingFeed.Name = feed.Name;
                existingFeed.Link = feed.Link;
                existingFeed.Enabled = feed.Enabled;

                await SaveContainerAsync(feedContainer);

            }
            finally { _fileLock.Release(); }
        }

        public async Task UpdateRefreshTimeAsync(int refreshTime)
        {
            await _fileLock.WaitAsync();
            try
            {
                var feedContainer = await LoadFeedsAsync();
                feedContainer.RefreshTimeInSeconds = refreshTime;
                await SaveContainerAsync(feedContainer);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<Feed> GetFeedByNameAsync(string name)
        {
            await _fileLock.WaitAsync();
            try
            {
                var feedContainer = await LoadFeedsAsync();
                var feed = feedContainer.Feeds.FirstOrDefault(f => f.Name == name);
                if (feed == null)
                    throw new Exception($"Feed с именем {name} не найден");
                return feed;

            }
            finally { _fileLock.Release(); }
        }

        public async Task<int> GetRefreshTimeAsync()
        {
            var feedContainer = await GetFeedsAsync();
            return feedContainer.RefreshTimeInSeconds;
        }
    }
}
