using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Auth.Dtos;
using Application.Categories.Dtos;
using Application.Products.Dtos;

namespace IntegrationTests;

public class BulkProductsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BulkProductsTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task BulkCreate_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var request = new BulkCreateProductsRequest
        {
            Quantity = 1000,
            BatchSize = 500
        };

        var response = await _client.PostAsJsonAsync("/products/bulk", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task BulkCreate_ShouldCreateProducts_WhenTokenIsValid()
    {
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var categories = await _client.GetFromJsonAsync<List<CategoryResponse>>("/category");

        Assert.NotNull(categories);
        Assert.NotEmpty(categories!);

        var request = new BulkCreateProductsRequest
        {
            Quantity = 200,
            BatchSize = 100,
            CategoryIds = categories!.Select(category => category.Id).ToArray(),
            UseRoundRobin = true
        };

        var response = await _client.PostAsJsonAsync("/products/bulk", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<BulkCreateProductsResponse>();

        Assert.NotNull(payload);
        Assert.Equal(200, payload!.RequestedCount);
        Assert.Equal(200, payload.CreatedCount);
        Assert.Equal(100, payload.BatchSize);
        Assert.True(payload.ElapsedMilliseconds >= 0);
        Assert.Equal(request.CategoryIds.Count, payload.CategoriesUsed.Count);

        var productsResponse = await _client.GetAsync("/products?pageNumber=1&pageSize=1");
        productsResponse.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await productsResponse.Content.ReadAsStringAsync());
        var totalCount = document.RootElement.GetProperty("totalCount").GetInt32();

        Assert.Equal(200, totalCount);
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        };

        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();

        return payload!.AccessToken;
    }
}
