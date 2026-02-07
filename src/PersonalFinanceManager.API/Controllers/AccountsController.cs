using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.DTOs.Account;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Controllers;

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
                CurrentBalance = a.CurrentBalance,
                Currency = a.Currency,
                IsActive = a.IsActive
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
                Id = a.Id,
                Name = a.Name,
                Type = a.Type,
                CurrentBalance = a.CurrentBalance,
                Currency = a.Currency,
                IsActive = a.IsActive
            })
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound();

        return Ok(account);
    }

    // POST: api/accounts
    [HttpPost]
    public async Task<IActionResult> CreateAccount(CreateAccountDto dto)
    {
        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            UserId = UserId,
            Name = dto.Name,
            Type = dto.Type,
            InitialBalance = dto.InitialBalance,
            CurrentBalance = dto.InitialBalance,
            Currency = dto.Currency,
            Description = dto.Description,
            Color = dto.Color,
            Icon = dto.Icon,
            IncludeInNetWorth = dto.IncludeInNetWorth,
            CreatedAt = DateTime.UtcNow
        };

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
        account.Color = dto.Color;
        account.Icon = dto.Icon;
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
