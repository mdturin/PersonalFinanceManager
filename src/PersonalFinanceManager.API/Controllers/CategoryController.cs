using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Application.DTOs.Budget;
using PersonalFinanceManager.Application.DTOs.Category;
using PersonalFinanceManager.Application.Helpers;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Infrastructure.Data.Context;
using System.Security.Claims;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/category")]
public class CategoryController : ControllerBase
{
    private ApplicationDbContext Context { get; }

    public CategoryController(ApplicationDbContext _context)
    {
        Context = _context;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: api/category
    [HttpGet]
    public async Task<IActionResult> GetCategoriesAsync()
    {
        var categories = await Context.Categories
            .Where(c => c.UserId == UserId)
            .Select(c => new
            {
                c.Id,
                c.Name
            })
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/category/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));

        var category = await Context.FindAsync<Category>(id);
        return (category == null) ? NotFound() : Ok(category);
    }

    // POST: api/category
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidDataException("Category name can't be empty!");

        var category = new Category()
        {
            UserId = UserId,
            Id = dto.Name.ToNormalizeString(),
            Name = dto.Name,
            Type = dto.Type,
        };

        await Context.AddAsync(category);
        await Context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, new CategoryDto(category));
    }

}
