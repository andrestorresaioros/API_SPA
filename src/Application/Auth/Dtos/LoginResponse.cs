namespace Application.Auth.Dtos;

public class LoginResponse
{
    public required string AccessToken { get; init; }

    public required DateTime ExpiresAtUtc { get; init; }

    public required string Username { get; init; }

    public required string Role { get; init; }
}
