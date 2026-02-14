using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Application.DTOs;
using PersonalFinanceManager.Application.DTOs.Account;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Infrastructure.Data.Context;
using System.Globalization;
using System.Security.Claims;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AccountsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: api/accounts/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetAccountsSummary()
    {
        var banglaBdt = new CultureInfo("en-BD");
        banglaBdt.NumberFormat.CurrencySymbol = "৳";
        banglaBdt.NumberFormat.CurrencyPositivePattern = 2;

        var taskList = new[]
        {
            GetTotalBalanceMetric(banglaBdt),
            GetMonthlyCashFlowMetric(banglaBdt),
            GetCreditUtilizationMetric(),
            GetConnectedInstitutions()
        };

        return Ok(await Task.WhenAll(taskList));
    }

    private async Task<MetricModel> GetConnectedInstitutions()
    {
        var accounts = _context.Accounts
            .Where(a => a.UserId == UserId);

        var connectedInstitutions = await accounts
            .Select(a => a.Institution)
            .Distinct()
            .CountAsync();

        var totalAccounts = await accounts.CountAsync();

        return new MetricModel()
        {
            Label = "Connected institutions",
            Value = connectedInstitutions.ToString(),
            Helper = $"Across {totalAccounts} Accounts"
        };
    }

    private async Task<MetricModel> GetCreditUtilizationMetric()
    {
        var creditAccounts = await _context.Accounts
            .Where(a => a.UserId == UserId && a.Type == AccountType.CreditCard && a.CreditLimit > 0)
            .Select(a => new
            {
                Used = Math.Abs(a.CurrentBalance),
                Limit = a.CreditLimit
            })
            .ToListAsync();

        if (creditAccounts.Count == 0)
        {
            return new MetricModel
            {
                Label = "Credit Utilization",
                Value = "0%",
                Helper = "No credit accounts"
            };
        }

        var totalUsed = creditAccounts.Sum(a => a.Used);
        var totalLimit = creditAccounts.Sum(a => a.Limit);

        var utilization = totalLimit == 0
            ? 0
            : (totalUsed / totalLimit) * 100;

        return new MetricModel
        {
            Label = "Credit Utilization",
            Value = $"{utilization:0.#}%",
            Helper = $"Used {totalUsed:0.##} of {totalLimit:0.##}"
        };
    }

    private async Task<MetricModel> GetMonthlyCashFlowMetric(CultureInfo banglaBdt)
    {
        var now = DateTime.UtcNow;

        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var currentMonthCashFlow = await CalculateMonthlyCashFlow(startOfMonth, endOfMonth);
        var previousMonthCashFlow = await CalculateMonthlyCashFlow(
            startOfMonth.AddMonths(-1),
            endOfMonth.AddMonths(-1)
        );

        string? note;
        if (previousMonthCashFlow == 0)
        {
            note = currentMonthCashFlow == 0
                ? "No change from last month"
                : "New activity this month";
        }
        else
        {
            var percentageChange = (currentMonthCashFlow - previousMonthCashFlow)
                / Math.Abs(previousMonthCashFlow) * 100;

            var direction = percentageChange >= 0 ? "Up" : "Down";

            note = $"{direction} {Math.Abs(percentageChange):0.#}% from last month";
        }

        return new MetricModel
        {
            Label = "Monthly Cash Flow",
            Value = currentMonthCashFlow.ToString("C", banglaBdt),
            Helper = note
        };
    }

    private async Task<double> CalculateMonthlyCashFlow(DateTime startOfMonth, DateTime endOfMonth)
    {
        var monthlyTransactions = _context.Transactions
            .Where(t => t.UserId == UserId && t.Date >= startOfMonth && t.Date < endOfMonth);

        var totalIncome = await monthlyTransactions
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);

        var totalExpense = await monthlyTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount);

        return totalIncome - totalExpense;
    }

    private async Task<MetricModel> GetTotalBalanceMetric(CultureInfo banglaBdt)
    {
        var totalBalance = await _context.Accounts
            .Where(a => a.UserId == UserId)
            .SumAsync(a => a.CurrentBalance);

        var totalNumberOfAccounts = await _context.Accounts
            .CountAsync(a => a.UserId == UserId);

        MetricModel item = new()
        {
            Label = "Total Balance",
            Value = totalBalance.ToString("C", banglaBdt),
            Helper = $"Across {totalNumberOfAccounts} accounts"
        };

        return item;
    }

    // GET: api/accounts/account-mix
    [HttpGet("account-mix")]
    public async Task<IActionResult> GetAccountMix()
    {
        var mixture = await _context.Accounts
            .Where(a => a.UserId == UserId)
            .GroupBy(a => a.Type)
            .Select(g => new
            {
                Type = g.Key,
                Value = g.Sum(a => a.CurrentBalance)
            })
            .OrderByDescending(s => s.Value)
            .ToListAsync();

        var totalBalance = mixture.Sum(m => m.Value);

        var metrics = mixture.Select(m =>
        {
            var contrib = totalBalance == 0
                ? 0
                : (m.Value / totalBalance) * 100;

            var metric = new MetricModel
            {
                Label = m.Type.ToString(),
                Value = $"{contrib:0.00}%",
                Trend = GetTrendValue(contrib)
            };

            return metric;
        }).ToList();

        return Ok(metrics);
    }

    private static string GetTrendValue(double value)
    {
        if (value >= 60) return "bg-success";
        if (value >= 40) return "";
        if (value >= 20) return "bg-warning";

        return "bg-danger";
    }

    // GET: api/accounts
    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var banglaBdt = new CultureInfo("en-BD");
        banglaBdt.NumberFormat.CurrencySymbol = "৳";
        banglaBdt.NumberFormat.CurrencyPositivePattern = 2;

        var accounts = await _context.Accounts
            .Where(a => a.UserId == UserId)
            .Select(a => new
            {
                Id = a.Id,
                Name = a.Name,
                Type = a.Type,
                Institution = a.Institution,
                CurrentBalance = a.CurrentBalance.ToString("C", banglaBdt),
                Currency = a.Currency,
                IsActive = a.IsActive,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return Ok(accounts.Select(a => new AccountDto()
        {
            Id = a.Id,
            Name = a.Name,
            Type = a.Type.ToString(),
            Institution = a.Institution,
            CurrentBalance = a.CurrentBalance,
            Currency = a.Currency,
            IsActive = a.IsActive,
            UpdatedAt = a.UpdatedAt
        }));
    }

    // GET: api/accounts/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccount(string id)
    {
        var banglaBdt = new CultureInfo("en-BD");
        banglaBdt.NumberFormat.CurrencySymbol = "৳";
        banglaBdt.NumberFormat.CurrencyPositivePattern = 2;

        var account = await _context.Accounts
            .Where(a => a.Id == id && a.UserId == UserId)
            .Select(a => new
            {
                Name = a.Name,
                Type = a.Type,
                Institution = a.Institution,
                CurrentBalance = a.CurrentBalance.ToString("C", banglaBdt),
                Currency = a.Currency,
                IsActive = a.IsActive,
                UpdatedAt = a.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return account == null
            ? NotFound()
            : Ok(new AccountDto()
            {
                Name = account.Name,
                Type = account.Type.ToString(),
                Institution = account.Institution,
                CurrentBalance = account.CurrentBalance,
                Currency = account.Currency,
                IsActive = account.IsActive,
                UpdatedAt = account.UpdatedAt
            });
    }

    // POST: api/accounts
    [HttpPost]
    public async Task<IActionResult> CreateAccount(CreateAccountDto dto)
    {
        var account = dto.ToAccount(UserId);
        
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account.Id);
    }

    // PUT: api/accounts/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(string id, UpdateAccountDto dto)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);

        if (account == null)
            return NotFound();

        account.Name = dto.Name;
        account.IsActive = dto.IsActive;
        account.IncludeInNetWorth = dto.IncludeInNetWorth;
        account.Description = dto.Description;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/accounts/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(string id)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);

        if (account == null)
            return NotFound();

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
