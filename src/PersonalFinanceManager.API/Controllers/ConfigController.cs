using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Application.Interfaces;

namespace PersonalFinanceManager.API.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController(IConfigService configService) : ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> GetConfig([FromQuery] string type)
    {
        var config = configService.GetConfig(type);
        return Ok(config);
    }
}