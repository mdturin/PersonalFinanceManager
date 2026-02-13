using PersonalFinanceManager.Application.DTOs;
using PersonalFinanceManager.Application.DTOs.Dashboard;

namespace PersonalFinanceManager.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(string userId);
    Task<List<MetricModel>> GetTopExpenseCategoriesAsync(string userId);
    Task<List<MetricModel>> GetRecentTransactionsAsync(string userId);
}
