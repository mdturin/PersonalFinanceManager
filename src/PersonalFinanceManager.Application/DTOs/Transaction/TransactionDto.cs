using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.Application.DTOs.Transaction;

public class TransactionDto
{
    public string Id { get; set; } = string.Empty;

    public string AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;

    public string? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public string Type { get; set; }
    public double Amount { get; set; }
    public string? Description { get; set; }

    public DateTime Date { get; set; }

    public TransactionDto()
    {
    }

    public TransactionDto(Core.Entities.Transaction transaction)
    {
        Id = transaction.Id;
        AccountId = transaction.AccountId;
        AccountName = transaction.Account.Name;

        CategoryId = transaction.CategoryId;
        CategoryName = transaction.Category?.Name;

        Amount = transaction.Amount;
        Type = transaction.Type.ToString();
        Date = transaction.Date;
        Description = transaction.Description;
    }
}