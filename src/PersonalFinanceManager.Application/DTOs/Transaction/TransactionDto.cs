using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Application.DTOs.Transaction;

public class TransactionDto
{
    public string Id { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string? TargetAccountId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? CategoryId { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
}