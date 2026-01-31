using RssFeeder.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace RssFeeder.Application.Interfaces
{
    public interface IFeedService
    {
        Task<IEnumerable<FeedDto>> GetAllFeedsAsync();
        Task<FeedDto> GetFeedByIdAsync(int id);
        Task AddFeedAsync(FeedDto feed);
        Task UpdateFeedAsync(int id, FeedDto feed);
        Task DeleteFeedByIdAsync(int id);
        Task UpdateRefreshTimeAsync(int refreshTimeSeconds);
        Task ToggleFeedEnabledAsync(string name, bool enabled);
    }
}
