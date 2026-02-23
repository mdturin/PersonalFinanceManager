using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Application.Helpers;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Infrastructure.Data.Context;

namespace PersonalFinanceManager.API.Extensions;

public static class DummyDataProvider
{
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

        // Creating Dummy User
        var dummyUser = await CreateDummyUser(email, password, firstName, lastName, userManager);
        if (dummyUser == null)
        {
            return;
        }

        // Cleanup dummy user data if needed
        await Cleanup(needCleanup, dbContext, dummyUser);

        // Checking for avoid redundant processing
        if (await HasAnyData(needCleanup, dbContext, dummyUser))
        {
            return;
        }

        // Creating Accounts
        (var checkingAccount, var savingsAccount) = await CreateAccounts(dbContext, dummyUser);

        // Creating Categories
        var categories = await CreateCategories(dbContext, dummyUser);

        var salaryCategory = categories["Salary"];
        var groceriesCategory = categories["Groceries"];

        // Creating Recurring Transactions
        var recurringSalary = await CreateRecurringTransactions(
            dbContext,
            dummyUser,
            checkingAccount,
            salaryCategory,
            groceriesCategory);

        // Creating Transactions
        await CreateTransactions(
            dbContext,
            dummyUser,
            checkingAccount,
            categories,
            recurringSalary
        );

        // Creating Budgets
        await CreateBudgets(dbContext, dummyUser, groceriesCategory);

        // Creating Goals
        await CreatingGoals(dbContext, dummyUser);

