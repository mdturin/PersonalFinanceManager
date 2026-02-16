using PersonalFinanceManager.Application.DTOs;
using PersonalFinanceManager.Application.DTOs.Dashboard;

namespace PersonalFinanceManager.Application.Interfaces;

public interface IDashboardService
{
    Task<List<MetricModel>> GetSummaryAsync(string userId);
    Task<List<MetricModel>> GetTopExpenseCategoriesAsync(string userId);
    Task<List<MetricModel>> GetRecentTransactionsAsync(string userId);
    Task<List<MetricModel>> GetExpenseTrendAsync(string userId);
    Task<List<MetricModel>> GetIncomeVsExpenseAsync(string userId);
}
