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
            try
            {
                var result = await _cricketService.GetLiveMatchesAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetLiveMatches endpoint");
                return StatusCode(500, new MatchListResponse 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }
    }
}