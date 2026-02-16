using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Application.DTOs;
using PersonalFinanceManager.Application.DTOs.Dashboard;
using PersonalFinanceManager.Application.Interfaces;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Infrastructure.Data.Context;
using System.Globalization;

namespace PersonalFinanceManager.Application.Services;

public class DashboardService(ApplicationDbContext Context) : IDashboardService
{
    public async Task<List<MetricModel>> GetSummaryAsync(string userId)
    {
        if(string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }

        var metrics = new List<MetricModel>();
        
        var banglaBdt = new CultureInfo("en-BD");
        banglaBdt.NumberFormat.CurrencySymbol = "৳";
        banglaBdt.NumberFormat.CurrencyPositivePattern = 2;

        var createMetricModel = new Func<string, double, MetricModel>((label, value) =>
        {
            return new MetricModel
            {
                Label = label,
                Value = value.ToString("C", banglaBdt)
            };
        });

        metrics.Add(createMetricModel("Total Balance", await GetTotalBalanceAsync(userId)));

        var totalIncome = await GetTotalIncomeAsync(userId);
        metrics.Add(createMetricModel("Total Income", totalIncome));

        var totalExpense = await GetTotalExpenseAsync(userId);
        metrics.Add(createMetricModel("Total Expense", totalExpense));

        var totalSaving = totalIncome - totalExpense;
        metrics.Add(createMetricModel("Total Saving", totalSaving));

        return metrics;
    }

    private Task<double> GetTotalExpenseAsync(string userId)
    {
        return Context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount);
    }

    private Task<double> GetTotalIncomeAsync(string userId)
    {
        return Context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);
    }

    private Task<double> GetTotalBalanceAsync(string userId)
    {
        return Context.Accounts
            .Where(a => a.UserId == userId)
            .SumAsync(a => a.CurrentBalance);
    }

    public async Task<List<MetricModel>> GetTopExpenseCategoriesAsync(string userId)
    {
        var topExpenses = await Context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.Category != null)
            .GroupBy(t => t.Category!.Name)
            .Select(g => new
            {
                CategoryName = g.Key,
                TotalAmount  = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .Take(5)
            .ToListAsync();

        var banglaBdt = new CultureInfo("en-BD");
        banglaBdt.NumberFormat.CurrencySymbol = "৳";
        banglaBdt.NumberFormat.CurrencyPositivePattern = 2;

        return topExpenses
            .Select(e => new MetricModel()
            {
                Label = e.CategoryName,
                Value = e.TotalAmount.ToString("C", banglaBdt)
            })
            .ToList();
    }

    public async Task<List<MetricModel>> GetRecentTransactionsAsync(string userId)
    {
        var topExpenses = await Context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
            .OrderByDescending(t => t.Amount)
            .Take(5)
            .Include(t => t.Category)
            .Select(t => new
            {
                CategoryName = t.Category!.Name,
                TotalAmount = t.Amount
            })
            .ToListAsync();

        var banglaBdt = new CultureInfo("en-BD"); 
        banglaBdt.NumberFormat.CurrencySymbol = "৳"; 
        banglaBdt.NumberFormat.CurrencyPositivePattern = 2;

        return topExpenses
            .Select(e => new MetricModel()
            {
                Label = e.CategoryName,
                Value = e.TotalAmount.ToString("C", banglaBdt)
            })
            .ToList();
    }

    public async Task<List<MetricModel>> GetExpenseTrendAsync(string userId)
    {
        var now = DateTime.UtcNow;

        var startDate = new DateTime(now.Year, 1, 1);
        var endDate = startDate.AddYears(1);

        var expensesCurrentYear = await Context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
            .Where(t => t.Date >= startDate && t.Date < endDate)
            .GroupBy(t => t.Date.Month)
            .Select(g => new
            {
                Month = g.Key,
                TotalAmount = g.Sum(t => t.Amount)
            })
            .ToListAsync();

        return expensesCurrentYear
            .Select(ex => new MetricModel()
            {
                Label = GetMonthName(ex.Month),
                Value = ex.TotalAmount.ToString()
            })
            .ToList();
    }

    private static string GetMonthName(int key)
    {
        return key switch
        {
            1 => "Jan",
            2 => "Feb",
            3 => "Mar",
            4 => "Apr",
            5 => "May",
            6 => "Jun",
            7 => "Jul",
            8 => "Aug",
            9 => "Sep",
            10 => "Oct",
            11 => "Nov",
            12 => "Dec",
            _ => throw new ArgumentOutOfRangeException(nameof(key), "Invalid month number")
        };
    }

    public async Task<List<MetricModel>> GetIncomeVsExpenseAsync(string userId)
    {
        var now = DateTime.UtcNow;

        var startDate = new DateTime(now.Year, 1, 1);
        var endDate = startDate.AddYears(1);

        var transactions = Context.Transactions
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Date < endDate);

        var incomeTotalAmountTask = transactions
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);

        var expenseTotalAmountTask = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount);

        var banglaBdt = new CultureInfo("en-BD");
        banglaBdt.NumberFormat.CurrencySymbol = "৳";
        banglaBdt.NumberFormat.CurrencyPositivePattern = 2;

        await Task.WhenAll(incomeTotalAmountTask, expenseTotalAmountTask);

        var incomeTotalAmount = incomeTotalAmountTask.Result;
        var expenseTotalAmount = expenseTotalAmountTask.Result;

        return
        [
            new MetricModel()
            {
                Label = "Income",
                Value = incomeTotalAmount.ToString(),
                Trend = "#198754"
            },
            new MetricModel()
            {
                Label = "Expense",
                Value = expenseTotalAmount.ToString(),
                Trend = "#dc3545"
            }
        ];
    }

}
