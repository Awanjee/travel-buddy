namespace TravelBuddy.Api.Entities;

/// <summary>Phase 2 — schema only; no UI in Phase 1.</summary>
public class TravelerProfile
{
    public Guid UserId { get; set; }
    public bool IsDiscoverable { get; set; }
    public string? PublicBio { get; set; }
    public string InterestsJson { get; set; } = "[]";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}

public class ConnectionRequest
{
    public Guid Id { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public Guid? TripId { get; set; }
    public string? Message { get; set; }
    public ConnectionStatus Status { get; set; } = ConnectionStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User FromUser { get; set; } = null!;
    public User ToUser { get; set; } = null!;
}

public class SwipeEvent
{
    public Guid Id { get; set; }
    public Guid ActorUserId { get; set; }
    public Guid TargetUserId { get; set; }
    public SwipeDirection Direction { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User ActorUser { get; set; } = null!;
    public User TargetUser { get; set; } = null!;
}
