namespace TravelBuddy.Api.Entities;

public class UserProfile
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? HomeCountryCode { get; set; }
    public string? PassportCountryCode { get; set; }
    public string? Bio { get; set; }

    public User User { get; set; } = null!;
}
