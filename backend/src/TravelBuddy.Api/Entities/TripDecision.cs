namespace TravelBuddy.Api.Entities;

public class TripDecision
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Guid CandidateId { get; set; }
    public DecisionType Decision { get; set; }
    public DateTime DecidedAt { get; set; } = DateTime.UtcNow;

    public Trip Trip { get; set; } = null!;
    public TripCandidate Candidate { get; set; } = null!;
}
