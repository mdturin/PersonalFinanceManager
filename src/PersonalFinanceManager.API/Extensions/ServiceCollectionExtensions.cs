using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceManager.Core.Configurations;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Infrastructure.Data.Context;
using PersonalFinanceManager.Application.Helpers;

namespace PersonalFinanceManager.API.Extensions;

/// <summary>
/// Extension methods for configuring user management services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds user management services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddUserManagement(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString,
        Action<IdentityOptions>? identityOptions = null,
        string? migrationsAssembly = null)
    {
        // Configure JWT settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

        if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("JWT settings are not properly configured.");
        }

        // Add DbContext with migrations assembly configuration
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (string.IsNullOrEmpty(migrationsAssembly))
            {
                // Try to get the calling assembly name
                var assembly = Assembly.GetCallingAssembly();
                migrationsAssembly = assembly.GetName().Name;
            }

            options.UseSqlite(connectionString, 
                sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly));
        });

        // Add Identity
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Default options
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;

                // Apply custom options if provided
                identityOptions?.Invoke(options);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Add Authentication
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    public static async Task SeedConfigAsync(this IServiceProvider serviceProvider)
    {
        var liteDbContext = serviceProvider.GetService<LiteDbContext>();
        if(liteDbContext == null)
            throw new Exception("LiteDbContext is not configured.");

        var config = new SideNavConfig()
        {
            Version = "2026-02-07-01",
            Sections =
            [
                new NavSection()
                {
                    Title = "Overview",
                    Items =
                    [
                        new NavItem()
                        {
                            Label = "Dashboard",
                            Route = "/",
                            Active = true,
                        },
                        
                        new NavItem()
                        {
                            Label = "Transactions",
                            Route = "/transactions",
                            Active = false,
                        },
                        
                        new NavItem()
                        {
                            Label = "Accounts",
                            Route = "/accounts",
                            Active = false,
                        },
                    ]
                }
            ]
        };

        liteDbContext.Save(config);
    }

    /// <summary>
    /// Seeds default roles
    /// </summary>
    public static async Task SeedRolesAsync(this IServiceProvider serviceProvider, params string[] roles)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    /// <summary>
    /// Seeds a default admin user
    /// </summary>
    public static async Task SeedAdminUserAsync(
        this IServiceProvider serviceProvider,
        string email,
        string password,
        string firstName,
        string lastName)
    {
        var userManager = serviceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var adminUser = await userManager.FindByEmailAsync(email);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    /// <summary>
    /// Seeds a dummy user and related finance data across all core models.
    /// </summary>
    public static async Task SeedDummyUserDataAsync(
        this IServiceProvider serviceProvider,
        string email,
        string password,
        string firstName,
        string lastName,
        bool needCleanup = false)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

        var dummyUser = await userManager.FindByEmailAsync(email);
        if (dummyUser == null)
        {
            dummyUser = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Currency = "USD",
                TimeZone = "UTC"
            };

            var result = await userManager.CreateAsync(dummyUser, password);
            if (!result.Succeeded)
            {
                return;
            }

            await userManager.AddToRoleAsync(dummyUser, "User");
        }

        if (needCleanup)
        {
            dbContext.Accounts
                .RemoveRange(await dbContext.Accounts.Where(a => a.UserId == dummyUser.Id).ToListAsync());

            dbContext.Categories
                .RemoveRange(await dbContext.Categories.Where(a => a.UserId == dummyUser.Id).ToListAsync());

            dbContext.Transactions
                .RemoveRange(await dbContext.Transactions.Where(a => a.UserId == dummyUser.Id).ToListAsync());

            dbContext.Budgets
                .RemoveRange(await dbContext.Budgets.Where(a => a.UserId == dummyUser.Id).ToListAsync());

            dbContext.Goals
                .RemoveRange(await dbContext.Goals.Where(a => a.UserId == dummyUser.Id).ToListAsync());

            dbContext.RecurringTransactions
                .RemoveRange(await dbContext.RecurringTransactions.Where(a => a.UserId == dummyUser.Id).ToListAsync());
        }

        var hasAnyData = await Task.FromResult(needCleanup)
            || await dbContext.Accounts.AnyAsync(a => a.UserId == dummyUser.Id)
            || await dbContext.Categories.AnyAsync(c => c.UserId == dummyUser.Id)
            || await dbContext.Transactions.AnyAsync(t => t.UserId == dummyUser.Id)
            || await dbContext.Budgets.AnyAsync(b => b.UserId == dummyUser.Id)
            || await dbContext.Goals.AnyAsync(g => g.UserId == dummyUser.Id)
            || await dbContext.RecurringTransactions.AnyAsync(rt => rt.UserId == dummyUser.Id);

        if (!needCleanup && hasAnyData)
        {
            return;
        }

        var checkingAccount = new Account
        {
            Id = Guid.NewGuid().ToString(),
            UserId = dummyUser.Id,
            Name = "Main Checking",
            Type = AccountType.Checking,
            Institution = "Demo Bank",
            CurrentBalance = 3250,
            Currency = "USD",
            Description = "Primary day-to-day account"
        };

        var savingsAccount = new Account
        {
            Id = Guid.NewGuid().ToString(),
            UserId = dummyUser.Id,
            Name = "Emergency Savings",
            Type = AccountType.Savings,
            Institution = "Demo Bank",
            CurrentBalance = 9400,
            Currency = "USD",
            Description = "Emergency fund account"
        };

        var salaryCategory = new Category
        {
            Id = "Salary".ToCheckSum(),
            UserId = dummyUser.Id,
            Name = "Salary",
            Type = CategoryType.Income,
            Icon = "wallet",
            Color = "#22C55E",
            SortOrder = 1
        };

        var groceriesCategory = new Category
        {
            Id = "Groceries".ToCheckSum(),
            UserId = dummyUser.Id,
            Name = "Groceries",
            Type = CategoryType.Expense,
            Icon = "shopping-cart",
            Color = "#3B82F6",
            SortOrder = 2
        };

        var transportCategory = new Category
        {
            Id = "Transport".ToCheckSum(),
            UserId = dummyUser.Id,
            Name = "Transport",
            Type = CategoryType.Expense,
            Icon = "car",
            Color = "#F59E0B",
            SortOrder = 3
        };

        var recurringSalary = new RecurringTransaction
        {
            Id = Guid.NewGuid().ToString(),
            UserId = dummyUser.Id,
            AccountId = checkingAccount.Id,
            CategoryId = salaryCategory.Id,
            Type = TransactionType.Income,
            Amount = 5000,
            Description = "Monthly Salary",
            Frequency = RecurrenceFrequency.Monthly,
            FrequencyInterval = 1,
            StartDate = DateTime.UtcNow.Date.AddMonths(-6),
            NextOccurrence = DateTime.UtcNow.Date.AddMonths(1)
        };

        var transactions = new List<Transaction>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = dummyUser.Id,
                AccountId = checkingAccount.Id,
                CategoryId = salaryCategory.Id,
                Type = TransactionType.Income,
                Amount = 5000,
                Date = DateTime.UtcNow.Date.AddDays(-10),
                Description = "Salary for current month",
                IsRecurring = true,
                RecurringTransactionId = recurringSalary.Id,
                Tags = "income,payroll"
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = dummyUser.Id,
                AccountId = checkingAccount.Id,
                CategoryId = groceriesCategory.Id,
                Type = TransactionType.Expense,
                Amount = 145.75,
                Date = DateTime.UtcNow.Date.AddDays(-5),
                Description = "Weekly grocery shopping",
                Tags = "food,home"
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = dummyUser.Id,
                AccountId = checkingAccount.Id,
                CategoryId = transportCategory.Id,
                Type = TransactionType.Expense,
                Amount = 42.30,
                Date = DateTime.UtcNow.Date.AddDays(-3),
                Description = "Fuel refill",
                Tags = "car,fuel"
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = dummyUser.Id,
                AccountId = checkingAccount.Id,
                TransferToAccountId = savingsAccount.Id,
                Type = TransactionType.Transfer,
                Amount = 600,
                Date = DateTime.UtcNow.Date.AddDays(-2),
                Description = "Monthly savings transfer",
                Notes = "Auto transfer to emergency fund"
            }
        };

        var groceryBudget = new Budget
        {
            Id = Guid.NewGuid().ToString(),
            UserId = dummyUser.Id,
            CategoryId = groceriesCategory.Id,
            Name = "Monthly Groceries Budget",
            Amount = 600,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
            AlertThreshold = 75
        };

        var emergencyGoal = new Goal
        {
            Id = Guid.NewGuid().ToString(),
            UserId = dummyUser.Id,
            Name = "Emergency Fund",
            Description = "Build a 6-month emergency fund",
            TargetAmount = 15000,
            CurrentAmount = 9400,
            TargetDate = DateTime.UtcNow.Date.AddMonths(9),
            Status = GoalStatus.InProgress,
            Icon = "target",
            Color = "#8B5CF6",
            Priority = 1
        };

        await dbContext.Accounts.AddRangeAsync(checkingAccount, savingsAccount);
        await dbContext.Categories.AddRangeAsync(salaryCategory, groceriesCategory, transportCategory);
        await dbContext.RecurringTransactions.AddAsync(recurringSalary);
        await dbContext.Transactions.AddRangeAsync(transactions);
        await dbContext.Budgets.AddAsync(groceryBudget);
        await dbContext.Goals.AddAsync(emergencyGoal);

        await dbContext.SaveChangesAsync();
    }
}
