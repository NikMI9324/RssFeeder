using Microsoft.AspNetCore.Mvc;
using RssFeeder.Application.Services;
using RssFeeder.Domain.Interfaces;

    namespace RssFeeder.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RssController : ControllerBase
    {
        private readonly RssSyndicationService _syndicationService;
        private readonly IResponseCaching _responseCaching;

        public RssController(RssSyndicationService syndicationService, IResponseCaching responseCaching)
        {
            _syndicationService = syndicationService;
            _responseCaching = responseCaching;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEnabled()
        {
            var channels = await _syndicationService.LoadAllEnabledFeedsAsync();
            return Ok(channels);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var channel = await _syndicationService.LoadFeedByNameAsync(name);
            if (channel == null) return NotFound();
            return Ok(channel);
        }

        [HttpGet("redis/{name}/raw")]
        public async Task<IActionResult> GetRawFromCache(string name)
        {
            var xml = await _responseCaching.GetFeedXmlAsync(name);
            if (string.IsNullOrWhiteSpace(xml)) return NotFound();
            return Content(xml, "application/rss+xml");
        }

        [HttpPost("redis/{name}/refresh")]
        public async Task<IActionResult> ForceRefresh(string name)
        {
            var xml = await _responseCaching.ForceRefreshAsync(name);
            if (string.IsNullOrWhiteSpace(xml)) return NotFound();
            return Content(xml, "application/rss+xml");
        }
    }
}
