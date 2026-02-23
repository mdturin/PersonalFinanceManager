using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Application.DTOs.Budget;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Infrastructure.Data.Context;
using System.Globalization;
using System.Security.Claims;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/budgets")]
public class BudgetController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BudgetController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetBudgets()
    {
        var budgets = await _context.Budgets
            .Include(b => b.Category)
            .Where(b => b.UserId == UserId && b.IsActive)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();

        return Ok(budgets.Select(b => new BudgetDto(b)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] BudgetUpsertDto dto)
    {
        if (!TryParseMonth(dto.Month, out var monthDate))
            return BadRequest("Month must be in yyyy-MM format.");

        if (dto.Amount <= 0)
            return BadRequest("Amount must be greater than zero.");

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId && c.UserId == UserId);

        if (!categoryExists)
            return BadRequest("Invalid category.");

        var existingBudget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.UserId == UserId
                                      && b.CategoryId == dto.CategoryId
                                      && b.Period == BudgetPeriod.Monthly
                                      && b.StartDate.Year == monthDate.Year
                                      && b.StartDate.Month == monthDate.Month
                                      && b.IsActive);

        if (existingBudget != null)
            return Conflict("A monthly budget already exists for this category and month.");

        var budget = new Core.Entities.Budget
        {
            Id = Guid.NewGuid().ToString(),
            UserId = UserId,
            CategoryId = dto.CategoryId,
            Name = $"Monthly budget {dto.Month}",
            Amount = dto.Amount,
            Period = BudgetPeriod.Monthly,
            StartDate = monthDate,
            EndDate = monthDate.AddMonths(1).AddDays(-1),
            IsActive = true
        };

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        await _context.Entry(budget).Reference(b => b.Category).LoadAsync();

        return CreatedAtAction(nameof(GetBudgetById), new { id = budget.Id }, new BudgetDto(budget));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBudgetById(string id)
    {
        var budget = await _context.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId && b.IsActive);

        return budget == null
            ? NotFound()
            : Ok(new BudgetDto(budget));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBudget(string id, [FromBody] BudgetUpsertDto dto)
    {
        var budget = await _context.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId && b.IsActive);

        if (budget == null)
            return NotFound();

        if (!TryParseMonth(dto.Month, out var monthDate))
            return BadRequest("Month must be in yyyy-MM format.");

        if (dto.Amount <= 0)
            return BadRequest("Amount must be greater than zero.");

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId && c.UserId == UserId);

        if (!categoryExists)
            return BadRequest("Invalid category.");

        var duplicateBudget = await _context.Budgets
            .AnyAsync(b => b.Id != id
                           && b.UserId == UserId
                           && b.CategoryId == dto.CategoryId
                           && b.Period == BudgetPeriod.Monthly
                           && b.StartDate.Year == monthDate.Year
                           && b.StartDate.Month == monthDate.Month
                           && b.IsActive);

        if (duplicateBudget)
            return Conflict("A monthly budget already exists for this category and month.");

        budget.CategoryId = dto.CategoryId;
        budget.Amount = dto.Amount;
        budget.StartDate = monthDate;
        budget.EndDate = monthDate.AddMonths(1).AddDays(-1);
        budget.Name = $"Monthly budget {dto.Month}";
        budget.Period = BudgetPeriod.Monthly;

        await _context.SaveChangesAsync();
        await _context.Entry(budget).Reference(b => b.Category).LoadAsync();

        return Ok(new BudgetDto(budget));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBudget(string id)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId && b.IsActive);

        if (budget == null)
            return NotFound();

        budget.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static bool TryParseMonth(string month, out DateTime monthDate)
    {
        return DateTime.TryParseExact(
            month,
            "yyyy-MM",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out monthDate);
    }
}
