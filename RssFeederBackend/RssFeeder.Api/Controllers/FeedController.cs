using Microsoft.AspNetCore.Mvc;
using RssFeeder.Application.Dtos;
using RssFeeder.Application.Interfaces;

namespace RssFeeder.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedsController : ControllerBase
    {
        private readonly IFeedService _feedService;

        public FeedsController(IFeedService feedService)
        {
            _feedService = feedService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var feeds = await _feedService.GetAllFeedsAsync();
            return Ok(feeds);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var feed = await _feedService.GetFeedByIdAsync(id);
            return Ok(feed);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FeedDto feed)
        {
            await _feedService.AddFeedAsync(feed);
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] FeedDto feed)
        {
            await _feedService.UpdateFeedAsync(id, feed);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _feedService.DeleteFeedByIdAsync(id);
            return NoContent();
        }

        [HttpPost("{name}/toggle")]
        public async Task<IActionResult> Toggle(string name, [FromQuery] bool enabled)
        {
            await _feedService.ToggleFeedEnabledAsync(name, enabled);
            return NoContent();
        }

        [HttpPut("refreshTime/{seconds:int}")]
        public async Task<IActionResult> UpdateRefreshTime(int seconds)
        {
            await _feedService.UpdateRefreshTimeAsync(seconds);
            return NoContent();
        }
    }
}
