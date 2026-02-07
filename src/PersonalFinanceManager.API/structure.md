PersonalFinanceManager
├── src
│   ├── PersonalFinanceManager.API
│   │   ├── Controllers
│   │   │   ├── AccountsController.cs
│   │   │   ├── AuthController.cs
│   │   │   ├── ConfigController.cs
│   │   │   ├── TransactionsController.cs
│   │   │   └── UsersController.cs
│   │   ├── Extensions
│   │   │   └── ServiceCollectionExtensions.cs
│   │   ├── Filters
│   │   ├── Middleware
│   │   ├── Dockerfile
│   │   ├── PersonalFinanceManager.API.csproj
│   │   ├── PersonalFinanceManager.http
│   │   ├── Program.cs
│   │   ├── Properties
│   │   │   └── launchSettings.json
│   │   ├── appsettings.Development.json
│   │   ├── appsettings.json
│   │   ├── LitePFMDatabase.db
│   │   ├── LitePFMDatabase-log.db
│   │   └── PersonalFinanceManagerDb
│   ├── PersonalFinanceManager.Application
│   │   ├── DTOs
│   │   ├── Interfaces
│   │   ├── Services
│   │   └── PersonalFinanceManager.Application.csproj
│   ├── PersonalFinanceManager.Core
│   │   ├── Configurations
│   │   ├── Entities
│   │   ├── Enums
│   │   ├── Interfaces
│   │   ├── Specifications
│   │   └── PersonalFinanceManager.Core.csproj
│   └── PersonalFinanceManager.Infrastructure
│       ├── Data
│       │   ├── Configurations
│       │   ├── Context
│       │   └── Migrations
│       └── PersonalFinanceManager.Infrastructure.csproj
└── tests
    ├── PersonalFinanceManager.UnitTests
    └── PersonalFinanceManager.IntegrationTests
