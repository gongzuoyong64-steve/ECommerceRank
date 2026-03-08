

using ECommerceRanking.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceRanking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CSVController : ControllerBase
{

    private readonly IRedisService _redisService;
    private readonly ILogger<CSVController> _logger;

    public CSVController(IRedisService redisService, ILogger<CSVController> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }


    [HttpPost()]
    [EndpointSummary("上傳 CSV 檔案")]
    public async Task<IActionResult> UploadCSV(IFormFile file)
    {
        if(file == null)
        {
            return StatusCode(500, new { Error = "請提供檔案" });
        }
        try
        {
            var result = await _redisService.UploadCSVAsync(file);
            if (result)
            {
                return Ok(new { Message = "CSV 檔案上傳成功" });
            }
            else
            {
                return BadRequest(new { Error = "CSV 檔案上傳失敗" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CSV");
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }
}