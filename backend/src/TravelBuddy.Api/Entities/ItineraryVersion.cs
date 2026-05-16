namespace TravelBuddy.Api.Entities;

public class ItineraryVersion
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public int VersionNumber { get; set; }
    public string PlanMarkdown { get; set; } = string.Empty;
    public string? ExportPdfPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Trip Trip { get; set; } = null!;
}
