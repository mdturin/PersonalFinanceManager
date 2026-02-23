using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Application.DTOs.Alert;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Infrastructure.Data.Context;
using System.Security.Claims;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AlertsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAlerts()
    {
        var alerts = new List<AlertDto>();
        var now = DateTime.UtcNow;

        var accounts = await _context.Accounts
            .Where(a => a.UserId == UserId)
            .ToListAsync();

        foreach (var account in accounts.Where(a => a.CurrentBalance <= 1000))
        {
            alerts.Add(new AlertDto
            {
                Id = $"low-balance-{account.Id}",
                Type = "low_balance",
                Severity = account.CurrentBalance < 0 ? "critical" : "warning",
                Title = $"Low balance: {account.Name}",
                Message = $"Current balance is {account.CurrentBalance:0.##}. Consider topping up soon.",
                CreatedAt = now
            });
        }

        var recentStart = now.AddDays(-30);
        var previousStart = now.AddDays(-60);

        var expenseTransactions = await _context.Transactions
            .Where(t => t.UserId == UserId && t.Type == TransactionType.Expense && t.CategoryId != null)
            .Select(t => new { t.CategoryId, t.Amount, t.Date })
            .ToListAsync();

        var categoryLookup = await _context.Categories
            .Where(c => c.UserId == UserId)
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        var recentByCategory = expenseTransactions
            .Where(t => t.Date >= recentStart)
            .GroupBy(t => t.CategoryId!)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var previousByCategory = expenseTransactions
            .Where(t => t.Date >= previousStart && t.Date < recentStart)
            .GroupBy(t => t.CategoryId!)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        foreach (var (categoryId, recentTotal) in recentByCategory)
        {
            var previousTotal = previousByCategory.GetValueOrDefault(categoryId, 0);
            var hasSignificantIncrease = recentTotal >= 500 && recentTotal > (previousTotal * 1.5);

            if (!hasSignificantIncrease)
                continue;

            var categoryName = categoryLookup.GetValueOrDefault(categoryId, "Uncategorized");

            alerts.Add(new AlertDto
            {
                Id = $"unusual-spending-{categoryId}",
                Type = "unusual_spending",
                Severity = "warning",
                Title = $"Unusual spending in {categoryName}",
                Message = $"Spent {recentTotal:0.##} in the last 30 days (previous 30 days: {previousTotal:0.##}).",
                CreatedAt = now
            });
        }

        var upcomingPayments = await _context.RecurringTransactions
            .Where(rt => rt.UserId == UserId
                         && rt.Type == TransactionType.Expense
                         && rt.IsActive
                         && rt.NextOccurrence != null
                         && rt.NextOccurrence >= now
                         && rt.NextOccurrence <= now.AddDays(7))
            .OrderBy(rt => rt.NextOccurrence)
            .ToListAsync();

        foreach (var payment in upcomingPayments)
        {
            alerts.Add(new AlertDto
            {
                Id = $"due-payment-{payment.Id}",
                Type = "due_payment",
                Severity = "info",
                Title = "Upcoming due payment",
                Message = $"{payment.Description} of {payment.Amount:0.##} is due on {payment.NextOccurrence:yyyy-MM-dd}.",
                CreatedAt = payment.NextOccurrence ?? now
            });
        }

        var sortedAlerts = alerts
            .OrderByDescending(a => SeverityRank(a.Severity))
            .ThenByDescending(a => a.CreatedAt)
            .ToList();

        return Ok(sortedAlerts);
    }

    private static int SeverityRank(string severity)
    {
        return severity switch
        {
            "critical" => 3,
            "warning" => 2,
            _ => 1
        };
    }
}
