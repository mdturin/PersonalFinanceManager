using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Application.DTOs.Dashboard;
using PersonalFinanceManager.Application.Interfaces;
using System.Security.Claims;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController(IDashboardService DashboardService) : ControllerBase
{
    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        DashboardSummaryDto result = null;
        try
        {
            result = await DashboardService
                .GetSummaryAsync(UserId);
        }
        catch(Exception ex)
        {
            throw new Exception("Found error on summary service!");
        }

        return Ok(result);
    }
}
