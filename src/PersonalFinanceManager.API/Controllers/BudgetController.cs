using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/budgets")]
public class BudgetController : ControllerBase
{
}
