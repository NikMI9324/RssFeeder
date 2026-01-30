
using RssFeeder.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RssFeeder.Domain.Interfaces
{
    public interface IFeedRepository
    {
        public Task<FeedContainer> GetFeedsAsync();
        public Task<Feed> GetFeedByIdAsync(int id);
        public Task AddFeedAsync(Feed feed);
        public Task DeleteFeedByIdAsync(int id);
        public Task UpdateFeedAsync(Feed feed);
        public Task UpdateRefreshTimeAsync(int refreshTime);
    }
}