        await dbContext.SaveChangesAsync();
    }

    private static async Task CreatingGoals(
        ApplicationDbContext dbContext, ApplicationUser dummyUser)
    {
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

        await dbContext.Goals.AddAsync(emergencyGoal);
    }

    private static async Task CreateBudgets(
        ApplicationDbContext dbContext, ApplicationUser dummyUser, Category groceriesCategory)
    {
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

        await dbContext.Budgets.AddAsync(groceryBudget);
    }

    private static async Task CreateTransactions(
        ApplicationDbContext dbContext, 
        ApplicationUser dummyUser, 
        Account checkingAccount, 
        Dictionary<string, Category> categories,
        RecurringTransaction recurringSalary)
    {
        var salaryCategory = categories["Salary"];
        var groceriesCategory = categories["Groceries"];
        var transportCategory = categories["Transport"];
        var shoppingCategory = categories["Shopping"];

        var transactions = new List<Transaction>
        {
            // Historical baseline (31-60 days ago) for unusual spending comparison
            new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = dummyUser.Id,
                AccountId = checkingAccount.Id,
                CategoryId = shoppingCategory.Id,
                Type = TransactionType.Expense,
                Amount = 120,
                Date = DateTime.UtcNow.Date.AddDays(-45),
                CreatedAt = DateTime.UtcNow.Date.AddDays(-45),
                UpdatedAt = DateTime.UtcNow.Date.AddDays(-45),
                Description = "Clothes purchase",
                Tags = "shopping,baseline"
            },
            // Income
            new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = dummyUser.Id,
                AccountId = checkingAccount.Id,
                CategoryId = salaryCategory.Id,
                Type = TransactionType.Income,
                Amount = 5000,
                Date = DateTime.UtcNow.Date.AddDays(-10),
                CreatedAt = DateTime.UtcNow.Date.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.Date.AddDays(-10),
                Description = "Salary for current month",
                IsRecurring = true,
                RecurringTransactionId = recurringSalary.Id,
                Tags = "income,payroll"
            },
            // Current month spending to trigger budget progress and unusual spending alerts
            new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = dummyUser.Id,
                AccountId = checkingAccount.Id,
                CategoryId = groceriesCategory.Id,
                Type = TransactionType.Expense,
                Amount = 145.75,
                Date = DateTime.UtcNow.Date.AddDays(-5),
                CreatedAt = DateTime.UtcNow.Date.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.Date.AddDays(-5),
                Description = "Weekly grocery shopping",
                Tags = "food,home"
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = dummyUser.Id,
                AccountId = checkingAccount.Id,
                CategoryId = shoppingCategory.Id,
                Type = TransactionType.Expense,
                Amount = 780,
                Date = DateTime.UtcNow.Date.AddDays(-4),
                CreatedAt = DateTime.UtcNow.Date.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.Date.AddDays(-4),
                Description = "Electronics purchase",
                Tags = "shopping,unusual"
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
                CreatedAt = DateTime.UtcNow.Date.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.Date.AddDays(-3),
                Description = "Fuel refill",
                Tags = "car,fuel"
            }
        };

        await dbContext.Transactions.AddRangeAsync(transactions);
    }

    private static async Task<RecurringTransaction> CreateRecurringTransactions(
        ApplicationDbContext dbContext, 
        ApplicationUser dummyUser, 
        Account checkingAccount, 
        Category salaryCategory,
        Category groceriesCategory)
    {
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

        var recurringRent = new RecurringTransaction
        {
            Id = Guid.NewGuid().ToString(),
            UserId = dummyUser.Id,
            AccountId = checkingAccount.Id,
            CategoryId = groceriesCategory.Id,
            Type = TransactionType.Expense,
            Amount = 1200,
            Description = "House rent",
            Frequency = RecurrenceFrequency.Monthly,
            FrequencyInterval = 1,
            StartDate = DateTime.UtcNow.Date.AddMonths(-6),
            NextOccurrence = DateTime.UtcNow.Date.AddDays(3)
        };

        await dbContext.RecurringTransactions.AddRangeAsync(recurringSalary, recurringRent);
        return recurringSalary;
    }

    private static async Task<Dictionary<string, Category>> CreateCategories(
        ApplicationDbContext dbContext, ApplicationUser dummyUser)
    {
        var categories = new Dictionary<string, Category>();

        var salaryCategory = new Category
        {
            Id = "Salary".ToNormalizeString(),
            UserId = dummyUser.Id,
            Name = "Salary",
            Type = CategoryType.Income,
            Icon = "wallet",
            Color = "#22C55E",
            SortOrder = 1
        };

        categories.Add("Salary", salaryCategory);

        var groceriesCategory = new Category
        {
            Id = "Groceries".ToNormalizeString(),
            UserId = dummyUser.Id,
            Name = "Groceries",
            Type = CategoryType.Expense,
            Icon = "shopping-cart",
            Color = "#3B82F6",
            SortOrder = 2
        };

        categories.Add("Groceries", groceriesCategory);

        var transportCategory = new Category
        {
            Id = "Transport".ToNormalizeString(),
            UserId = dummyUser.Id,
            Name = "Transport",
            Type = CategoryType.Expense,
            Icon = "car",
            Color = "#F59E0B",
            SortOrder = 3
        };

        categories.Add("Transport", transportCategory);

        var shoppingCategory = new Category
        {
            Id = "Shopping".ToNormalizeString(),
            UserId = dummyUser.Id,
            Name = "Shopping",
            Type = CategoryType.Expense,
            Icon = "mall",
            Color = "#F59E0B",
            SortOrder = 4
        };

        categories.Add("Shopping", shoppingCategory);

        await dbContext.Categories.AddRangeAsync(categories.Values);

        return categories;
    }

    private static async Task<(Account checkingAccount, Account savingsAccount)> CreateAccounts(
        ApplicationDbContext dbContext, ApplicationUser dummyUser)
    {
        var checkingAccount = new Account
        {
            Id = Guid.NewGuid().ToString(),
            UserId = dummyUser.Id,
            Name = "Main Checking",
            Type = AccountType.Checking,
            Institution = "Demo Bank",
            CurrentBalance = 850,
            Currency = "USD",
            Description = "Primary day-to-day account",
            CreatedAt = DateTime.UtcNow.Date.AddMonths(-6),
            UpdatedAt = DateTime.UtcNow.Date.AddMonths(-1)
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
            Description = "Emergency fund account",
            CreatedAt = DateTime.UtcNow.Date.AddMonths(-4),
            UpdatedAt = DateTime.UtcNow.Date.AddMonths(-1)
        };

        await dbContext.Accounts.AddRangeAsync(checkingAccount, savingsAccount);
        return (checkingAccount, savingsAccount);
    }

    private static async Task<bool> HasAnyData(
        bool needCleanup, 
        ApplicationDbContext dbContext, 
        ApplicationUser dummyUser)
    {
        var hasAnyData = await Task.FromResult(needCleanup)
                    || await dbContext.Accounts.AnyAsync(a => a.UserId == dummyUser.Id)
                    || await dbContext.Categories.AnyAsync(c => c.UserId == dummyUser.Id)
                    || await dbContext.Transactions.AnyAsync(t => t.UserId == dummyUser.Id)
                    || await dbContext.Budgets.AnyAsync(b => b.UserId == dummyUser.Id)
                    || await dbContext.Goals.AnyAsync(g => g.UserId == dummyUser.Id)
                    || await dbContext.RecurringTransactions.AnyAsync(rt => rt.UserId == dummyUser.Id);

        return hasAnyData && !needCleanup;
    }

    private static async Task Cleanup(
        bool needCleanup, 
        ApplicationDbContext dbContext, 
        ApplicationUser dummyUser)
    {
        if (!needCleanup)
        {
            return;
        }

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

    private static async Task<ApplicationUser?> CreateDummyUser(
        string email, 
        string password, 
        string firstName, 
        string lastName, 
        UserManager<ApplicationUser> userManager)
    {
        var dummyUser = await userManager.FindByEmailAsync(email);
        if(dummyUser != null)
        {
            return dummyUser;
        }

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
            return null;
        }

        await userManager.AddToRoleAsync(dummyUser, "User");
        return dummyUser;
    }
}
