using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using RssFeeder.Application.Services;
using RssFeeder.Domain.Entities;
using RssFeeder.Domain.Interfaces;
using Xunit;

namespace RssFeeder.Testing.CachingTests
{
    public class RssFeederCachingTests
    {
        [Fact]
        public async Task GetFeedXmlAsync_ReturnsCachedValue_WhenPresent()
        {
            var cacheMock = new Mock<IDistributedCache>();
            var cachedBytes = Encoding.UTF8.GetBytes("<rss>cached</rss>");
            cacheMock.Setup(c => c.GetAsync("rss:Test", It.IsAny<CancellationToken>())).ReturnsAsync(cachedBytes);

            var httpFactoryMock = new Mock<IHttpClientFactory>();
            var feedRepoMock = new Mock<IFeedRepository>();

            var svc = new ResponseCaching(cacheMock.Object, httpFactoryMock.Object, feedRepoMock.Object);

            var result = await svc.GetFeedXmlAsync("Test");

            Assert.Equal("<rss>cached</rss>", result);
            httpFactoryMock.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetFeedXmlAsync_FetchesAndCaches_WhenNotInCache()
        {
            var cacheMock = new Mock<IDistributedCache>();
            cacheMock.Setup(c => c.GetAsync("rss:NoCache", It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

            var feed = new Feed { Name = "NoCache", Link = "http://example.com/feed" };
            var feedContainer = new FeedContainer { RefreshTimeInSeconds = 60, Feeds = new List<Feed> { feed } };

            var feedRepoMock = new Mock<IFeedRepository>();
            feedRepoMock.Setup(r => r.GetFeedByNameAsync("NoCache")).ReturnsAsync(feed);
            feedRepoMock.Setup(r => r.GetFeedsAsync()).ReturnsAsync(feedContainer);
                
            var handler = new TestMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<rss>fresh</rss>", Encoding.UTF8) });
            var httpClient = new HttpClient(handler);

            var httpFactoryMock = new Mock<IHttpClientFactory>();
            httpFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            cacheMock
                .Setup(c => c.SetAsync(
                    "rss:NoCache",
                    It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "<rss>fresh</rss>"),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var svc = new ResponseCaching(cacheMock.Object, httpFactoryMock.Object, feedRepoMock.Object);

            var result = await svc.GetFeedXmlAsync("NoCache");

            Assert.Equal("<rss>fresh</rss>", result);
            cacheMock.Verify(c => c.SetAsync(
                "rss:NoCache",
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "<rss>fresh</rss>"),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        class TestMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;
            public TestMessageHandler(HttpResponseMessage response) { _response = response; }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }
    }
}
