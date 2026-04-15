namespace Application.Auth.Configuration;

public class SeedUserOptions
{
    public const string SectionName = "SeedUser";

    public string Username { get; set; } = "admin";

    public string Password { get; set; } = "Admin123!";

    public string Role { get; set; } = "Admin";
}
