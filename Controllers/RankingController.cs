using ECommerceRanking.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceRanking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RankingController : ControllerBase
{
    private readonly IRedisService _redisService;
    private readonly ILogger<RankingController> _logger;

    public RankingController(IRedisService redisService, ILogger<RankingController> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    /// <summary>
    /// 增加商品分数
    /// </summary>
    [HttpPost("products/{productId}/score")]
    [EndpointSummary("商品分數增加")]
    public async Task<IActionResult> IncrementScore(string productId, [FromBody] IncrementScoreRequest request)
    {
        try
        {
            var newScore = await _redisService.IncrementProductScoreAsync(productId, request.Score);
            return Ok(new { ProductId = productId, NewScore = newScore });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing score for product {ProductId}", productId);
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    /// <summary>
    /// 獲取排行榜
    /// </summary>
    [HttpGet]
    [EndpointSummary("獲取排行榜")]
    public async Task<IActionResult> GetRanking([FromQuery] int startRank = 0, [FromQuery] int endRank = 9, [FromQuery] string order = "desc")
    {
        try
        {
            var orderEnum = order.ToLower() == "asc" ? Order.Ascending : Order.Descending;
            var ranking = await _redisService.GetRankingAsync(startRank, endRank, orderEnum);
            return Ok(ranking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ranking");
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    /// <summary>
    /// 獲取商品排名
    /// </summary>
    [HttpGet("products/{productId}/rank")]
    [EndpointSummary("獲取商品排名")]
    public async Task<IActionResult> GetProductRank(string productId, [FromQuery] string order = "desc")
    {
        try
        {
            var orderEnum = order.ToLower() == "asc" ? Order.Ascending : Order.Descending;
            var rank = await _redisService.GetProductRankAsync(productId, orderEnum);
            
            if (rank == null)
            {
                return NotFound(new { Error = "Product not found in ranking" });
            }

            return Ok(new { ProductId = productId, Rank = rank });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rank for product {ProductId}", productId);
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    /// <summary>
    /// 獲取商品分数
    /// </summary>
    [HttpGet("products/{productId}/score")]
    [EndpointSummary("獲取商品分數")]
    public async Task<IActionResult> GetProductScore(string productId)
    {
        try
        {
            var score = await _redisService.GetProductScoreAsync(productId);
            
            if (score == null)
            {
                return NotFound(new { Error = "Product not found in ranking" });
            }

            return Ok(new { ProductId = productId, Score = score });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting score for product {ProductId}", productId);
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    /// <summary>
    /// 清空排行榜
    /// </summary>
    [HttpDelete]
    [EndpointSummary("清空排行榜")]
    public async Task<IActionResult> ClearRanking()
    {
        try
        {
            var result = await _redisService.ClearRankingAsync();
            return Ok(new { Success = result, Message = "Ranking cleared" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing ranking");
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }
}

public class IncrementScoreRequest
{
    public double Score { get; set; }
}
