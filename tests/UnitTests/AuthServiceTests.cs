using Application.Auth.Configuration;
using Application.Auth.Dtos;
using Application.Auth.Services;
using Microsoft.Extensions.Options;

namespace UnitTests;

public class AuthServiceTests
{
    private static AuthService CreateService()
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            Key = "test_key_with_minimum_32_chars_12345",
            ExpiresMinutes = 60
        });

        var seedUserOptions = Options.Create(new SeedUserOptions
        {
            Username = "admin",
            Password = "Admin123!",
            Role = "Admin"
        });

        return new AuthService(jwtOptions, seedUserOptions);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var service = CreateService();

        var result = await service.LoginAsync(new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.AccessToken));
        Assert.Equal("admin", result.Username);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenCredentialsAreInvalid()
    {
        var service = CreateService();

        var result = await service.LoginAsync(new LoginRequest
        {
            Username = "admin",
            Password = "wrong"
        });

        Assert.Null(result);
    }
}
