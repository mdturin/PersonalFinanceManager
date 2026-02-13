using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        return Ok(await DashboardService.GetSummaryAsync(UserId));
    }

    [HttpGet("top-expense-categories")]
    public async Task<IActionResult> GetTopExpenseCategories()
    {
        return Ok(await DashboardService.GetTopExpenseCategoriesAsync(UserId));
    }

    [HttpGet("recent-transactions")]
    public async Task<IActionResult> GetRecentTransactionsAsync()
    {
        return Ok(await DashboardService.GetRecentTransactionsAsync(UserId));
    }
}
