using System.Net;
using System.Net.Http.Json;
using Application.Auth.Dtos;

namespace IntegrationTests;

public class AuthAndAuthorizationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthAndAuthorizationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        };

        var response = await _client.PostAsJsonAsync("/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.AccessToken));
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "wrong"
        };

        var response = await _client.PostAsJsonAsync("/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostCategory_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var request = new
        {
            categoryName = "SERVIDORES",
            description = "Infraestructura on premise"
        };

        var response = await _client.PostAsJsonAsync("/category", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
