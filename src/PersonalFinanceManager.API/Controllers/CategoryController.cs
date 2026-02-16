using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
}
