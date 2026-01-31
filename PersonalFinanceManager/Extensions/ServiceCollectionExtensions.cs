using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceManager.Configurations;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Interfaces;
using PersonalFinanceManager.Models;

namespace PersonalFinanceManager.Extensions;

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
}