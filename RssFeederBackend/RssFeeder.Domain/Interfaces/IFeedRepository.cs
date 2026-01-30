
using RssFeeder.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RssFeeder.Domain.Interfaces
{
    public interface IFeedRepository
    {
        public Task<FeedContainer> GetFeedsAsync();
        public Task<Feed> GetFeedByIdAsync(int id);
        public Task<Feed> GetFeedByNameAsync(string name);
        public Task AddFeedAsync(Feed feed);
        public Task DeleteFeedByIdAsync(int id);
        public Task UpdateFeedAsync(Feed feed, string name);
        public Task UpdateRefreshTimeAsync(int refreshTime);
    }
}
