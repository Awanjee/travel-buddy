namespace TravelBuddy.Api.Entities;

public class TripCandidate
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public CandidateType Type { get; set; }
    public CandidateTag Tag { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? PriceEstimateUsd { get; set; }
    public int Score { get; set; }
    public string? BookingSearchQuery { get; set; }
    public string? ExternalUrl { get; set; }

    public Trip Trip { get; set; } = null!;
    public TripDecision? Decision { get; set; }
}
