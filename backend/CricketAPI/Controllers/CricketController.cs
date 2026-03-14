using CricketAPI.Models;
using CricketAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CricketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CricketController : ControllerBase
    {
        private readonly ICricketService _cricketService;
        private readonly ILogger<CricketController> _logger;

        public CricketController(ICricketService cricketService, ILogger<CricketController> logger)
        {
            _cricketService = cricketService;
            _logger = logger;
        }

        [HttpGet("matches")]
        public async Task<ActionResult<MatchListResponse>> GetLiveMatches()
        {
            var result = await _cricketService.GetLiveMatchesAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("matches/{id}")]
        public async Task<ActionResult<MatchDetailResponse>> GetMatchDetail(string id)
        {
            var result = await _cricketService.GetMatchDetailAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }
    }
}