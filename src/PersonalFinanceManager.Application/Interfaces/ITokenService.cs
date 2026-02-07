using System.Security.Claims;
using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Application.Interfaces;

/// <summary>
/// Interface for JWT token generation and validation
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}