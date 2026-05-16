namespace TravelBuddy.Api.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UserProfile? Profile { get; set; }
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public TravelerProfile? TravelerProfile { get; set; }
}
