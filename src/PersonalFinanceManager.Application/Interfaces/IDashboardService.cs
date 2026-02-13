using PersonalFinanceManager.Application.DTOs.Dashboard;

namespace PersonalFinanceManager.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(string userId);
}
