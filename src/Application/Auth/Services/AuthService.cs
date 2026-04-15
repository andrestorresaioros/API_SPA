using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Auth.Configuration;
using Application.Auth.Dtos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly JwtOptions _jwtOptions;
    private readonly SeedUserOptions _seedUserOptions;

    public AuthService(IOptions<JwtOptions> jwtOptions, IOptions<SeedUserOptions> seedUserOptions)
    {
        _jwtOptions = jwtOptions.Value;
        _seedUserOptions = seedUserOptions.Value;
    }

    public Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();

        var validCredentials = string.Equals(username, _seedUserOptions.Username, StringComparison.Ordinal)
            && string.Equals(request.Password, _seedUserOptions.Password, StringComparison.Ordinal);

        if (!validCredentials)
        {
            return Task.FromResult<LoginResponse?>(null);
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes);
        var token = BuildToken(username, _seedUserOptions.Role, expiresAt);

        return Task.FromResult<LoginResponse?>(new LoginResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAt,
            Username = username,
            Role = _seedUserOptions.Role
        });
    }

    private string BuildToken(string username, string role, DateTime expiresAt)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_jwtOptions.Key);

        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException("JWT key must have at least 32 characters.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
