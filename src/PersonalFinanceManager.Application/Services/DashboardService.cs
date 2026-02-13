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
    public async Task<DashboardSummaryDto> GetSummaryAsync(string userId)
    {
        if(string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }

        var metrics = new List<MetricModel>();
        var createMetricModel = new Func<string, double, MetricModel>((label, value) =>
        {
            var banglaBdt = new CultureInfo("en-BD"); // English Bangladesh
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

        var dashboardSummary = new DashboardSummaryDto
        {
            Metrics = metrics
        };

        return dashboardSummary;
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
}
