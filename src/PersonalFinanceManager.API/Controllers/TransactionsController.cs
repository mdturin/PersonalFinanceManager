using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Application.DTOs.Transaction;
using PersonalFinanceManager.Application.Helpers;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Infrastructure.Data.Context;
using System.ComponentModel;
using System.Security.Claims;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ILogger<TransactionsController> _logger;
    private readonly ApplicationDbContext _context;

    public TransactionsController(
        ILogger<TransactionsController> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: api/transactions
    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] string? type = null,
        [FromQuery] string? accountId = null,
        [FromQuery] string? categoryName = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = _context.Transactions
            .Include(t => t.Account)
            .Where(t => t.Account.UserId == UserId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<TransactionType>(type, true, out var transactionType))
        {
            query = query.Where(t => t.Type == transactionType);
        }

        if (!string.IsNullOrWhiteSpace(accountId))
        {
            query = query.Where(t => t.AccountId == accountId);
        }

        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            var categoryId = categoryName.ToCheckSum();
            query = query.Where(t => t.CategoryId == categoryId);
        }

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        var transactions = await query
            .Include(t => t.Account)
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        var result = transactions
            .Select(t => new TransactionDto(t))
            .ToList();

        return Ok(result);
    }

    // GET: api/transactions/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransaction(string id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.Id == id && t.UserId == UserId)
            .FirstOrDefaultAsync();

        return (transaction == null)
            ? NotFound()
            : Ok(new TransactionDto(transaction));
    }

    // POST: api/transactions
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto dto)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == dto.AccountId && a.UserId == UserId);

        if (account == null) return BadRequest("Invalid source account.");

        if (!Enum.TryParse<TransactionType>(dto.Type, true, out var transactionType))
            return BadRequest("Invalid transaction type.");

        Transaction? transaction = null;
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();

        try
        {
            AdjustCreatingTransactionAccountBalance(dto, account, transactionType);

            transaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                UserId = UserId,
                AccountId = dto.AccountId,
                CategoryId = dto.CategoryId,
                Type = transactionType,
                Amount = dto.Amount,
                Date = dto.Date,
                Description = dto.Description
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction creation failed! Rolling back.");
            await dbTransaction.RollbackAsync();
            throw;
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(transaction!.CategoryId) && transaction.Category == null)
            {
                transaction.Category = await _context.Categories
                    .FindAsync(transaction.CategoryId);
            }
        }

        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, new TransactionDto(transaction));
    }

    private static void AdjustCreatingTransactionAccountBalance(
        TransactionDto dto, 
        Account account, 
        TransactionType transactionType)
    {
        // Adjust balances
        switch (transactionType)
        {
            case TransactionType.Expense:
                account.CurrentBalance -= dto.Amount;
                break;

            case TransactionType.Income:
                account.CurrentBalance += dto.Amount;
                break;

            default:
                throw new InvalidEnumArgumentException(nameof(TransactionType), (int)transactionType, typeof(TransactionType));
        }
    }

    // PUT: api/transactions/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(string id, TransactionDto dto)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.Account.UserId == UserId);

        if (transaction == null) return NotFound();

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == UserId);

        if (category == null)
            return BadRequest("Invalid category.");

        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            AdjustRevertingTransactionAccountBalance(transaction);

            transaction.Amount = dto.Amount;
            transaction.Description = dto.Description;
            transaction.Date = dto.Date;
            transaction.UpdatedAt = DateTime.UtcNow;

            if (transaction.CategoryId != dto.CategoryId)
            {
                transaction.CategoryId = dto.CategoryId;
                transaction.Category = category; // optional if tracking
            }

            AdjustCreatingTransactionAccountBalance(dto, transaction.Account, transaction.Type);

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction update failed! Rolling back.");
            await dbTransaction.RollbackAsync();
            throw;
        }

        return Ok(new TransactionDto(transaction));
    }

    // DELETE: api/transactions/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(string id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.TransferToAccount)
            .FirstOrDefaultAsync(t => t.Id == id && t.Account.UserId == UserId);

        if (transaction == null) return NotFound();

        AdjustRevertingTransactionAccountBalance(transaction);

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static void AdjustRevertingTransactionAccountBalance(Transaction transaction)
    {
        // Reverse the transaction from balance
        switch (transaction.Type)
        {
            case TransactionType.Expense:
                transaction.Account.CurrentBalance += transaction.Amount;
                break;

            case TransactionType.Income:
                transaction.Account.CurrentBalance -= transaction.Amount;
                break;

            default:
                throw new InvalidEnumArgumentException(nameof(TransactionType), (int)transaction.Type, typeof(TransactionType));
        }
    }
}
