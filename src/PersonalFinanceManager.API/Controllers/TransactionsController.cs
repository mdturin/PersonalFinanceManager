using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Application.DTOs.Transaction;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Infrastructure.Data.Context;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TransactionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: api/transactions
    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] string? accountId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = _context.Transactions
            .Include(t => t.Account)
            .Where(t => t.Account.UserId == UserId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(accountId))
            query = query.Where(t => t.AccountId == accountId);

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        var transactions = await query
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                TargetAccountId = t.TransferToAccountId,
                Type = t.Type,
                Amount = t.Amount,
                CategoryId = t.CategoryId,
                Description = t.Description,
                Date = t.Date
            })
            .ToListAsync();

        return Ok(transactions);
    }

    // GET: api/transactions/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransaction(string id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .Where(t => t.Id == id && t.Account.UserId == UserId)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                TargetAccountId = t.TransferToAccountId,
                Type = t.Type,
                Amount = t.Amount,
                CategoryId = t.CategoryId,
                Description = t.Description,
                Date = t.Date
            })
            .FirstOrDefaultAsync();

        return (transaction == null)
            ? NotFound()
            : Ok(transaction);
    }

    // POST: api/transactions
    [HttpPost]
    public async Task<IActionResult> CreateTransaction(CreateTransactionDto dto)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == dto.AccountId && a.UserId == UserId);

        if (account == null) return BadRequest("Invalid source account.");

        // For transfers, validate target account
        Account? targetAccount = null;
        if (!string.IsNullOrEmpty(dto.TargetAccountId))
        {
            targetAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == dto.TargetAccountId);

            if (targetAccount == null)
                return BadRequest("Invalid target account.");
        }

        // Adjust balances
        if (dto.Type == TransactionType.Expense)
            account.CurrentBalance -= dto.Amount;
        else if (dto.Type == TransactionType.Income)
            account.CurrentBalance += dto.Amount;
        else if (dto.Type == TransactionType.Transfer && targetAccount != null)
        {
            account.CurrentBalance -= dto.Amount;
            targetAccount.CurrentBalance += dto.Amount;
        }
        else
        {
            throw new InvalidEnumArgumentException(nameof(TransactionType), (int)dto.Type, typeof(TransactionType));
        }
        
        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = dto.AccountId,
            TransferToAccountId = dto.TargetAccountId,
            Type = dto.Type,
            Amount = dto.Amount,
            CategoryId = dto.CategoryId,
            Description = dto.Description,
            Date = dto.Date,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction.Id);
    }

    // PUT: api/transactions/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(string id, UpdateTransactionDto dto)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id && t.Account.UserId == UserId);

        if (transaction == null) return NotFound();

        // Optional: reverse old amount from balance
        if (transaction.Type == TransactionType.Expense)
            transaction.Account.CurrentBalance += transaction.Amount;
        else if (transaction.Type == TransactionType.Income)
            transaction.Account.CurrentBalance -= transaction.Amount;

        transaction.Amount = dto.Amount;
        transaction.Description = dto.Description;
        transaction.CategoryId = dto.CategoryId;
        transaction.Date = dto.Date;
        transaction.UpdatedAt = DateTime.UtcNow;

        // Reapply new amount
        if (transaction.Type == TransactionType.Expense)
            transaction.Account.CurrentBalance -= dto.Amount;
        else if (transaction.Type == TransactionType.Income)
            transaction.Account.CurrentBalance += dto.Amount;

        await _context.SaveChangesAsync();
        return NoContent();
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

        // Reverse the transaction from balance
        if (transaction.Type == TransactionType.Expense)
            transaction.Account.CurrentBalance += transaction.Amount;
        else if (transaction.Type == TransactionType.Income)
            transaction.Account.CurrentBalance -= transaction.Amount;
        else if (transaction.Type == TransactionType.Transfer && transaction.TransferToAccount != null)
        {
            transaction.Account.CurrentBalance += transaction.Amount;
            transaction.TransferToAccount.CurrentBalance -= transaction.Amount;
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
