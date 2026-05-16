namespace TravelBuddy.Api.Entities;

public class Trip
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DestinationCountryCode { get; set; } = string.Empty;
    public string DestinationCountryName { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int PartySize { get; set; } = 1;
    public BudgetBand BudgetBand { get; set; }
    public EnergyLevel EnergyLevel { get; set; }
    public VisaIntent VisaIntent { get; set; }
    public string PreferencesJson { get; set; } = "[]";
    public string? PersonalNotes { get; set; }
    public string Status { get; set; } = "draft";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public VisaGuidance? VisaGuidance { get; set; }
    public ICollection<TripCandidate> Candidates { get; set; } = new List<TripCandidate>();
    public ICollection<TripDecision> Decisions { get; set; } = new List<TripDecision>();
    public ICollection<ItineraryVersion> ItineraryVersions { get; set; } = new List<ItineraryVersion>();
}
