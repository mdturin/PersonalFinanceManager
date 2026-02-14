using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Application.DTOs.Transaction;

public class TransactionDto
{
    public string Id { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Type { get; set; }
    public double Amount { get; set; }
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
}