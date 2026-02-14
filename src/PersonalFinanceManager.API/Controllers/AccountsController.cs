using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Application.DTOs.Account;
using PersonalFinanceManager.Infrastructure.Data.Context;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AccountsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: api/accounts
    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var accounts = await _context.Accounts
            .Where(a => a.UserId == UserId)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                Name = a.Name,
                Type = a.Type,
                Institution = a.Institution,
                CurrentBalance = a.CurrentBalance,
                Currency = a.Currency,
                IsActive = a.IsActive,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return Ok(accounts);
    }

    // GET: api/accounts/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccount(string id)
    {
        var account = await _context.Accounts
            .Where(a => a.Id == id && a.UserId == UserId)
            .Select(a => new AccountDto
            {
                Name = a.Name,
                Type = a.Type,
                Institution = a.Institution,
                CurrentBalance = a.CurrentBalance,
                Currency = a.Currency,
                IsActive = a.IsActive,
                UpdatedAt = a.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return account == null
            ? NotFound()
            : Ok(account);
    }

    // POST: api/accounts
    [HttpPost]
    public async Task<IActionResult> CreateAccount(CreateAccountDto dto)
    {
        var account = dto.ToAccount(UserId);
        
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account.Id);
    }

    // PUT: api/accounts/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(string id, UpdateAccountDto dto)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);

        if (account == null)
            return NotFound();

        account.Name = dto.Name;
        account.IsActive = dto.IsActive;
        account.IncludeInNetWorth = dto.IncludeInNetWorth;
        account.Description = dto.Description;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/accounts/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(string id)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);

        if (account == null)
            return NotFound();

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
