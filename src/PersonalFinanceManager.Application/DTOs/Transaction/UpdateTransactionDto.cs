namespace PersonalFinanceManager.Application.DTOs.Transaction;

public class UpdateTransactionDto
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? CategoryId { get; set; }
    public DateTime Date { get; set; }
}