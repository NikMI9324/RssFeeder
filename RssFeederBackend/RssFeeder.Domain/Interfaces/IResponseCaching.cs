using System;
using System.Collections.Generic;
using System.Text;

namespace RssFeeder.Domain.Interfaces
{
    public interface IResponseCaching
    {
        Task<string> GetFeedXmlAsync(string feedName);
        Task<string> ForceRefreshAsync(string feedName);
        Task<bool> RemoveAsync(string feedName);
    }
}
