PersonalFinanceManager
├── appsettings.Development.json
├── appsettings.json
├── Configurations
│   └── JwtSettings.cs
├── Controllers
│   ├── AuthController.cs
│   └── UsersController.cs
├── Data
│   ├── ApplicationDbContext.cs
│   └── Configurations
│       ├── AccountConfiguration.cs
│       ├── ApplicationUserConfiguration.cs
│       ├── BudgetConfiguration.cs
│       ├── CategoryConfiguration.cs
│       ├── GoalConfiguration.cs
│       ├── RecurringTransactionConfiguration.cs
│       └── TransactionConfiguration.cs
├── Dockerfile
├── DTOs
│   ├── AuthResponseDto.cs
│   ├── ChangePasswordDto.cs
│   ├── ForgotPasswordDto.cs
│   ├── LoginDto.cs
│   ├── RefreshTokenDto.cs
│   ├── RegisterDto.cs
│   ├── ResetPasswordDto.cs
│   ├── UpdateUserDto.cs
│   └── UserDto.cs
├── Enums
│   ├── AccountType.cs
│   ├── BudgetPeriod.cs
│   ├── CategoryType.cs
│   ├── GoalStatus.cs
│   ├── RecurrenceFrequency.cs
│   └── TransactionType.cs
├── Extensions
│   └── ServiceCollectionExtensions.cs
├── Interfaces
│   ├── IAuthService.cs
│   ├── ITokenService.cs
│   └── IUserService.cs
├── Models
│   ├── Account.cs
│   ├── ApplicationUser.cs
│   ├── BaseEntity.cs
│   ├── Budget.cs
│   ├── Category.cs
│   ├── Goal.cs
│   ├── RecurringTransaction.cs
│   └── Transaction.cs
├── PersonalFinanceManager.csproj
├── PersonalFinanceManagerDb
├── PersonalFinanceManager.http
├── Program.cs
├── Properties
│   └── launchSettings.json
├── Services
│   ├── AuthService.cs
│   ├── TokenService.cs
│   └── UserService.cs
